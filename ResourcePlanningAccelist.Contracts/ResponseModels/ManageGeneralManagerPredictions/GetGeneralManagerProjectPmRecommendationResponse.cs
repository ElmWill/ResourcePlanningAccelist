namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectPmRecommendationResponse
{
    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public int CandidateLimit { get; set; }

    public int CandidatePoolSize { get; set; }

    public bool HasPmRequirement { get; set; }

    public bool PmRequirementAlreadyFull { get; set; }

    public GeneralManagerProjectCandidatePredictionResponse? RecommendedPm { get; set; }

    public List<GeneralManagerProjectCandidatePredictionResponse> Candidates { get; set; } = new();
}
