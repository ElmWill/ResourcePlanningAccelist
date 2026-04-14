using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;

public class LoginRequest : IRequest<LoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
