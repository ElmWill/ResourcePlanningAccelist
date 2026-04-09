namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

public class UpdateEmployeeAvailabilityResponse
{
    public Guid EmployeeId { get; set; }

    public decimal AvailabilityPercent { get; set; }

    public decimal WorkloadPercent { get; set; }
}