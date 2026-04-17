using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourcePlanningAccelist.Contracts.RequestModels.Reports;
using ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;
using System.Threading;
using System.Threading.Tasks;

namespace ResourcePlanningAccelist.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("metrics")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOrGm)]
    public async Task<ActionResult<GetReportDashboardMetricsResponse>> GetMetrics([FromQuery] GetReportDashboardMetricsRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("export")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOrGm)]
    public async Task<IActionResult> Export([FromQuery] ExportReportRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return File(result.FileContent, result.ContentType, result.FileName);
    }
}
