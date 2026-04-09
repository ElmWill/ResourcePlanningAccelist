namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

public class GeneralManagerProjectCandidatePredictionResponse
{
    public Guid EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? DepartmentName { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public decimal FitScore { get; set; }

    public decimal SkillScore { get; set; }

    public decimal CapacityScore { get; set; }

    public decimal HistoryScore { get; set; }

    public decimal RoleExperienceScore { get; set; }

    public decimal AvailabilityPercent { get; set; }

    public decimal WorkloadPercent { get; set; }

    public List<string> MatchedSkills { get; set; } = new();

    public List<string> InferredSkills { get; set; } = new();

    public List<string> MissingSkills { get; set; } = new();

    public string Reason { get; set; } = string.Empty;
}