namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

public class CreateEmployeeResponse
{
    public Guid EmployeeId { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class UpdateEmployeeResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class DeleteEmployeeResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
