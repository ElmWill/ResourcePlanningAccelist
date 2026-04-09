using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

public class GetEmployeeDetailRequest : IRequest<GetEmployeeDetailResponse>
{
    public Guid EmployeeId { get; set; }
}