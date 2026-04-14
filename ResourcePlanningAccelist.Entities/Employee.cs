using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class Employee : AuditableEntity
{
    public Guid UserId { get; set; }

    public virtual AppUser User { get; set; } = default!;

    [StringLength(100)]
    public string? EmployeeCode { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    public Guid? DepartmentId { get; set; }

    public virtual Department? Department { get; set; }

    [StringLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;

    public DateOnly? HireDate { get; set; }

    public decimal AvailabilityPercent { get; set; } = 100;

    public decimal WorkloadPercent { get; set; }

    public WorkloadStatus WorkloadState { get; set; } = WorkloadStatus.Available;

    public decimal AssignedHours { get; set; }

    public virtual ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<EmployeeContract> Contracts { get; set; } = new List<EmployeeContract>();
}