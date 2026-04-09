namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerDecisions;

public class GeneralManagerContractDecisionItemResponse
{
    public Guid DecisionId { get; set; }

    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string EmployeeAvatar { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public DateOnly? ContractEndDate { get; set; }

    public decimal AvailabilityPercent { get; set; }

    public decimal WorkloadPercent { get; set; }

    public int ActiveAssignmentCount { get; set; }

    public string DecisionType { get; set; } = string.Empty;

    public string DecisionStatus { get; set; } = string.Empty;
}