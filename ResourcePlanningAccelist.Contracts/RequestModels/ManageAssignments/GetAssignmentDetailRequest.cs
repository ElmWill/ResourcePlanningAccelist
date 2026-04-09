using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;

public class GetAssignmentDetailRequest : IRequest<GetAssignmentDetailResponse>
{
    public Guid AssignmentId { get; set; }
}