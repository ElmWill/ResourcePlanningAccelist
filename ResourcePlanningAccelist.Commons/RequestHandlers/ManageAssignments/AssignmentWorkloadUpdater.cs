using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;

public static class AssignmentWorkloadUpdater
{
    private static readonly AssignmentStatus[] ActiveAssignmentStatuses =
    {
        AssignmentStatus.Pending,
        AssignmentStatus.GmApproved,
        AssignmentStatus.Approved,
        AssignmentStatus.Accepted,
        AssignmentStatus.InProgress,
    };

    private static readonly ResourcePlanningAccelist.Entities.TaskStatus[] ActiveTaskStatuses =
    {
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

    public static async Task RecalculateEmployeeWorkloadsAsync(
        ApplicationDbContext dbContext,
        IEnumerable<Guid> employeeIds,
        CancellationToken cancellationToken)
    {
        var empIdList = employeeIds.Distinct().ToList();
        if (!empIdList.Any()) return;

        var employeesToUpdate = await dbContext.Employees
            .Where(e => empIdList.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        if (!employeesToUpdate.Any()) return;

        var allocationPercents = await dbContext.Assignments
            .Where(item => empIdList.Contains(item.EmployeeId))
            .Where(item => ActiveAssignmentStatuses.Contains(item.Status))
            .GroupBy(item => new { item.EmployeeId, item.ProjectId })
            .Select(group => new
            {
                group.Key.EmployeeId,
                Allocation = Math.Min(100m, group.Sum(item => item.AllocationPercent))
            })
            .GroupBy(item => item.EmployeeId)
            .Select(group => new
            {
                EmployeeId = group.Key,
                TotalAllocation = group.Sum(item => item.Allocation)
            })
            .ToDictionaryAsync(item => item.EmployeeId, item => item.TotalAllocation, cancellationToken);

        var taskHours = await dbContext.TaskAssignments
            .Where(item => empIdList.Contains(item.EmployeeId))
            .Where(item => ActiveTaskStatuses.Contains(item.Status))
            .GroupBy(item => item.EmployeeId)
            .Select(group => new
            {
                EmployeeId = group.Key,
                TotalTaskHours = group.Sum(item => (decimal)item.WorkloadHours)
            })
            .ToDictionaryAsync(item => item.EmployeeId, item => item.TotalTaskHours, cancellationToken);

        foreach (var employeeId in employeesToUpdate.Keys)
        {
            var employee = employeesToUpdate[employeeId];
            var allocationPercent = allocationPercents.GetValueOrDefault(employeeId, 0m);
            var totalTaskHours = taskHours.GetValueOrDefault(employeeId, 0m);

            var taskWorkloadPercent = totalTaskHours <= 0m ? 0m : (totalTaskHours / WeeklyHoursBaseline) * 100m;
            var normalizedWorkload = Math.Max(0m, allocationPercent + taskWorkloadPercent);
            
            employee.WorkloadPercent = normalizedWorkload;
            employee.AvailabilityPercent = Math.Max(0m, 100m - normalizedWorkload);
            var assignmentHours = (allocationPercent / 100m) * WeeklyHoursBaseline;
            employee.AssignedHours = Math.Round(assignmentHours + totalTaskHours, 2, MidpointRounding.AwayFromZero);

            employee.WorkloadState = normalizedWorkload switch
            {
                <= 30m => WorkloadStatus.Available,
                <= 70m => WorkloadStatus.Moderate,
                <= 100m => WorkloadStatus.Busy,
                _ => WorkloadStatus.Overloaded,
            };
        }
    }
}
