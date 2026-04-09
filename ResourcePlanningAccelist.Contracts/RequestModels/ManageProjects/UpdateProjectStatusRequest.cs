using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

public class UpdateProjectStatusRequest : IRequest<UpdateProjectStatusResponse>
{
    public Guid ProjectId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? RejectionReason { get; set; }
}