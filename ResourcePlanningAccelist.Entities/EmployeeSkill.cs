using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class EmployeeSkill : AuditableEntity
{
    public Guid EmployeeId { get; set; }

    public virtual Employee Employee { get; set; } = default!;

    public Guid SkillId { get; set; }

    public virtual Skill Skill { get; set; } = default!;

    public int Proficiency { get; set; } = 3;

    public bool IsPrimary { get; set; }
}