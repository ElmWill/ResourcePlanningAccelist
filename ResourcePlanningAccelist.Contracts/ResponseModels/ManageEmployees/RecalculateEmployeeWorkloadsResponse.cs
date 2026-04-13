namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

public class RecalculateEmployeeWorkloadsResponse
{
    public int TotalEmployeesRecalculated { get; set; }

    public int OverloadedEmployeeCount { get; set; }
}
