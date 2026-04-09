using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

public class CancelProjectRequest : IRequest<CancelProjectResponse>
{
    public Guid ProjectId { get; set; }

    public string Reason { get; set; } = string.Empty;
}