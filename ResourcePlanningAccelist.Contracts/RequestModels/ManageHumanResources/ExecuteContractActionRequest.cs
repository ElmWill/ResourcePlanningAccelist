using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;

public class ExecuteContractActionRequest : IRequest<ExecuteContractActionResponse>
{
    public Guid DecisionId { get; set; }
}
