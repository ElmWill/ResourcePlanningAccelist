using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;

public class GetAssignmentListRequest : IRequest<GetAssignmentListResponse>
{
    public Guid? ProjectId { get; set; }

    public Guid? EmployeeId { get; set; }

    public string? Status { get; set; }

    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }
}