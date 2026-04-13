namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers;

public class UpdateUserProfileResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
