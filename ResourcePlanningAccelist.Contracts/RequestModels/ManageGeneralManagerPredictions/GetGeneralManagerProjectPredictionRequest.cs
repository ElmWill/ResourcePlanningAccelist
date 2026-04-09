using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectPredictionRequest : IRequest<GetGeneralManagerProjectPredictionResponse>
{
    public Guid ProjectId { get; set; }

    public int? CandidateLimit { get; set; }
}