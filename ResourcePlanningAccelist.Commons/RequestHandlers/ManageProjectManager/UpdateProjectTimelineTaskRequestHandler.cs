using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjectManager;

public class UpdateProjectTimelineTaskRequestHandler : IRequestHandler<UpdateProjectTimelineTaskRequest, UpdateProjectTimelineTaskResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateProjectTimelineTaskRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateProjectTimelineTaskResponse> Handle(UpdateProjectTimelineTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await _dbContext.ProjectTimelineTasks
            .Include(item => item.Project)
            .FirstOrDefaultAsync(
                item => item.Id == request.TimelineTaskId
                     && item.ProjectId == request.ProjectId
                     && item.Project.PmOwnerUserId == request.PmUserId,
                cancellationToken)
            ?? throw new KeyNotFoundException("Timeline task not found for this project manager.");

        if (!Enum.TryParse<TimelineTaskStatus>(request.Status, true, out var parsedStatus))
        {
            throw new InvalidOperationException("Invalid timeline task status.");
        }

        task.Name = request.Name;
        task.StartOffsetDays = request.StartOffsetDays;
        task.DurationDays = request.DurationDays;
        task.ColorTag = request.ColorTag;
        task.SortOrder = request.SortOrder;
        task.Status = parsedStatus;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProjectTimelineTaskResponse
        {
            TimelineTaskId = task.Id,
            Status = task.Status.ToString()
        };
    }
}