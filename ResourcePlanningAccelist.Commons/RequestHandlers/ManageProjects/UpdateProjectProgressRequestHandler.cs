using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class UpdateProjectProgressRequestHandler : IRequestHandler<UpdateProjectProgressRequest, UpdateProjectProgressResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateProjectProgressRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateProjectProgressResponse> Handle(UpdateProjectProgressRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(item => item.Id == request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");

        project.ProgressPercent = request.ProgressPercent;

        if (request.ProgressPercent == 100)
        {
            project.Status = ProjectStatus.Completed;
        }
        else if (request.ProgressPercent > 0 && project.Status is ProjectStatus.Assigned or ProjectStatus.Approved)
        {
            project.Status = ProjectStatus.InProgress;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProjectProgressResponse
        {
            ProjectId = project.Id,
            ProgressPercent = project.ProgressPercent,
            Status = project.Status.ToString()
        };
    }
}