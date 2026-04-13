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

    [HttpGet("{employeeId:guid}/assignments")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOrGm)]
    public async Task<ActionResult<GetEmployeeAssignmentsResponse>> Assignments(
        Guid employeeId,
        [FromQuery] string? status,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var request = new GetEmployeeAssignmentsRequest
        {
            EmployeeId = employeeId,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{employeeId:guid}/workload-summary")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOrGm)]
    public async Task<ActionResult<GetEmployeeWorkloadSummaryResponse>> WorkloadSummary(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var request = new GetEmployeeWorkloadSummaryRequest { EmployeeId = employeeId };
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("update-status")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOrGm)]
    public async Task<ActionResult<UpdateEmployeeStatusResponse>> UpdateStatus(
        [FromBody] UpdateEmployeeStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("update-availability")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOrGm)]
    public async Task<ActionResult<UpdateEmployeeAvailabilityResponse>> UpdateAvailability(
        [FromBody] UpdateEmployeeAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("create")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOnly)]
    public async Task<ActionResult<CreateEmployeeResponse>> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("update/{employeeId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOnly)]
    public async Task<ActionResult<UpdateEmployeeResponse>> Update(
        Guid employeeId,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        request.EmployeeId = employeeId;
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{employeeId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.HrOnly)]
    public async Task<ActionResult<DeleteEmployeeResponse>> Delete(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var request = new DeleteEmployeeRequest { EmployeeId = employeeId };
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}