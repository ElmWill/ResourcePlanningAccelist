using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;

public class UpdateAssignmentProgressRequest : IRequest<UpdateAssignmentProgressResponse>
{
    public Guid AssignmentId { get; set; }

    public int ProgressPercent { get; set; }
}