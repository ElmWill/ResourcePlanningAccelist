using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;
using System.Security.Claims;

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
    [Authorize(Policy = AuthorizationPolicyNames.ProjectReadAccess)]
    public async Task<ActionResult<GetProjectManagerProjectListResponse>> ListProjects(
        [FromQuery] GetProjectManagerProjectListRequest request,
        CancellationToken cancellationToken)
    {
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(nameIdentifier, out var userId))
        {
            if (!User.IsInRole("gm"))
            {
                request.PmUserId = userId;
            }
        }

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/overview")]
    [Authorize(Policy = AuthorizationPolicyNames.ProjectReadAccess)]
    public async Task<ActionResult<GetProjectManagerProjectOverviewResponse>> ProjectOverview(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var request = new GetProjectManagerProjectOverviewRequest
        {
            ProjectId = projectId
        };

        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(nameIdentifier, out var userId))
        {
            if (!User.IsInRole("gm"))
            {
                request.PmUserId = userId;
            }
        }

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/team")]
    [Authorize(Policy = AuthorizationPolicyNames.ProjectReadAccess)]
    public async Task<ActionResult<GetProjectManagerProjectTeamResponse>> ProjectTeam(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var request = new GetProjectManagerProjectTeamRequest
        {
            ProjectId = projectId
        };

        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(nameIdentifier, out var userId))
        {
            if (!User.IsInRole("gm"))
            {
                request.PmUserId = userId;
            }
        }

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/activity")]
    [Authorize(Policy = AuthorizationPolicyNames.ProjectReadAccess)]
    public async Task<ActionResult<GetProjectManagerProjectActivityResponse>> ProjectActivity(
        Guid projectId,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var request = new GetProjectManagerProjectActivityRequest
        {
            ProjectId = projectId,
            Limit = limit
        };

        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(nameIdentifier, out var userId))
        {
            if (!User.IsInRole("gm"))
            {
                request.PmUserId = userId;
            }
        }

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

    [HttpGet("projects/{projectId:guid}/milestones")]
    [Authorize(Policy = AuthorizationPolicyNames.ProjectReadAccess)]
    public async Task<ActionResult<GetProjectMilestoneListResponse>> Milestones(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var request = new GetProjectMilestoneListRequest
        {
            ProjectId = projectId
        };

        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(nameIdentifier, out var userId))
        {
            if (!User.IsInRole("gm"))
            {
                request.PmUserId = userId;
            }
        }

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("projects/milestones/create")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOrPm)]
    public async Task<ActionResult<CreateProjectMilestoneResponse>> CreateMilestone(
        [FromBody] CreateProjectMilestoneRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("projects/milestones/update-status")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOrPm)]
    public async Task<ActionResult<UpdateProjectMilestoneStatusResponse>> UpdateMilestoneStatus(
        [FromBody] UpdateProjectMilestoneStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/timeline-tasks")]
    [Authorize(Policy = AuthorizationPolicyNames.ProjectReadAccess)]
    public async Task<ActionResult<GetProjectTimelineTaskListResponse>> TimelineTasks(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var request = new GetProjectTimelineTaskListRequest
        {
            ProjectId = projectId
        };

        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(nameIdentifier, out var userId))
        {
            if (!User.IsInRole("gm"))
            {
                request.PmUserId = userId;
            }
        }

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("projects/timeline-tasks/create")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOrPm)]
    public async Task<ActionResult<CreateProjectTimelineTaskResponse>> CreateTimelineTask(
        [FromBody] CreateProjectTimelineTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("projects/timeline-tasks/update")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOrPm)]
    public async Task<ActionResult<UpdateProjectTimelineTaskResponse>> UpdateTimelineTask(
        [FromBody] UpdateProjectTimelineTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}