using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerDecisions;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerDecisions;
using ResourcePlanningAccelist.Entities;

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
        var targetStatus = request.Action.Equals("Applied", StringComparison.OrdinalIgnoreCase)
            ? DecisionStatus.Executed
            : request.Action.Equals("Rejected", StringComparison.OrdinalIgnoreCase)
                ? DecisionStatus.ClarificationRequested
                : throw new InvalidOperationException("Invalid recommendation action.");

        var decisionType = request.RecommendationType.ToLowerInvariant() switch
        {
            "adjust-timeline" => DecisionType.ExtendContract,
            "reallocate" => DecisionType.HireResource,
            "add-resource" => DecisionType.HireResource,
            _ => DecisionType.HireResource,
        };

        var recommendationMarker = $"[recommendation:{request.RecommendationId}]";

        var decision = await _dbContext.GmDecisions
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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateGeneralManagerRecommendationResponse
        {
            DecisionId = decision.Id,
            Status = decision.Status.ToString(),
            Action = request.Action,
        };
    }
}
