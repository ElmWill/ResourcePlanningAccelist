using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjectManager;

public class GetProjectTimelineTaskListRequestHandler : IRequestHandler<GetProjectTimelineTaskListRequest, GetProjectTimelineTaskListResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectTimelineTaskListRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProjectTimelineTaskListResponse> Handle(GetProjectTimelineTaskListRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.Id == request.ProjectId && item.PmOwnerUserId == request.PmUserId)
            .Select(item => new { item.Id, item.StartDate })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Project not found for this project manager.");

        var tasks = await _dbContext.ProjectTimelineTasks
            .AsNoTracking()
            .Where(item => item.ProjectId == request.ProjectId)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.StartOffsetDays)
            .Select(item => new ProjectTimelineTaskItemResponse
            {
                TimelineTaskId = item.Id,
                Name = item.Name,
                StartOffsetDays = item.StartOffsetDays,
                DurationDays = item.DurationDays,
                StartDate = project.StartDate.AddDays(item.StartOffsetDays),
                EndDate = project.StartDate.AddDays(item.StartOffsetDays + item.DurationDays),
                ColorTag = item.ColorTag,
                Status = item.Status.ToString(),
                SortOrder = item.SortOrder
            })
            .ToListAsync(cancellationToken);

        return new GetProjectTimelineTaskListResponse
        {
            ProjectId = request.ProjectId,
            Tasks = tasks
        };
    }
}