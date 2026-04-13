namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;

public class GetHRDashboardSummaryResponse
{
    public int PendingValidationsCount { get; set; }
    public int TotalEmployeeCount { get; set; }
    public int ExpiringContractsCount { get; set; }
    public int ActiveHiringRequestsCount { get; set; }
    public int PendingGmDecisionsCount { get; set; }
    public int PendingClarificationsCount { get; set; }

    public List<HRRecentValidationRequestResponse> RecentRequests { get; set; } = [];
    public List<HRExpiringContractResponse> ExpiringContracts { get; set; } = [];
    public List<RecentGmDecisionResponse> RecentGmDecisions { get; set; } = [];
}

public class RecentGmDecisionResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class HRExpiringContractResponse
{
    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public DateOnly? EndDate { get; set; }
}

public class HRRecentValidationRequestResponse
{
    public Guid Id { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public bool HasConflict { get; set; }

    public int DaysWaiting { get; set; }
}
