namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class GetProjectManagerProjectOverviewResponse
{
    public Guid ProjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int ProgressPercent { get; set; }

    public string Status { get; set; } = string.Empty;

    public string RiskLevel { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int DaysRemaining { get; set; }

    public int TeamSize { get; set; }

    public int TotalAssignments { get; set; }

    public int CompletedAssignments { get; set; }
}