using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

public class GetProjectAssignmentsResponse
{
    public Guid ProjectId { get; set; }

    public List<AssignmentListItemResponse> Assignments { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}