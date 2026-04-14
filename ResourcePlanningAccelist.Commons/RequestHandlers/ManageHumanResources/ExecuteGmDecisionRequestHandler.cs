using System;
using System.Linq;
using System.Text.Json;
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
            if (decision.DecisionType == DecisionType.ProjectAssignment && decision.Status == DecisionStatus.Pending)
            {
                // Delayed creation logic: Parse the JSON metadata stored in Details
                try
                {
                    var jsonStart = decision.Details.IndexOf('{');
                    if (jsonStart >= 0)
                    {
                        var json = decision.Details.Substring(jsonStart);
                        using var metadata = JsonDocument.Parse(json);
                        if (metadata.RootElement.TryGetProperty("assignment", out var assignmentData))
                        {
                            var empId = assignmentData.GetProperty("employeeId").GetGuid();
                            var roleName = assignmentData.GetProperty("roleName").GetString() ?? "Resource";
                            var startDate = DateOnly.Parse(assignmentData.GetProperty("startDate").GetString()!);
                            var endDate = DateOnly.Parse(assignmentData.GetProperty("endDate").GetString()!);
                            var allocation = assignmentData.GetProperty("allocationPercent").GetDecimal();
                            var skills = assignmentData.GetProperty("requiredSkills").EnumerateArray()
                                .Select(s => s.GetString() ?? string.Empty)
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList();

                            var newAssignment = new Assignment
                            {
                                ProjectId = decision.ProjectId ?? Guid.Empty,
                                EmployeeId = empId,
                                AssignedByUserId = decision.SubmittedByUserId,
                                RoleName = roleName,
                                StartDate = startDate,
                                EndDate = endDate,
                                AllocationPercent = 0.0m, // GM assigns resource at 0% workload; PM will refine later.
                                Status = AssignmentStatus.Approved,
                                AcceptedAt = DateTimeOffset.UtcNow,
                                ConflictWarning = $"Auto-created from GM decision execution ({decision.Title}).",
                            };

                            _dbContext.Assignments.Add(newAssignment);

                            // Notify Employee
                            _dbContext.Notifications.Add(new Notification
                            {
                                UserId = await _dbContext.Employees.Where(e => e.Id == empId).Select(e => e.UserId).FirstAsync(cancellationToken),
                                Type = NotificationType.Assignment,
                                Title = "New Management Assignment",
                                Message = $"A GM-approved assignment for project '{decision.Title}' has been finalized and assigned to you.",
                                CreatedAt = DateTimeOffset.UtcNow,
                                IsRead = false,
                                SourceEntityType = "Assignment",
                                SourceEntityId = newAssignment.Id
                            });

                            // Save assignment first so WorkloadHelper can see it
                            await _dbContext.SaveChangesAsync(cancellationToken);

                            // Recalculate Workload
                            await WorkloadHelper.RecalculateEmployeeWorkloadAsync(empId, _dbContext, cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If JSON parsing fails, we still mark the decision as executed but log/handle the failure
                    // In a production app, we would use a logging framework here.
                    Console.WriteLine($"Failed to create assignment from GM decision JSON metadata: {ex.Message}");
                }
            }

            decision.Status = DecisionStatus.Executed;
            decision.ExecutedAt = DateTimeOffset.UtcNow;

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
