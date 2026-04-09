namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

public class UpdateAssignmentStatusResponse
{
    public Guid AssignmentId { get; set; }

    public string Status { get; set; } = string.Empty;
}