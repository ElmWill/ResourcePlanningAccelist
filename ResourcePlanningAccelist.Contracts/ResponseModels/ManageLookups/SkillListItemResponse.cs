namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageLookups;

public class SkillListItemResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}