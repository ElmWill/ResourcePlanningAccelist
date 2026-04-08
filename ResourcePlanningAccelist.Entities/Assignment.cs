using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class Assignment : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public virtual Project Project { get; set; } = default!;

    public Guid EmployeeId { get; set; }

    public virtual Employee Employee { get; set; } = default!;

    public Guid? AssignedByUserId { get; set; }

    public virtual AppUser? AssignedByUser { get; set; }

    [StringLength(200)]
    public string RoleName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal AllocationPercent { get; set; }

    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;

    public int ProgressPercent { get; set; }

    [StringLength(2000)]
    public string? ConflictWarning { get; set; }

    public DateTimeOffset? AcceptedAt { get; set; }

    public DateTimeOffset? RejectedAt { get; set; }

    public virtual AssignmentReview? Review { get; set; }
}