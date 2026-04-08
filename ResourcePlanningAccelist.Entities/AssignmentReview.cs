using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class AssignmentReview : AuditableEntity
{
    public Guid AssignmentId { get; set; }

    public virtual Assignment Assignment { get; set; } = default!;

    public Guid? ReviewedByUserId { get; set; }

    public virtual AppUser? ReviewedByUser { get; set; }

    public AssignmentReviewStatus Status { get; set; } = AssignmentReviewStatus.Pending;

    public bool HasConflict { get; set; }

    [StringLength(2000)]
    public string? ConflictDetails { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }
}