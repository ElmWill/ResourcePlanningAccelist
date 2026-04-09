namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

public class GeneralManagerProjectRiskRequirementResponse
{
    public Guid RequirementId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal CoverageScore { get; set; }

    public decimal BestCandidateScore { get; set; }

    public string GapSummary { get; set; } = string.Empty;
}
