using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

public class CreateProjectTimelineTaskRequest : IRequest<CreateProjectTimelineTaskResponse>
{
    public Guid PmUserId { get; set; }

    public Guid ProjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int StartOffsetDays { get; set; }

    public int DurationDays { get; set; }

    public string ColorTag { get; set; } = "blue";

    public int SortOrder { get; set; }
}