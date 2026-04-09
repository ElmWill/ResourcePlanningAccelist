using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

public class GetEmployeeAssignmentsRequest : IRequest<GetEmployeeAssignmentsResponse>
{
    public Guid EmployeeId { get; set; }

    public string? Status { get; set; }

    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }
}