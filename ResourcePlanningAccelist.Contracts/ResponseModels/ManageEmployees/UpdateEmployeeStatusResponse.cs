namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

public class UpdateEmployeeStatusResponse
{
    public Guid EmployeeId { get; set; }

    public string Status { get; set; } = string.Empty;
}