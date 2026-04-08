using System.ComponentModel.DataAnnotations;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class ProjectResourceRequirement : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public virtual Project Project { get; set; } = default!;

    [StringLength(200)]
    public string RoleName { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Mid;

    [StringLength(2000)]
    public string? Notes { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<ProjectRequirementSkill> RequiredSkills { get; set; } = new List<ProjectRequirementSkill>();
}