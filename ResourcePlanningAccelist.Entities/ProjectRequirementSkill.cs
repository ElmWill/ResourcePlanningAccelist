using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class ProjectRequirementSkill : AuditableEntity
{
    public Guid RequirementId { get; set; }

    public virtual ProjectResourceRequirement Requirement { get; set; } = default!;

    public Guid SkillId { get; set; }

    public virtual Skill Skill { get; set; } = default!;
}