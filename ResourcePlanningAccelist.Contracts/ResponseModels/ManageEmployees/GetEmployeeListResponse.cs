namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

public class GetEmployeeListResponse
{
    public List<EmployeeListItemResponse> Employees { get; set; } = new();
}