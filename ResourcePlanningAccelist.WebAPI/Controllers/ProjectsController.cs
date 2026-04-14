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

    [HttpPost("{projectId:guid}/attachments")]
    [Authorize(Policy = AuthorizationPolicyNames.MarketingOnly)]
    public async Task<ActionResult<CreateProjectAttachmentResponse>> UploadAttachment(
        Guid projectId,
        [FromForm] IFormFile file,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var request = new CreateProjectAttachmentRequest
        {
            ProjectId = projectId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            FileContent = memoryStream.ToArray()
        };

        var result = await _mediator.Send(request, cancellationToken);

        // Write file to disk
        var absolutePath = Path.Combine(environment.ContentRootPath, result.StorageKey);
        var directory = Path.GetDirectoryName(absolutePath)!;
        Directory.CreateDirectory(directory);
        await System.IO.File.WriteAllBytesAsync(absolutePath, result.FileContent, cancellationToken);

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

    [HttpGet("{projectId:guid}/revision")]
    [Authorize(Policy = AuthorizationPolicyNames.ProjectReadAccess)]
    public async Task<ActionResult<GetProjectRevisionResponse>> Revision(
    Guid projectId,
    CancellationToken cancellationToken)
    {
        var request = new GetProjectRevisionRequest { ProjectId = projectId };
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{projectId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.MarketingOnly)]
    public async Task<ActionResult<UpdateRevisedProjectResponse>> UpdateRevised(
    Guid projectId,
    [FromBody] UpdateRevisedProjectRequest request,
    CancellationToken cancellationToken)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}