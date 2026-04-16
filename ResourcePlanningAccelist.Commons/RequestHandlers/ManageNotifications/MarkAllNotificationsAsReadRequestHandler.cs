using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageNotifications;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageNotifications;

public class MarkAllNotificationsAsReadRequestHandler : IRequestHandler<MarkAllNotificationsAsReadRequest, bool>
{
    private readonly ApplicationDbContext _dbContext;

    public MarkAllNotificationsAsReadRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(MarkAllNotificationsAsReadRequest request, CancellationToken cancellationToken)
    {
        var unreadNotifications = await _dbContext.Notifications
            .Where(item => item.UserId == request.UserId && !item.IsRead)
            .ToListAsync(cancellationToken);

        if (unreadNotifications.Count == 0)
        {
            return true;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
