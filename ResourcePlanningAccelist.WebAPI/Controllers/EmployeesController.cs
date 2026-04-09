using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;

namespace ResourcePlanningAccelist.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("list")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOrGm)]
    public async Task<ActionResult<GetEmployeeListResponse>> List(
        [FromQuery] GetEmployeeListRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{employeeId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOrGm)]
    public async Task<ActionResult<GetEmployeeDetailResponse>> Detail(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var request = new GetEmployeeDetailRequest { EmployeeId = employeeId };
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}