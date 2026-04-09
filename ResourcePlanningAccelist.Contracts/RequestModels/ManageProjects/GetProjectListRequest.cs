using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

public class GetProjectListRequest : IRequest<GetProjectListResponse>
{
    public string? Status { get; set; }

    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }
}