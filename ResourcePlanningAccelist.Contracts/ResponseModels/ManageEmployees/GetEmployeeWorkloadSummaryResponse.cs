namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

public class GetEmployeeWorkloadSummaryResponse
{
    public Guid EmployeeId { get; set; }

    public decimal AvailabilityPercent { get; set; }

    public decimal WorkloadPercent { get; set; }

    public decimal AssignedHours { get; set; }

    public int ActiveAssignmentCount { get; set; }

    public string WorkloadState { get; set; } = string.Empty;
}