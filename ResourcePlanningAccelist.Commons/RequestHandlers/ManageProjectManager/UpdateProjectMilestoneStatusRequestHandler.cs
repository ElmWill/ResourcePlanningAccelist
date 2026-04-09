using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjectManager;

public class UpdateProjectMilestoneStatusRequestHandler : IRequestHandler<UpdateProjectMilestoneStatusRequest, UpdateProjectMilestoneStatusResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateProjectMilestoneStatusRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateProjectMilestoneStatusResponse> Handle(UpdateProjectMilestoneStatusRequest request, CancellationToken cancellationToken)
    {
        var milestone = await _dbContext.ProjectMilestones
            .Include(item => item.Project)
            .FirstOrDefaultAsync(
                item => item.Id == request.MilestoneId
                     && item.ProjectId == request.ProjectId
                     && item.Project.PmOwnerUserId == request.PmUserId,
                cancellationToken)
            ?? throw new KeyNotFoundException("Milestone not found for this project manager.");

        milestone.IsCompleted = request.IsCompleted;
        milestone.CompletedAt = request.IsCompleted ? DateTimeOffset.UtcNow : null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProjectMilestoneStatusResponse
        {
            MilestoneId = milestone.Id,
            IsCompleted = milestone.IsCompleted
        };
    }
}