namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

public class CancelProjectResponse
{
    public Guid ProjectId { get; set; }

    public string Status { get; set; } = string.Empty;
}