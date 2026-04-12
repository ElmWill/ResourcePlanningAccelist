using System;
using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class HiringRequest : AuditableEntity
{
    public Guid GmDecisionId { get; set; }

    public virtual GmDecision GmDecision { get; set; } = default!;

    [StringLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Details { get; set; } = string.Empty;

    public HiringRequestStatus Status { get; set; } = HiringRequestStatus.Sourcing;

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}

