using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class CancelProjectRequestHandler : IRequestHandler<CancelProjectRequest, CancelProjectResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CancelProjectRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CancelProjectResponse> Handle(CancelProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(item => item.Id == request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");

        project.Status = ProjectStatus.Cancelled;
        project.RejectionReason = request.Reason;
        project.RejectedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CancelProjectResponse
        {
            ProjectId = project.Id,
            Status = project.Status.ToString()
        };
    }
}