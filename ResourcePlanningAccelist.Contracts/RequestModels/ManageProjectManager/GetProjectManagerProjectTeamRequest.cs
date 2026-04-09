using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

public class GetProjectManagerProjectTeamRequest : IRequest<GetProjectManagerProjectTeamResponse>
{
    public Guid PmUserId { get; set; }

    public Guid ProjectId { get; set; }
}