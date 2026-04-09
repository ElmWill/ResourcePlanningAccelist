using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

public class AssignProjectManagerRequest : IRequest<AssignProjectManagerResponse>
{
    public Guid ProjectId { get; set; }

    public Guid PmUserId { get; set; }
}