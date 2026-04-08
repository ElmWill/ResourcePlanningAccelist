using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class GmDecisionEmployee : AuditableEntity
{
    public Guid DecisionId { get; set; }

    public virtual GmDecision Decision { get; set; } = default!;

    public Guid EmployeeId { get; set; }

    public virtual Employee Employee { get; set; } = default!;
}