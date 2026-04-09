namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class GetProjectMilestoneListResponse
{
    public Guid ProjectId { get; set; }

    public List<ProjectMilestoneItemResponse> Milestones { get; set; } = new();
}