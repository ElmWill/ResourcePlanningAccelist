using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;
using System.Security.Claims;

namespace ResourcePlanningAccelist.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("list")]
    [Authorize(Policy = AuthorizationPolicyNames.ProjectReadAccess)]
    public async Task<ActionResult<GetProjectListResponse>> List(
        [FromQuery] GetProjectListRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("create")]
    [Authorize(Policy = AuthorizationPolicyNames.MarketingOnly)]
    public async Task<ActionResult<CreateProjectResponse>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(nameIdentifier, out var userId))
        {
            request.CreatedByUserId = userId;
        }

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{projectId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.ProjectReadAccess)]
    public async Task<ActionResult<GetProjectDetailResponse>> Detail(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var request = new GetProjectDetailRequest { ProjectId = projectId };
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("update-status")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOrPm)]
    public async Task<ActionResult<UpdateProjectStatusResponse>> UpdateStatus(
        [FromBody] UpdateProjectStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("update-progress")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOrPm)]
    public async Task<ActionResult<UpdateProjectProgressResponse>> UpdateProgress(
        [FromBody] UpdateProjectProgressRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("cancel")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOrPm)]
    public async Task<ActionResult<CancelProjectResponse>> Cancel(
        [FromBody] CancelProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{projectId:guid}/assignments")]
    [Authorize(Policy = AuthorizationPolicyNames.ProjectReadAccess)]
    public async Task<ActionResult<GetProjectAssignmentsResponse>> Assignments(
        Guid projectId,
        [FromQuery] string? status,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var request = new GetProjectAssignmentsRequest
        {
            ProjectId = projectId,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}