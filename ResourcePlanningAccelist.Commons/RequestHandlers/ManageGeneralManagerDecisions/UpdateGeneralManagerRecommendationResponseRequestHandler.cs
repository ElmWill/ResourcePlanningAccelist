using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerDecisions;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerDecisions;
using ResourcePlanningAccelist.Entities;
using System.Text.Json;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageGeneralManagerDecisions;

public class UpdateGeneralManagerRecommendationResponseRequestHandler : IRequestHandler<UpdateGeneralManagerRecommendationResponseRequest, UpdateGeneralManagerRecommendationResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateGeneralManagerRecommendationResponseRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateGeneralManagerRecommendationResponse> Handle(
        UpdateGeneralManagerRecommendationResponseRequest request,
        CancellationToken cancellationToken)
    {
        var decisionType = request.RecommendationType.ToLowerInvariant() switch
        {
            "adjust-timeline" => DecisionType.ExtendContract,
            "reallocate" => DecisionType.ProjectAssignment,
            "add-resource" => DecisionType.ProjectAssignment,
            _ => (DecisionType?)null,
        } ?? DecisionType.HireResource;

        var targetStatus = request.Action.Equals("Applied", StringComparison.OrdinalIgnoreCase)
            ? (decisionType == DecisionType.ProjectAssignment ? DecisionStatus.Pending : DecisionStatus.Executed)
            : request.Action.Equals("Rejected", StringComparison.OrdinalIgnoreCase)
                ? DecisionStatus.ClarificationRequested
                : throw new InvalidOperationException("Invalid recommendation action.");

        var recommendationMarker = $"[recommendation:{request.RecommendationId}]";

        var decision = await _dbContext.GmDecisions
            .Include(item => item.AffectedEmployees)
            .FirstOrDefaultAsync(
                item => item.ProjectId == request.ProjectId && item.Details.Contains(recommendationMarker),
                cancellationToken);

        if (decision == null)
        {
            decision = new GmDecision
            {
                ProjectId = request.ProjectId,
                DecisionType = decisionType,
                Title = request.Title,
                Details = $"{recommendationMarker} {request.Details}".Trim(),
                SubmittedAt = DateTimeOffset.UtcNow,
            };

            _dbContext.GmDecisions.Add(decision);
        }
        else
        {
            decision.Title = request.Title;
            decision.Details = $"{recommendationMarker} {request.Details}".Trim();
            decision.DecisionType = decisionType;
        }

        // Link Affected Employees if metadata is available
        if (request.Details.TrimStart().StartsWith("{"))
        {
            try
            {
                using var metadata = JsonDocument.Parse(request.Details);
                if (metadata.RootElement.TryGetProperty("assignment", out var assignment) &&
                    assignment.TryGetProperty("employeeId", out var employeeIdProp) &&
                    Guid.TryParse(employeeIdProp.GetString(), out var empId))
                {
                    if (!decision.AffectedEmployees.Any(ae => ae.EmployeeId == empId))
                    {
                        decision.AffectedEmployees.Add(new GmDecisionEmployee { EmployeeId = empId });
                    }
                }
            }
            catch { /* ignore invalid JSON */ }
        }

        decision.Status = targetStatus;

        if (targetStatus == DecisionStatus.Executed)
        {
            decision.ExecutedAt = DateTimeOffset.UtcNow;
            decision.ClarificationRequestedAt = null;
        }
        else if (targetStatus == DecisionStatus.ClarificationRequested)
        {
            decision.ClarificationRequestedAt = DateTimeOffset.UtcNow;
            decision.ExecutedAt = null;
        }
        else
        {
            decision.ExecutedAt = null;
            decision.ClarificationRequestedAt = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateGeneralManagerRecommendationResponse
        {
            DecisionId = decision.Id,
            Status = decision.Status.ToString(),
            Action = request.Action,
        };
    }
}
