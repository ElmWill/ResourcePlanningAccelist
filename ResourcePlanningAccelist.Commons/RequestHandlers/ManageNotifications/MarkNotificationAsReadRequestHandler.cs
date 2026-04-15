using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageNotifications;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageNotifications;

public class MarkNotificationAsReadRequestHandler : IRequestHandler<MarkNotificationAsReadRequest, bool>
{
    private readonly ApplicationDbContext _dbContext;

    public MarkNotificationAsReadRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(MarkNotificationAsReadRequest request, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(item => item.Id == request.NotificationId && item.UserId == request.UserId, cancellationToken);

        if (notification == null)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
