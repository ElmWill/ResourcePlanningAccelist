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
}
