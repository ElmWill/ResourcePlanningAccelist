using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

public class UpdateProjectTimelineTaskRequest : IRequest<UpdateProjectTimelineTaskResponse>
{
    public Guid PmUserId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid TimelineTaskId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int StartOffsetDays { get; set; }

    public int DurationDays { get; set; }

    public string ColorTag { get; set; } = "blue";

    public string Status { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}