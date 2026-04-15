using MediatR;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageNotifications;

public class MarkNotificationAsReadRequest : IRequest<bool>
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
}
