using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;

public class SplitAssignmentWorkloadRequest : IRequest<SplitAssignmentWorkloadResponse>
{
    public Guid ProjectId { get; set; }

    public Guid FromEmployeeId { get; set; }

    public Guid ToEmployeeId { get; set; }

    public Guid? AssignedByUserId { get; set; }

    public string? RoleName { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal SplitAllocationPercent { get; set; }
}
