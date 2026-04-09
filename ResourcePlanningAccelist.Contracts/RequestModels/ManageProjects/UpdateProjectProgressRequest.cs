using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

public class UpdateProjectProgressRequest : IRequest<UpdateProjectProgressResponse>
{
    public Guid ProjectId { get; set; }

    public int ProgressPercent { get; set; }
}