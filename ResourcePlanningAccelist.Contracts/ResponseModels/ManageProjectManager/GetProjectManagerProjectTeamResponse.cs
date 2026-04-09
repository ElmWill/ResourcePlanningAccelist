namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class GetProjectManagerProjectTeamResponse
{
    public Guid ProjectId { get; set; }

    public List<ProjectManagerTeamMemberItemResponse> TeamMembers { get; set; } = new();
}