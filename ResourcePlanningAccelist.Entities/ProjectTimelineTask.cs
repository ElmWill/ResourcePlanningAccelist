using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class ProjectTimelineTask : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public virtual Project Project { get; set; } = default!;

    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public int StartOffsetDays { get; set; }

    public int DurationDays { get; set; }

    [StringLength(50)]
    public string ColorTag { get; set; } = "blue";

    public TimelineTaskStatus Status { get; set; } = TimelineTaskStatus.Pending;

    public int SortOrder { get; set; }
}