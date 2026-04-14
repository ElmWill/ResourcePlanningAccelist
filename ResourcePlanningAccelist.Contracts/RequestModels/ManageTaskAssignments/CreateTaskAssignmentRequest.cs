using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageTaskAssignments;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;

public class CreateTaskAssignmentRequest : IRequest<CreateTaskAssignmentResponse>
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid AssignedByUserId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public int? WorkloadHours { get; set; }
    public DateOnly DueDate { get; set; }
}
