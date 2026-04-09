using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

public class GetProjectDetailRequest : IRequest<GetProjectDetailResponse>
{
    public Guid ProjectId { get; set; }
}