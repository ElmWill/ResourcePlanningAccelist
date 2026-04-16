using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageTaskAssignments;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;

public class DeleteTaskAssignmentRequest : IRequest<DeleteTaskAssignmentResponse>
{
    public Guid TaskId { get; set; }
}
