using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerDecisions;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerDecisions;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;
using ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;
using System.Security.Claims;

namespace ResourcePlanningAccelist.WebAPI.Controllers;

[ApiController]
[Route("api/general-manager/predictions")]
[Authorize]
public class GeneralManagerController : ControllerBase
{
    private readonly IMediator _mediator;

    public GeneralManagerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("projects/{projectId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOnly)]
    public async Task<ActionResult<GetGeneralManagerProjectPredictionResponse>> PredictProject(
        Guid projectId,
        [FromQuery] int? candidateLimit,
        CancellationToken cancellationToken)
    {
        var request = new GetGeneralManagerProjectPredictionRequest
        {
            ProjectId = projectId,
            CandidateLimit = candidateLimit
        };

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/risk")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOnly)]
    public async Task<ActionResult<GetGeneralManagerProjectRiskResponse>> GetProjectRisk(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var request = new GetGeneralManagerProjectRiskRequest
        {
            ProjectId = projectId
        };

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("workforce-summary")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOnly)]
    public async Task<ActionResult<GetGeneralManagerWorkforceSummaryResponse>> GetWorkforceSummary(
        [FromQuery] Guid? departmentId,
        [FromQuery] int? topSkillLimit,
        CancellationToken cancellationToken)
    {
        var request = new GetGeneralManagerWorkforceSummaryRequest
        {
            DepartmentId = departmentId,
            TopSkillLimit = topSkillLimit
        };

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("contract-decisions")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOrGm)]
    public async Task<ActionResult<GetGeneralManagerContractDecisionSummaryResponse>> GetContractDecisions(
        CancellationToken cancellationToken)
    {
        var request = new GetGeneralManagerContractDecisionSummaryRequest();
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
    [HttpGet("decisions")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOrGm)]
    public async Task<ActionResult<GetGeneralManagerDecisionListResponse>> GetDecisions(
        CancellationToken cancellationToken)
    {
        var request = new GetGeneralManagerDecisionListRequest();
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("decisions/recommendation-response")]
    [Authorize(Policy = AuthorizationPolicyNames.GmOnly)]
    public async Task<ActionResult<UpdateGeneralManagerRecommendationResponse>> UpdateRecommendationResponse(
        [FromBody] UpdateGeneralManagerRecommendationResponseRequest request,
        CancellationToken cancellationToken)
    {
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(nameIdentifier, out var userId))
        {
            request.SubmittedByUserId = userId;
        }

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

}
