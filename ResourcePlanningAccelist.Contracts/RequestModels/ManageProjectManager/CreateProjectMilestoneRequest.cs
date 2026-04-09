using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

public class CreateProjectMilestoneRequest : IRequest<CreateProjectMilestoneResponse>
{
    public Guid PmUserId { get; set; }

    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly DueDate { get; set; }

    public int SortOrder { get; set; }
}