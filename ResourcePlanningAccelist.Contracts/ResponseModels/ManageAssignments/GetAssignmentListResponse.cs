namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

public class GetAssignmentListResponse
{
    public List<AssignmentListItemResponse> Assignments { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}