using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;

public class UpdateUserProfileRequest : IRequest<UpdateUserProfileResponse>
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
