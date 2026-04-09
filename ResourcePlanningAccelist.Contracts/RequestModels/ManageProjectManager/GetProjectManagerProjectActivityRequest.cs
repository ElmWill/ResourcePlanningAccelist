using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

public class GetProjectManagerProjectActivityRequest : IRequest<GetProjectManagerProjectActivityResponse>
{
    public Guid PmUserId { get; set; }

    public Guid ProjectId { get; set; }

    public int? Limit { get; set; }
}