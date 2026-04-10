namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

public class CreateProjectResourceRequirementItem
{
    public string RoleName { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public string ExperienceLevel { get; set; } = "Mid";

    public string? Notes { get; set; }

    public int SortOrder { get; set; }

    public List<Guid> SkillIds { get; set; } = new();
}
