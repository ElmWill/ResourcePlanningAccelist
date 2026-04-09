using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageLookups;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageLookups;

public class GetDepartmentListRequest : IRequest<GetDepartmentListResponse>
{
    public string? Query { get; set; }

    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }
}