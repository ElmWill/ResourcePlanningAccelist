namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

public class GetEmployeeDashboardResponse
{
    public int PendingAssignmentsCount { get; set; }
    public int ActiveProjectsCount { get; set; }
    public List<EmployeeDashboardAssignmentResponse> Assignments { get; set; } = new();
}

public class EmployeeDashboardAssignmentResponse
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectDescription { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public int ProjectProgressPercent { get; set; }
    public int AllocationPercent { get; set; }
    public DateOnly EndDate { get; set; }
}
