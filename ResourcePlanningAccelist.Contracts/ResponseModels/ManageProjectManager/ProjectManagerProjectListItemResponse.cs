namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class ProjectManagerProjectListItemResponse
{
    public Guid ProjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ClientName { get; set; }

    public string Status { get; set; } = string.Empty;

    public int ProgressPercent { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int TeamSize { get; set; }
}