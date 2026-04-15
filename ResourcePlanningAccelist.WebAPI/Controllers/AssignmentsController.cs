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
    [Authorize(Policy = AuthorizationPolicyNames.PmHrOrGm)]
    public async Task<ActionResult<GetAssignmentListResponse>> List(
        [FromQuery] GetAssignmentListRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{assignmentId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.PmHrOrGm)]
    public async Task<ActionResult<GetAssignmentDetailResponse>> Detail(
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var request = new GetAssignmentDetailRequest { AssignmentId = assignmentId };
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("update-status")]
    [Authorize(Policy = AuthorizationPolicyNames.PmHrOrGm)]
    public async Task<ActionResult<UpdateAssignmentStatusResponse>> UpdateStatus(
        [FromBody] UpdateAssignmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("update-progress")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOrHr)]
    public async Task<ActionResult<UpdateAssignmentProgressResponse>> UpdateProgress(
        [FromBody] UpdateAssignmentProgressRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("split-workload")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOrHr)]
    public async Task<ActionResult<SplitAssignmentWorkloadResponse>> SplitWorkload(
        [FromBody] SplitAssignmentWorkloadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("employee-dashboard/{employeeId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.AnyRole)]
    public async Task<ActionResult<GetEmployeeDashboardResponse>> GetEmployeeDashboard(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var request = new GetEmployeeDashboardRequest { EmployeeId = employeeId };
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}