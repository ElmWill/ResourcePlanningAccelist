using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;

public class GetUserProfileRequest : IRequest<GetUserProfileResponse>
{
    public Guid UserId { get; set; }
}
