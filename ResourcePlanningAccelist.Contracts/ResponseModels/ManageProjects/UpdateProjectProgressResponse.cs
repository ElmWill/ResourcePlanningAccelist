namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

public class UpdateProjectProgressResponse
{
    public Guid ProjectId { get; set; }

    public int ProgressPercent { get; set; }

    public string Status { get; set; } = string.Empty;
}