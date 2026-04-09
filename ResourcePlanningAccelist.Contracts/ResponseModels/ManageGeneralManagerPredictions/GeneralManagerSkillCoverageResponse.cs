namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

public class GeneralManagerSkillCoverageResponse
{
    public Guid SkillId { get; set; }

    public string SkillName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int EmployeeCount { get; set; }

    public decimal CoveragePercent { get; set; }
}
