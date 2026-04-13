using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class GmDecision : AuditableEntity
{
    public Guid? ProjectId { get; set; }

    public virtual Project? Project { get; set; }

    public DecisionType DecisionType { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string Details { get; set; } = string.Empty;

    public DateOnly? Deadline { get; set; }

    public DecisionStatus Status { get; set; } = DecisionStatus.Pending;

    public Guid? SubmittedByUserId { get; set; }

    public virtual AppUser? SubmittedByUser { get; set; }

    public Guid? ExecutedByUserId { get; set; }

    public virtual AppUser? ExecutedByUser { get; set; }

    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ExecutedAt { get; set; }

    public DateTimeOffset? ClarificationRequestedAt { get; set; }
    
    [StringLength(4000)]
    public string? ClarificationReason { get; set; }

    public virtual ICollection<GmDecisionEmployee> AffectedEmployees { get; set; } = new List<GmDecisionEmployee>();
}