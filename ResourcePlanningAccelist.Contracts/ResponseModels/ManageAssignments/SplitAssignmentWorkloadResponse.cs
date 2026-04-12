namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

public class SplitAssignmentWorkloadResponse
{
    public Guid SourceAssignmentId { get; set; }

    public decimal SourceAllocationPercent { get; set; }

    public Guid TargetAssignmentId { get; set; }

    public decimal TargetAllocationPercent { get; set; }

    public decimal AppliedSplitPercent { get; set; }
}
