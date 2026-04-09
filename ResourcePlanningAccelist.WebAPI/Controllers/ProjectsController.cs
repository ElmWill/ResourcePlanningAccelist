using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;

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
}