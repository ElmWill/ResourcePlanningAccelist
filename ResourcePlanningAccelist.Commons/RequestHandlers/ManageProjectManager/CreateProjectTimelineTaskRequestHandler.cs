using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjectManager;

public class CreateProjectTimelineTaskRequestHandler : IRequestHandler<CreateProjectTimelineTaskRequest, CreateProjectTimelineTaskResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateProjectTimelineTaskRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateProjectTimelineTaskResponse> Handle(CreateProjectTimelineTaskRequest request, CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects.AnyAsync(
            item => item.Id == request.ProjectId && item.PmOwnerUserId == request.PmUserId,
            cancellationToken);

        if (!projectExists)
        {
            throw new KeyNotFoundException("Project not found for this project manager.");
        }

        var task = new ProjectTimelineTask
        {
            ProjectId = request.ProjectId,
            Name = request.Name,
            StartOffsetDays = request.StartOffsetDays,
            DurationDays = request.DurationDays,
            ColorTag = request.ColorTag,
            SortOrder = request.SortOrder
        };

        _dbContext.ProjectTimelineTasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProjectTimelineTaskResponse
        {
            TimelineTaskId = task.Id
        };
    }
}