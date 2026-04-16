using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjectManager;

public class GetProjectMilestoneListRequestHandler : IRequestHandler<GetProjectMilestoneListRequest, GetProjectMilestoneListResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectMilestoneListRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProjectMilestoneListResponse> Handle(GetProjectMilestoneListRequest request, CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects.AnyAsync(
            item => item.Id == request.ProjectId && (request.PmUserId == null || request.PmUserId == Guid.Empty || item.PmOwnerUserId == request.PmUserId),
            cancellationToken);

        if (!projectExists)
        {
            throw new KeyNotFoundException("Project not found for this project manager.");
        }

        var milestones = await _dbContext.ProjectMilestones
            .AsNoTracking()
            .Where(item => item.ProjectId == request.ProjectId)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.DueDate)
            .Select(item => new ProjectMilestoneItemResponse
            {
                MilestoneId = item.Id,
                Title = item.Title,
                Description = item.Description,
                DueDate = item.DueDate,
                IsCompleted = item.IsCompleted,
                SortOrder = item.SortOrder
            })
            .ToListAsync(cancellationToken);

        return new GetProjectMilestoneListResponse
        {
            ProjectId = request.ProjectId,
            Milestones = milestones
        };
    }
}