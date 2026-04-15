using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectPmRecommendationRequest : IRequest<GetGeneralManagerProjectPmRecommendationResponse>
{
    public Guid ProjectId { get; set; }

    public int? CandidateLimit { get; set; }
}
