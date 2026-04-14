using System.Collections.Generic;

namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

public class GetProjectRevisionResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ClientName { get; set; }

    public string? Description { get; set; }

    public string? RejectionReason { get; set; }

    public List<ProjectSkillResponse> ProjectSkills { get; set; } = new();

    public List<ProjectResourceRequirementResponse> ResourceRequirements { get; set; } = new();
}

public class ProjectSkillResponse
{
    public Guid SkillId { get; set; }

    public string SkillName { get; set; } = string.Empty;
}

public class ProjectResourceRequirementResponse
{
    public Guid RequirementId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string ExperienceLevel { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public int SortOrder { get; set; }

    public List<ProjectRequirementSkillResponse> RequiredSkills { get; set; } = new();
}

public class ProjectRequirementSkillResponse
{
    public Guid SkillId { get; set; }

    public string SkillName { get; set; } = string.Empty;
}
