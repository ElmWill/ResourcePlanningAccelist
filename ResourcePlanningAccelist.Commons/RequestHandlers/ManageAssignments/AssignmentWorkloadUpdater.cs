using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;

internal static class AssignmentWorkloadUpdater
{
    private static readonly AssignmentStatus[] ActiveAssignmentStatuses =
    {
        AssignmentStatus.Pending,
        AssignmentStatus.Approved,
        AssignmentStatus.Accepted,
        AssignmentStatus.InProgress,
    };

    private static readonly ResourcePlanningAccelist.Entities.TaskStatus[] ActiveTaskStatuses =
    {
        ResourcePlanningAccelist.Entities.TaskStatus.Pending,
        ResourcePlanningAccelist.Entities.TaskStatus.InProgress,
    };

    private const decimal WeeklyHoursBaseline = 40m;

    public static async Task RecalculateEmployeeWorkloadAsync(
        ApplicationDbContext dbContext,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(item => item.Id == employeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Employee not found.");

        var allocationPercent = await dbContext.Assignments
            .Where(item => item.EmployeeId == employeeId)
            .Where(item => ActiveAssignmentStatuses.Contains(item.Status))
            .GroupBy(item => item.ProjectId)
            .Select(group => Math.Min(100m, group.Sum(item => item.AllocationPercent)))
            .SumAsync(cancellationToken);

        var taskHours = await dbContext.TaskAssignments
            .Where(item => item.EmployeeId == employeeId)
            .Where(item => ActiveTaskStatuses.Contains(item.Status))
            .SumAsync(item => (decimal)item.WorkloadHours, cancellationToken);

        var taskWorkloadPercent = taskHours <= 0m
            ? 0m
            : (taskHours / WeeklyHoursBaseline) * 100m;

        var normalizedWorkload = Math.Max(0m, allocationPercent + taskWorkloadPercent);
        employee.WorkloadPercent = normalizedWorkload;
        employee.AvailabilityPercent = Math.Max(0m, 100m - normalizedWorkload);
        var assignmentHours = (allocationPercent / 100m) * WeeklyHoursBaseline;
        employee.AssignedHours = Math.Round(assignmentHours + taskHours, 2, MidpointRounding.AwayFromZero);

        employee.WorkloadState = normalizedWorkload switch
        {
            <= 30m => WorkloadStatus.Available,
            <= 70m => WorkloadStatus.Moderate,
            <= 100m => WorkloadStatus.Busy,
            _ => WorkloadStatus.Overloaded,
        };
    }
}
