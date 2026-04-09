using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

public class GetEmployeeWorkloadSummaryRequest : IRequest<GetEmployeeWorkloadSummaryResponse>
{
    public Guid EmployeeId { get; set; }
}