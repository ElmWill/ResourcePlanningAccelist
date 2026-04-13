namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers;

public class GetUserProfileResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }
}
