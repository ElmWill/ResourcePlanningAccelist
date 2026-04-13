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

        var normalizedWorkload = Math.Max(0m, allocationPercent);
        employee.WorkloadPercent = normalizedWorkload;
        employee.AvailabilityPercent = Math.Max(0m, 100m - normalizedWorkload);
        employee.AssignedHours = Math.Round((normalizedWorkload / 100m) * WeeklyHoursBaseline, 2, MidpointRounding.AwayFromZero);

        employee.WorkloadState = normalizedWorkload switch
        {
            <= 30m => WorkloadStatus.Available,
            <= 70m => WorkloadStatus.Moderate,
            <= 100m => WorkloadStatus.Busy,
            _ => WorkloadStatus.Overloaded,
        };
    }
}
