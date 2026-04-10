using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

public class EmployeeListItemResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string? Department { get; set; }

    public decimal AvailabilityPercent { get; set; }

    public decimal WorkloadPercent { get; set; }
    public decimal AssignedHours { get; set; }
    public string Phone { get; set; } = string.Empty;
    public List<AssignmentListItemResponse> Assignments { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateOnly? HireDate { get; set; }
    public List<string> Skills { get; set; } = new();
}