using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerDecisions;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerDecisions;

public class UpdateGeneralManagerRecommendationResponseRequest : IRequest<UpdateGeneralManagerRecommendationResponse>
{
    public Guid ProjectId { get; set; }

    public string RecommendationId { get; set; } = string.Empty;

    public string RecommendationType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;
}
