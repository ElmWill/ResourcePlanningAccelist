namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

public class UpdateProjectStatusResponse
{
    public Guid ProjectId { get; set; }

    public string Status { get; set; } = string.Empty;
}