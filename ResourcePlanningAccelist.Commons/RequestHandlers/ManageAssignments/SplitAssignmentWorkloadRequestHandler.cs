using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;

public class SplitAssignmentWorkloadRequestHandler : IRequestHandler<SplitAssignmentWorkloadRequest, SplitAssignmentWorkloadResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public SplitAssignmentWorkloadRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SplitAssignmentWorkloadResponse> Handle(SplitAssignmentWorkloadRequest request, CancellationToken cancellationToken)
    {
        var activeStatuses = new[]
        {
            AssignmentStatus.Accepted,
            AssignmentStatus.Approved,
            AssignmentStatus.InProgress,
            AssignmentStatus.Pending,
        };

        var projectExists = await _dbContext.Projects.AnyAsync(project => project.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new InvalidOperationException("Project does not exist.");
        }

        var sourceAssignment = await _dbContext.Assignments
            .Where(assignment => assignment.ProjectId == request.ProjectId)
            .Where(assignment => assignment.EmployeeId == request.FromEmployeeId)
            .Where(assignment => activeStatuses.Contains(assignment.Status))
            .Where(assignment => assignment.AllocationPercent > 0)
            .OrderByDescending(assignment => assignment.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Source assignment has no active positive allocation available to split.");

        var targetEmployee = await _dbContext.Employees
            .AsNoTracking()
            .Include(employee => employee.Contract)
            .FirstOrDefaultAsync(employee => employee.Id == request.ToEmployeeId, cancellationToken)
            ?? throw new InvalidOperationException("Target employee was not found.");

        if (targetEmployee.Status != EmploymentStatus.Active)
        {
            throw new InvalidOperationException("Target employee is not active and cannot accept split workload.");
        }

        if (targetEmployee.Contract is not null && targetEmployee.Contract.Status is not ContractStatus.Active and not ContractStatus.Extended)
        {
            throw new InvalidOperationException("Target employee contract is not active for workload split.");
        }

        var targetAssignment = await _dbContext.Assignments
            .Where(assignment => assignment.ProjectId == request.ProjectId)
            .Where(assignment => assignment.EmployeeId == request.ToEmployeeId)
            .Where(assignment => activeStatuses.Contains(assignment.Status))
            .OrderByDescending(assignment => assignment.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var targetCurrentAllocation = await _dbContext.Assignments
            .Where(assignment => assignment.EmployeeId == request.ToEmployeeId)
            .Where(assignment => activeStatuses.Contains(assignment.Status))
            .SumAsync(assignment => assignment.AllocationPercent, cancellationToken);

        var targetCapacityLeft = Math.Max(0m, 100m - targetCurrentAllocation);
        if (targetCapacityLeft <= 0m)
        {
            throw new InvalidOperationException("Target employee assignment is already at 100% allocation.");
        }

        if (targetCapacityLeft < request.SplitAllocationPercent)
        {
            throw new InvalidOperationException($"Target employee has only {targetCapacityLeft}% capacity left, below requested split of {request.SplitAllocationPercent}%.");
        }

        var appliedSplit = Math.Min(request.SplitAllocationPercent, sourceAssignment.AllocationPercent);
        if (appliedSplit <= 0m)
        {
            throw new InvalidOperationException("No transferable allocation could be applied.");
        }

        sourceAssignment.AllocationPercent -= appliedSplit;

        if (targetAssignment is null)
        {
            targetAssignment = new Assignment
            {
                ProjectId = sourceAssignment.ProjectId,
                EmployeeId = request.ToEmployeeId,
                AssignedByUserId = request.AssignedByUserId ?? sourceAssignment.AssignedByUserId,
                RoleName = string.IsNullOrWhiteSpace(request.RoleName) ? sourceAssignment.RoleName : request.RoleName.Trim(),
                StartDate = request.StartDate ?? sourceAssignment.StartDate,
                EndDate = request.EndDate ?? sourceAssignment.EndDate,
                AllocationPercent = appliedSplit,
                Priority = sourceAssignment.Priority,
                Status = sourceAssignment.Status,
                ConflictWarning = sourceAssignment.ConflictWarning,
            };

            _dbContext.Assignments.Add(targetAssignment);
        }
        else
        {
            targetAssignment.AllocationPercent += appliedSplit;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await AssignmentWorkloadUpdater.RecalculateEmployeeWorkloadAsync(_dbContext, sourceAssignment.EmployeeId, cancellationToken);
        await AssignmentWorkloadUpdater.RecalculateEmployeeWorkloadAsync(_dbContext, targetAssignment.EmployeeId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SplitAssignmentWorkloadResponse
        {
            SourceAssignmentId = sourceAssignment.Id,
            SourceAllocationPercent = sourceAssignment.AllocationPercent,
            TargetAssignmentId = targetAssignment.Id,
            TargetAllocationPercent = targetAssignment.AllocationPercent,
            AppliedSplitPercent = appliedSplit,
        };
    }
}
