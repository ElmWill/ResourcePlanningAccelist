using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.Helpers;

public static class WorkloadHelper
{
    public static async Task RecalculateEmployeeWorkloadAsync(Guid employeeId, ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees
            .Include(e => e.Assignments)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee == null) return;

        // Count pending and active assignments towards current workload so project assignment impact is immediate
        var activeAssignments = employee.Assignments
            .Where(a => a.Status == AssignmentStatus.Pending ||
                        a.Status == AssignmentStatus.Approved || 
                        a.Status == AssignmentStatus.Accepted || 
                        a.Status == AssignmentStatus.InProgress)
            .ToList();

        decimal totalAllocation = activeAssignments.Sum(a => a.AllocationPercent);
        
        // standard 8-hour work day
        decimal totalHours = totalAllocation * 0.08m; 

        employee.WorkloadPercent = totalAllocation;
        employee.AvailabilityPercent = Math.Max(0, 100 - employee.WorkloadPercent);
        employee.AssignedHours = totalHours;

        employee.WorkloadState = employee.WorkloadPercent switch
        {
            <= 0 => WorkloadStatus.Available,
            <= 30 => WorkloadStatus.Available,
            <= 70 => WorkloadStatus.Moderate,
            <= 100 => WorkloadStatus.Busy,
            _ => WorkloadStatus.Overloaded
        };
    }
}
