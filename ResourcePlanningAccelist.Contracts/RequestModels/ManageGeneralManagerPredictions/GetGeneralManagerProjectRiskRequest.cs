using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectRiskRequest : IRequest<GetGeneralManagerProjectRiskResponse>
{
    public Guid ProjectId { get; set; }

    public int? CandidateLimit { get; set; }
}
