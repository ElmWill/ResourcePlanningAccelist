using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class TaskAssignment : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public virtual Project Project { get; set; } = default!;

    public Guid EmployeeId { get; set; }

    public virtual Employee Employee { get; set; } = default!;

    public Guid? AssignedByUserId { get; set; }

    public virtual AppUser? AssignedByUser { get; set; }

    [StringLength(500)]
    public string TaskName { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    public int WorkloadHours { get; set; }

    public DateOnly DueDate { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Pending;
}

public enum TaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
}
