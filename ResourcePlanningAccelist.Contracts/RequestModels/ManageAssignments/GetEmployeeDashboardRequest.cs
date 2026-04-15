using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;

public class GetEmployeeDashboardRequest : IRequest<GetEmployeeDashboardResponse>
{
    public Guid EmployeeId { get; set; }
}
