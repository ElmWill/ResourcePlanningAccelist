using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

public class CreateProjectRequest : IRequest<CreateProjectResponse>
{
    public Guid CreatedByUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ClientName { get; set; }

    public string? Description { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}