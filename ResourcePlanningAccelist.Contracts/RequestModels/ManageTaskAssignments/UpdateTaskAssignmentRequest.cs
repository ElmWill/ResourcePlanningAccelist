using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageTaskAssignments;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;

public class UpdateTaskAssignmentRequest : IRequest<UpdateTaskAssignmentResponse>
{
    public Guid TaskId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TaskName { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public int? WorkloadHours { get; set; }
    public DateOnly? DueDate { get; set; }
}
