using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjectManager;

public class CreateProjectMilestoneRequestHandler : IRequestHandler<CreateProjectMilestoneRequest, CreateProjectMilestoneResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateProjectMilestoneRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateProjectMilestoneResponse> Handle(CreateProjectMilestoneRequest request, CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects.AnyAsync(
            item => item.Id == request.ProjectId && item.PmOwnerUserId == request.PmUserId,
            cancellationToken);

        if (!projectExists)
        {
            throw new KeyNotFoundException("Project not found for this project manager.");
        }

        var milestone = new ProjectMilestone
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            SortOrder = request.SortOrder
        };

        _dbContext.ProjectMilestones.Add(milestone);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProjectMilestoneResponse
        {
            MilestoneId = milestone.Id
        };
    }
}