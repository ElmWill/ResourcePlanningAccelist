using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

public class GetProjectManagerProjectOverviewRequest : IRequest<GetProjectManagerProjectOverviewResponse>
{
    public Guid PmUserId { get; set; }

    public Guid ProjectId { get; set; }
}