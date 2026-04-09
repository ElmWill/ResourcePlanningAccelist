namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class UpdateProjectMilestoneStatusResponse
{
    public Guid MilestoneId { get; set; }

    public bool IsCompleted { get; set; }
}