using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;
using ResourcePlanningAccelist.Entities;
using ResourcePlanningAccelist.Commons.Helpers;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageHumanResources;

public class ExecuteGmDecisionRequestHandler : IRequestHandler<ExecuteGmDecisionRequest, ExecuteGmDecisionResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public ExecuteGmDecisionRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExecuteGmDecisionResponse> Handle(ExecuteGmDecisionRequest request, CancellationToken cancellationToken)
    {
        // Try searching in GmDecisions first
        var decision = await _dbContext.GmDecisions
            .FirstOrDefaultAsync(item => item.Id == request.DecisionId, cancellationToken);

        if (decision != null)
        {
            decision.Status = DecisionStatus.Executed;
            decision.ExecutedAt = DateTimeOffset.UtcNow;
            // In a real app, we'd get the current user ID from the context
            // decision.ExecutedByUserId = ... 

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ExecuteGmDecisionResponse
            {
                Success = true,
                Message = "GM decision executed successfully."
            };
        }

        // If not found in GmDecisions, try searching in Assignments (for ProjectAssignment type)
        var assignment = await _dbContext.Assignments
            .FirstOrDefaultAsync(item => item.Id == request.DecisionId, cancellationToken);

        if (assignment != null)
        {
            assignment.Status = AssignmentStatus.Approved;
            assignment.AcceptedAt = DateTimeOffset.UtcNow;

            // Notify Employee
            _dbContext.Notifications.Add(new Notification
            {
                UserId = await _dbContext.Employees.Where(e => e.Id == assignment.EmployeeId).Select(e => e.UserId).FirstAsync(cancellationToken),
                Type = NotificationType.Assignment,
                Title = "New Project Assignment",
                Message = $"You have been assigned to project '{await _dbContext.Projects.Where(p => p.Id == assignment.ProjectId).Select(p => p.Name).FirstAsync(cancellationToken)}'. Please review and accept.",
                CreatedAt = DateTimeOffset.UtcNow,
                IsRead = false,
                SourceEntityType = "Assignment",
                SourceEntityId = assignment.Id
            });

            // SAVE FIRST so the recalculation query can see the new status
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Recalculate Workload
            await WorkloadHelper.RecalculateEmployeeWorkloadAsync(assignment.EmployeeId, _dbContext, cancellationToken);
            
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ExecuteGmDecisionResponse
            {
                Success = true,
                Message = "Project assignment approved successfully."
            };
        }

        return new ExecuteGmDecisionResponse
        {
            Success = false,
            Message = "Decision or Assignment not found."
        };
    }
}
