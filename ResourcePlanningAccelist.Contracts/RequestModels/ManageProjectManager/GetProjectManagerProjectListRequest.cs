using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

public class GetProjectManagerProjectListRequest : IRequest<GetProjectManagerProjectListResponse>
{
    public Guid PmUserId { get; set; }

    public string? Status { get; set; }

    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }
}