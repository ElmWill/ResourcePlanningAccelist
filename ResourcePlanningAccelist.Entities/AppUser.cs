using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class AppUser : AuditableEntity
{
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(512)]
    public string? AvatarUrl { get; set; }

    public UserRole Role { get; set; }

    public Guid? DepartmentId { get; set; }

    public virtual Department? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual Employee? EmployeeProfile { get; set; }

    public virtual ICollection<Project> CreatedProjects { get; set; } = new List<Project>();

    public virtual ICollection<Project> ApprovedProjects { get; set; } = new List<Project>();

    public virtual ICollection<Project> ManagedProjects { get; set; } = new List<Project>();

    public virtual ICollection<ProjectReview> ProjectReviews { get; set; } = new List<ProjectReview>();

    public virtual ICollection<ProjectAttachment> UploadedProjectAttachments { get; set; } = new List<ProjectAttachment>();

    public virtual ICollection<Assignment> CreatedAssignments { get; set; } = new List<Assignment>();

    public virtual ICollection<AssignmentReview> AssignmentReviews { get; set; } = new List<AssignmentReview>();

    public virtual ICollection<EmployeeContract> ExecutedContracts { get; set; } = new List<EmployeeContract>();

    public virtual ICollection<GmDecision> SubmittedDecisions { get; set; } = new List<GmDecision>();

    public virtual ICollection<GmDecision> ExecutedDecisions { get; set; } = new List<GmDecision>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}