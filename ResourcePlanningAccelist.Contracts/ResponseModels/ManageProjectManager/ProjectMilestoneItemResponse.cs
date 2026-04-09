namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class ProjectMilestoneItemResponse
{
    public Guid MilestoneId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly DueDate { get; set; }

    public bool IsCompleted { get; set; }

    public int SortOrder { get; set; }
}