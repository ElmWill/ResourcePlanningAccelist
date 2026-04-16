using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageNotifications;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageNotifications;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageNotifications;

public class GetNotificationsRequestHandler : IRequestHandler<GetNotificationsRequest, GetNotificationsResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetNotificationsRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetNotificationsResponse> Handle(GetNotificationsRequest request, CancellationToken cancellationToken)
    {
        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Where(item => item.UserId == request.UserId)
            .OrderByDescending(item => !item.IsRead)
            .ThenByDescending(item => item.CreatedAt)
            .Take(request.Limit ?? 50)
            .Select(item => new NotificationItemResponse
            {
                Id = item.Id,
                Type = item.Type.ToString().ToLower(),
                Title = item.Title,
                Message = item.Message,
                CreatedAt = item.CreatedAt,
                IsRead = item.IsRead,
                SourceEntityType = item.SourceEntityType,
                SourceEntityId = item.SourceEntityId
            })
            .ToListAsync(cancellationToken);

        return new GetNotificationsResponse
        {
            Notifications = notifications
        };
    }
}
