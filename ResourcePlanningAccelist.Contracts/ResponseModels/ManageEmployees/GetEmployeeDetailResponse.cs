namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

public class GetEmployeeDetailResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string? Department { get; set; }

    public decimal AvailabilityPercent { get; set; }

    public decimal WorkloadPercent { get; set; }

    public List<string> Skills { get; set; } = new();

    public string Status { get; set; } = string.Empty;
}