using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;
using ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;

namespace ResourcePlanningAccelist.WebAPI.Controllers;

[ApiController]
[Route("api/human-resource")]
[Authorize]
public class HumanResourceController : ControllerBase
{
    private readonly IMediator _mediator;

    public HumanResourceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard-summary")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOrGm)]
    public async Task<ActionResult<GetHRDashboardSummaryResponse>> GetDashboardSummary(
        CancellationToken cancellationToken)
    {
        var request = new GetHRDashboardSummaryRequest();
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("execute-decision")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOnly)]
    public async Task<ActionResult<ExecuteGmDecisionResponse>> ExecuteDecision(
        [FromBody] ExecuteGmDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("execute-contract")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOnly)]
    public async Task<ActionResult<ExecuteContractActionResponse>> ExecuteContract(
        [FromBody] ExecuteContractActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("start-hiring")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOnly)]
    public async Task<ActionResult<StartHiringResponse>> StartHiring(
        [FromBody] StartHiringRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("hiring/list")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOnly)]
    public async Task<ActionResult<GetHiringListResponse>> GetHiringList(
        CancellationToken cancellationToken)
    {
        var request = new GetHiringListRequest();
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("hiring/update-stage")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOnly)]
    public async Task<ActionResult<UpdateHiringStageResponse>> UpdateHiringStage(
        [FromBody] UpdateHiringStageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("rehire")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOnly)]
    public async Task<ActionResult<RehireEmployeeResponse>> RehireEmployee(
        [FromBody] RehireEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("decision/{id}/clarify")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOnly)]
    public async Task<ActionResult<RequestClarificationResponse>> RequestClarification(
        string id,
        [FromBody] RequestClarificationRequestBody body,
        CancellationToken cancellationToken)
    {
        var request = new RequestClarificationRequest
        {
            DecisionId = Guid.Parse(id),
            Reason = body.Reason
        };
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}

public class RequestClarificationRequestBody
{
    public string Reason { get; set; } = string.Empty;
}
