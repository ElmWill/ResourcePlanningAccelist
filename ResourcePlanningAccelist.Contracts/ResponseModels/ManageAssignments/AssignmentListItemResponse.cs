namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

public class AssignmentListItemResponse
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal AllocationPercent { get; set; }
}