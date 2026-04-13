using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;

public class CreateAssignmentRequest : IRequest<CreateAssignmentResponse>
{
    public Guid ProjectId { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid AssignedByUserId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal AllocationPercent { get; set; }

    public List<string> RequiredSkills { get; set; } = [];

    public string? AdditionalNeeds { get; set; }
}