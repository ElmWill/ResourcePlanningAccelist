using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;

public class RequestClarificationRequest : IRequest<RequestClarificationResponse>
{
    public Guid DecisionId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
