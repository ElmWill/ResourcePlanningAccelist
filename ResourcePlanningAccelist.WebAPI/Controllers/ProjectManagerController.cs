using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;

namespace ResourcePlanningAccelist.WebAPI.Controllers;

[ApiController]
[Route("api/project-manager")]
[Authorize]
public class ProjectManagerController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectManagerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("projects/list")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOnly)]
    public async Task<ActionResult<GetProjectManagerProjectListResponse>> ListProjects(
        [FromQuery] GetProjectManagerProjectListRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/overview")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOnly)]
    public async Task<ActionResult<GetProjectManagerProjectOverviewResponse>> ProjectOverview(
        Guid projectId,
        [FromQuery] Guid pmUserId,
        CancellationToken cancellationToken)
    {
        var request = new GetProjectManagerProjectOverviewRequest
        {
            ProjectId = projectId,
            PmUserId = pmUserId
        };

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/team")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOnly)]
    public async Task<ActionResult<GetProjectManagerProjectTeamResponse>> ProjectTeam(
        Guid projectId,
        [FromQuery] Guid pmUserId,
        CancellationToken cancellationToken)
    {
        var request = new GetProjectManagerProjectTeamRequest
        {
            ProjectId = projectId,
            PmUserId = pmUserId
        };

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/activity")]
    [Authorize(Policy = AuthorizationPolicyNames.PmOnly)]
    public async Task<ActionResult<GetProjectManagerProjectActivityResponse>> ProjectActivity(
        Guid projectId,
        [FromQuery] Guid pmUserId,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var request = new GetProjectManagerProjectActivityRequest
        {
            ProjectId = projectId,
            PmUserId = pmUserId,
            Limit = limit
        };

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("projects/assign-owner")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOrPm)]
    public async Task<ActionResult<AssignProjectManagerResponse>> AssignOwner(
        [FromBody] AssignProjectManagerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}