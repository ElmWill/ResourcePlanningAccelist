namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectRiskResponse
{
    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public decimal StaffingRiskScore { get; set; }

    public decimal CoverageScore { get; set; }

    public string RiskLevel { get; set; } = string.Empty;

    public int RequiredResourceCount { get; set; }

    public int ActiveCandidateCount { get; set; }

    public decimal AverageWorkloadPercent { get; set; }

    public List<string> MainRiskReasons { get; set; } = new();

    public List<GeneralManagerProjectRiskRequirementResponse> Requirements { get; set; } = new();
}
