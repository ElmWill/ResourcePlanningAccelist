using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;

public class ExecuteGmDecisionRequest : IRequest<ExecuteGmDecisionResponse>
{
    public Guid DecisionId { get; set; }
    
    public string? Notes { get; set; }

    public Guid ExecutedByUserId { get; set; }
}
