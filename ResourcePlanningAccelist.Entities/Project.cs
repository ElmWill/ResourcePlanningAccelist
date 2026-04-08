using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class Project : AuditableEntity
{
    public Guid CreatedByUserId { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = default!;

    public Guid? ApprovedByUserId { get; set; }

    public virtual AppUser? ApprovedByUser { get; set; }

    public Guid? PmOwnerUserId { get; set; }

    public virtual AppUser? PmOwnerUser { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? ClientName { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    public int ProgressPercent { get; set; }

    public ProjectRiskLevel RiskLevel { get; set; } = ProjectRiskLevel.Medium;

    public decimal ResourceUtilizationPercent { get; set; }

    public int TotalRequiredResources { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public DateTimeOffset? RejectedAt { get; set; }

    [StringLength(2000)]
    public string? RejectionReason { get; set; }

    public virtual ICollection<ProjectReview> Reviews { get; set; } = new List<ProjectReview>();

    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();

    public virtual ICollection<ProjectResourceRequirement> ResourceRequirements { get; set; } = new List<ProjectResourceRequirement>();

    public virtual ICollection<ProjectAttachment> Attachments { get; set; } = new List<ProjectAttachment>();

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<GmDecision> Decisions { get; set; } = new List<GmDecision>();

    public virtual ICollection<EmployeeContract> CurrentContracts { get; set; } = new List<EmployeeContract>();
}