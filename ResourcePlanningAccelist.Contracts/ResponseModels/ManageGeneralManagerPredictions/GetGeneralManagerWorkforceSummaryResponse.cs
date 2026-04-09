namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;

public class GetGeneralManagerWorkforceSummaryResponse
{
    public Guid? DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public int TotalEmployeeCount { get; set; }

    public int ActiveEmployeeCount { get; set; }

    public decimal AverageAvailabilityPercent { get; set; }

    public decimal AverageWorkloadPercent { get; set; }

    public int OverloadedEmployeeCount { get; set; }

    public List<GeneralManagerSkillCoverageResponse> TopSkills { get; set; } = new();
}
