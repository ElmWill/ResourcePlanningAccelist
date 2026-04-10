using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

public class DeleteEmployeeRequest : IRequest<DeleteEmployeeResponse>
{
    public Guid EmployeeId { get; set; }
}
