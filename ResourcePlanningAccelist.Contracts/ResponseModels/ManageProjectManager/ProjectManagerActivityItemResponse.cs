namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class ProjectManagerActivityItemResponse
{
    public DateTimeOffset OccurredAt { get; set; }

    public string Message { get; set; } = string.Empty;
}