namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

public class GeneralManagerProjectRequirementPredictionResponse
{
    public Guid RequirementId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string ExperienceLevel { get; set; } = string.Empty;

    public List<string> RequiredSkills { get; set; } = new();

    public decimal CoverageScore { get; set; }

    public decimal BestCandidateScore { get; set; }

    public List<GeneralManagerProjectCandidatePredictionResponse> RecommendedCandidates { get; set; } = new();
}