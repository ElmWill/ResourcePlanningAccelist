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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RequestClarificationResponse
        {
            Success = true,
            Message = "Clarification requested successfully."
        };
    }
}
