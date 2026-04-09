namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectPredictionResponse
{
    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public decimal OverallCoverageScore { get; set; }

    public decimal StaffingRiskScore { get; set; }

    public int RequiredResourceCount { get; set; }

    public int CandidatePoolSize { get; set; }

    public int CandidateLimit { get; set; }

    public List<GeneralManagerProjectRequirementPredictionResponse> Requirements { get; set; } = new();
}