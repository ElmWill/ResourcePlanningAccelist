using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

public class GetEmployeeListRequest : IRequest<GetEmployeeListResponse>
{
    public string? Department { get; set; }
}