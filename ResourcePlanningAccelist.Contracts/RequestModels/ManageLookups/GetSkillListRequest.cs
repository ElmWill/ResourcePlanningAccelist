using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageLookups;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageLookups;

public class GetSkillListRequest : IRequest<GetSkillListResponse>
{
    public string? Query { get; set; }

    public string? Category { get; set; }

    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }
}