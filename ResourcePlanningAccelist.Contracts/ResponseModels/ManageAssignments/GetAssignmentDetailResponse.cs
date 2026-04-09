namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

public class GetAssignmentDetailResponse
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal AllocationPercent { get; set; }

    public int ProgressPercent { get; set; }

    public string Status { get; set; } = string.Empty;
}