using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class ProjectReview : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public virtual Project Project { get; set; } = default!;

    public Guid? ReviewerUserId { get; set; }

    public virtual AppUser? ReviewerUser { get; set; }

    public ReviewDecision Decision { get; set; }

    [StringLength(2000)]
    public string? Feedback { get; set; }

    public DateTimeOffset ReviewedAt { get; set; } = DateTimeOffset.UtcNow;
}