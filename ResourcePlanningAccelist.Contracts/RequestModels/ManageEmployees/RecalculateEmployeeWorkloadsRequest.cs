using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;

public class RecalculateEmployeeWorkloadsRequest : IRequest<RecalculateEmployeeWorkloadsResponse>
{
}
