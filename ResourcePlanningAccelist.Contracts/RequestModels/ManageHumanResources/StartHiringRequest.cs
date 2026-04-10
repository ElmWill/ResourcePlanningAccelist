using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;

public class StartHiringRequest : IRequest<StartHiringResponse>
{
    public Guid DecisionId { get; set; }
}
