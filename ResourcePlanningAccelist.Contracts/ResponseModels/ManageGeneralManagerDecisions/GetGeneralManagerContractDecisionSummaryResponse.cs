namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerDecisions;

public class GetGeneralManagerContractDecisionSummaryResponse
{
    public List<GeneralManagerContractDecisionItemResponse> Decisions { get; set; } = new();
}