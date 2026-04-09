using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

public class UpdateProjectMilestoneStatusRequest : IRequest<UpdateProjectMilestoneStatusResponse>
{
    public Guid PmUserId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid MilestoneId { get; set; }

    public bool IsCompleted { get; set; }
}