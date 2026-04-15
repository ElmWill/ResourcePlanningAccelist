namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageTaskAssignments;

public class UpdateTaskAssignmentResponse
{
    public Guid TaskId { get; set; }
    public string Status { get; set; } = string.Empty;
}
