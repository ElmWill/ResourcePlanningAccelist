namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class GetProjectTimelineTaskListResponse
{
    public Guid ProjectId { get; set; }

    public List<ProjectTimelineTaskItemResponse> Tasks { get; set; } = new();
}