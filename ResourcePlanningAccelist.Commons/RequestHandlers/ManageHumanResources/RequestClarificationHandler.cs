using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageHumanResources;

public class RequestClarificationHandler : IRequestHandler<RequestClarificationRequest, RequestClarificationResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public RequestClarificationHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RequestClarificationResponse> Handle(RequestClarificationRequest request, CancellationToken cancellationToken)
    {
        var decision = await _dbContext.GmDecisions
            .FirstOrDefaultAsync(item => item.Id == request.DecisionId, cancellationToken);

        if (decision == null)
        {
            return new RequestClarificationResponse
            {
                Success = false,
                Message = "GM decision not found."
            };
        }

        decision.Status = DecisionStatus.ClarificationRequested;
        decision.ClarificationRequestedAt = DateTimeOffset.UtcNow;
        decision.ClarificationReason = request.Reason;

        // Notify the General Manager who submitted the decision
        if (decision.SubmittedByUserId.HasValue)
        {
            _dbContext.Notifications.Add(new Notification
            {
                UserId = decision.SubmittedByUserId.Value,
                Type = NotificationType.Alert,
                Title = "Clarification Requested",
                Message = $"HR has requested clarification on your decision: '{decision.Title}'. Reason: {request.Reason}",
                CreatedAt = DateTimeOffset.UtcNow,
                IsRead = false,
                SourceEntityType = "GmDecision",
                SourceEntityId = decision.ProjectId ?? decision.Id
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RequestClarificationResponse
        {
            Success = true,
            Message = "Clarification requested successfully."
        };
    }
}
