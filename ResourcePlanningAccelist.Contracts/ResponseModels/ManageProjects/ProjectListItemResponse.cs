namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

public class ProjectListItemResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}