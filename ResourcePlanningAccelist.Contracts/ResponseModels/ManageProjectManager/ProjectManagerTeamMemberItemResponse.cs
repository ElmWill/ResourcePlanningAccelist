namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

public class ProjectManagerTeamMemberItemResponse
{
    public Guid EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public decimal AllocationPercent { get; set; }

    public string AssignmentStatus { get; set; } = string.Empty;
}