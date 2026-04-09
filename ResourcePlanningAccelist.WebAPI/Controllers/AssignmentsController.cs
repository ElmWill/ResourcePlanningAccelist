using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;

namespace ResourcePlanningAccelist.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssignmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOrHr)]
    public async Task<ActionResult<CreateAssignmentResponse>> Create(
        [FromBody] CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("list")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOrHr)]
    public async Task<ActionResult<GetAssignmentListResponse>> List(
        [FromQuery] GetAssignmentListRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("update-status")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOrHr)]
    public async Task<ActionResult<UpdateAssignmentStatusResponse>> UpdateStatus(
        [FromBody] UpdateAssignmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}