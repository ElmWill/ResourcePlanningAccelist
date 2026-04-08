using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class Skill : AuditableEntity
{
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public SkillCategory Category { get; set; }

    public virtual ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();

    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();

    public virtual ICollection<ProjectRequirementSkill> RequirementSkills { get; set; } = new List<ProjectRequirementSkill>();
}