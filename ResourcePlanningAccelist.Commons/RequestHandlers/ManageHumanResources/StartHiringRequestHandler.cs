using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageHumanResources;

public class StartHiringRequestHandler : IRequestHandler<StartHiringRequest, StartHiringResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public StartHiringRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StartHiringResponse> Handle(StartHiringRequest request, CancellationToken cancellationToken)
    {
        var decision = await _dbContext.GmDecisions
            .Include(item => item.Project)
            .FirstOrDefaultAsync(item => item.Id == request.DecisionId, cancellationToken);

        if (decision == null)
        {
            return new StartHiringResponse
            {
                Success = false,
                Message = "Hiring decision not found."
            };
        }

        // 1. Update Decision status to Executed (meaning HR has acted on it)
        decision.Status = DecisionStatus.Executed;
        decision.ExecutedAt = DateTimeOffset.UtcNow;

        // 2. Create the HiringRequest entry (the "tracking record" requested by user)
        var hiringRequest = new HiringRequest
        {
            GmDecisionId = decision.Id,
            JobTitle = decision.Title.Contains("Hire") ? decision.Title.Replace("Hire", "").Trim() : "New Resource",
            Details = decision.Details,
            Status = HiringRequestStatus.InProgress,
            StartedAt = DateTimeOffset.UtcNow
        };
        _dbContext.HiringRequests.Add(hiringRequest);

        // 3. Create a Notification for relevant parties (e.g. Marketing/HR lead)
        // In a real app, we'd target specific users/roles.
        if (decision.SubmittedByUserId.HasValue)
        {
            var notification = new Notification
            {
                UserId = decision.SubmittedByUserId.Value,
                Type = NotificationType.Alert,
                Title = "Hiring Process Started",
                Message = $"The recruitment process for '{hiringRequest.JobTitle}' has been initiated as per your decision for project '{decision.Project?.Name ?? "General"}'.",
                IsRead = false,
                SourceEntityType = nameof(HiringRequest),
                SourceEntityId = hiringRequest.Id
            };
            _dbContext.Notifications.Add(notification);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new StartHiringResponse
        {
            Success = true,
            Message = "Hiring process initiated and tracking record created."
        };
    }
}
