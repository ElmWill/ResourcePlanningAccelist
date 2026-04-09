namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class GetProjectManagerProjectActivityResponse
{
    public Guid ProjectId { get; set; }

    public List<ProjectManagerActivityItemResponse> Activities { get; set; } = new();
}