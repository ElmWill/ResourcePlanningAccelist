using MediatR;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class CreateProjectRequestHandler : IRequestHandler<CreateProjectRequest, CreateProjectResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateProjectRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateProjectResponse> Handle(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            CreatedByUserId = request.CreatedByUserId,
            Name = request.Name,
            ClientName = request.ClientName,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = ProjectStatus.Draft
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProjectResponse
        {
            ProjectId = project.Id
        };
    }
}