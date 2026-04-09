namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class UpdateProjectTimelineTaskResponse
{
    public Guid TimelineTaskId { get; set; }

    public string Status { get; set; } = string.Empty;
}