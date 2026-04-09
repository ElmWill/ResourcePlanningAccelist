namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class ProjectTimelineTaskItemResponse
{
    public Guid TimelineTaskId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int StartOffsetDays { get; set; }

    public int DurationDays { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string ColorTag { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}