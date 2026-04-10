namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;

public class GetHRDashboardSummaryResponse
{
    public int PendingValidationsCount { get; set; }

    public int TotalEmployeeCount { get; set; }

    public List<HRRecentValidationRequestResponse> RecentRequests { get; set; } = [];
}

public class HRRecentValidationRequestResponse
{
    public Guid Id { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public bool HasConflict { get; set; }

    public int DaysWaiting { get; set; }
}
