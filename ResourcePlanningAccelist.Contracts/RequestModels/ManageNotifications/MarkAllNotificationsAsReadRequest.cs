using MediatR;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageNotifications;

public class MarkAllNotificationsAsReadRequest : IRequest<bool>
{
    public Guid UserId { get; set; }
}
