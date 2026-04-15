using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageTaskAssignments;
using ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;

namespace ResourcePlanningAccelist.WebAPI.Controllers;

[ApiController]
[Route("api/task-assignments")]
[Authorize]
public class TaskAssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaskAssignmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOnly)]
    public async Task<ActionResult<CreateTaskAssignmentResponse>> Create(
        [FromBody] CreateTaskAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("update")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOnly)]
    public async Task<ActionResult<UpdateTaskAssignmentResponse>> Update(
        [FromBody] UpdateTaskAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("list")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOnly)]
    public async Task<ActionResult<GetTaskAssignmentsResponse>> List(
        [FromQuery] Guid pmUserId,
        [FromQuery] Guid? projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var request = new GetTaskAssignmentsRequest
        {
            PmUserId = pmUserId,
            ProjectId = projectId,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOnly)]
    public async Task<ActionResult<GetTaskAssignmentsResponse>> GetByProject(
        Guid projectId,
        [FromQuery] Guid pmUserId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var request = new GetTaskAssignmentsRequest
        {
            ProjectId = projectId,
            PmUserId = pmUserId,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}
