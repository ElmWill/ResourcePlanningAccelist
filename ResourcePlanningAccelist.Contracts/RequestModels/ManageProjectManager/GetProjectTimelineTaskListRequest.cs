using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

public class GetProjectTimelineTaskListRequest : IRequest<GetProjectTimelineTaskListResponse>
{
    public Guid PmUserId { get; set; }

    public Guid ProjectId { get; set; }
}