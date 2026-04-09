using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

public class UpdateEmployeeStatusRequest : IRequest<UpdateEmployeeStatusResponse>
{
    public Guid EmployeeId { get; set; }

    public string Status { get; set; } = string.Empty;
}