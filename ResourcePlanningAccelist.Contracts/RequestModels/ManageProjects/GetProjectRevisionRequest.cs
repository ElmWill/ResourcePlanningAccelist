using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

public class GetProjectRevisionRequest : IRequest<GetProjectRevisionResponse>
{
    public Guid ProjectId { get; set; }
}
