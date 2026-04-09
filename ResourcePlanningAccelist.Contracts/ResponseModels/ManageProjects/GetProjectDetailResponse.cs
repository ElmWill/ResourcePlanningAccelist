namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

public class GetProjectDetailResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ClientName { get; set; }

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;

    public int ProgressPercent { get; set; }

    public string RiskLevel { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}