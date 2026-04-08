namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

public class EmployeeListItemResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public decimal AvailabilityPercent { get; set; }

    public decimal WorkloadPercent { get; set; }
}