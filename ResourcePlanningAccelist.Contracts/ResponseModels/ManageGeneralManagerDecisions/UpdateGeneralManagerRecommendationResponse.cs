namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerDecisions;

public class UpdateGeneralManagerRecommendationResponse
{
    public Guid DecisionId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;
}
