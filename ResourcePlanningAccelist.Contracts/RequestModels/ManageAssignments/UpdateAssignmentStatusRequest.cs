using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;

public class UpdateAssignmentStatusRequest : IRequest<UpdateAssignmentStatusResponse>
{
    public Guid AssignmentId { get; set; }

    public string Status { get; set; } = string.Empty;
}