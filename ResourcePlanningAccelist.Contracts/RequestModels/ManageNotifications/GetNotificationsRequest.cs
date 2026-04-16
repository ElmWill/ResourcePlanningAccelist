using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageNotifications;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageNotifications;

public class GetNotificationsRequest : IRequest<GetNotificationsResponse>
{
    public Guid UserId { get; set; }
    public int? Limit { get; set; } = 50;
}
