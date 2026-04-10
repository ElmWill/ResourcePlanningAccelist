namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerDecisions;

public class GetGeneralManagerDecisionListResponse
{
    public List<GeneralManagerDecisionListItemResponse> Decisions { get; set; } = new();
}

public class GeneralManagerDecisionListItemResponse
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public List<string> AffectedEmployees { get; set; } = new();

    public DateOnly? Deadline { get; set; }

    public DateTimeOffset SubmittedAt { get; set; }

    public string Status { get; set; } = string.Empty;
}
