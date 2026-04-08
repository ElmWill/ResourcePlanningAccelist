using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class EmployeeContract : AuditableEntity
{
    public Guid EmployeeId { get; set; }

    public virtual Employee Employee { get; set; } = default!;

    public Guid? CurrentProjectId { get; set; }

    public virtual Project? CurrentProject { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public ContractStatus Status { get; set; } = ContractStatus.Active;

    [StringLength(2000)]
    public string? Notes { get; set; }
}