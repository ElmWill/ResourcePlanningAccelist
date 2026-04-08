using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class ProjectSkill : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public virtual Project Project { get; set; } = default!;

    public Guid SkillId { get; set; }

    public virtual Skill Skill { get; set; } = default!;
}