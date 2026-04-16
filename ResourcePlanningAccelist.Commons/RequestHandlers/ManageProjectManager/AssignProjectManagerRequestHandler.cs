using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjectManager;

public class AssignProjectManagerRequestHandler : IRequestHandler<AssignProjectManagerRequest, AssignProjectManagerResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public AssignProjectManagerRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AssignProjectManagerResponse> Handle(AssignProjectManagerRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(item => item.Id == request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");

        var pmUser = await _dbContext.Users.FirstOrDefaultAsync(item => item.Id == request.PmUserId, cancellationToken)
            ?? throw new KeyNotFoundException("Project manager user not found.");

        if (pmUser.Role != UserRole.Pm)
        {
            throw new InvalidOperationException("Provided user is not a project manager.");
        }

        var activePmProjectCount = await _dbContext.Projects
            .AsNoTracking()
            .CountAsync(item =>
                item.PmOwnerUserId == request.PmUserId &&
                item.Id != project.Id &&
                (item.Status == ProjectStatus.Assigned || item.Status == ProjectStatus.InProgress),
                cancellationToken);

        var projectWillBeActive = project.Status == ProjectStatus.Approved;
        if (projectWillBeActive && activePmProjectCount >= 2)
        {
            throw new InvalidOperationException("This project manager can only hold 2 active projects at a time.");
        }

        project.PmOwnerUserId = request.PmUserId;

        if (project.Status == ProjectStatus.Approved)
        {
            project.Status = ProjectStatus.Assigned;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AssignProjectManagerResponse
        {
            ProjectId = project.Id,
            PmUserId = request.PmUserId
        };
    }
}