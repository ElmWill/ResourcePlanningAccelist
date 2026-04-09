using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;

public class GetEmployeeAssignmentsResponse
{
    public Guid EmployeeId { get; set; }

    public List<AssignmentListItemResponse> Assignments { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}