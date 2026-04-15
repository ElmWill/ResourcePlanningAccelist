using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageTaskAssignments;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;

public class GetTaskAssignmentsRequest : IRequest<GetTaskAssignmentsResponse>
{
    public Guid? ProjectId { get; set; }
    public Guid PmUserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}
