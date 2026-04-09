namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

public class UpdateAssignmentProgressResponse
{
    public Guid AssignmentId { get; set; }

    public int ProgressPercent { get; set; }

    public string Status { get; set; } = string.Empty;
}