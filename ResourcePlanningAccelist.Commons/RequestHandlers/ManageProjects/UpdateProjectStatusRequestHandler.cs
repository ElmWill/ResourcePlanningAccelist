using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class UpdateProjectStatusRequestHandler : IRequestHandler<UpdateProjectStatusRequest, UpdateProjectStatusResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateProjectStatusRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateProjectStatusResponse> Handle(UpdateProjectStatusRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(item => item.Id == request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");

        if (!Enum.TryParse<ProjectStatus>(request.Status, true, out var parsedStatus))
        {
            throw new InvalidOperationException("Invalid project status.");
        }

        project.Status = parsedStatus;

        if (parsedStatus == ProjectStatus.Approved && request.PmOwnerUserId.HasValue)
        {
            var pmOwnerSourceId = request.PmOwnerUserId.Value;

            var pmUserId = await _dbContext.Employees
                .AsNoTracking()
                .Where(item => item.Id == pmOwnerSourceId)
                .Select(item => item.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (pmUserId != Guid.Empty)
            {
                project.PmOwnerUserId = pmUserId;
            }
            else if (await _dbContext.Users.AsNoTracking().AnyAsync(item => item.Id == pmOwnerSourceId, cancellationToken))
            {
                project.PmOwnerUserId = pmOwnerSourceId;
            }
            else
            {
                throw new KeyNotFoundException("Recommended PM not found.");
            }

            var activePmProjectCount = await _dbContext.Projects
                .AsNoTracking()
                .CountAsync(item =>
                    item.PmOwnerUserId == project.PmOwnerUserId &&
                    item.Id != project.Id &&
                    (item.Status == ProjectStatus.Assigned || item.Status == ProjectStatus.InProgress),
                    cancellationToken);

            if (activePmProjectCount >= 2)
            {
                throw new InvalidOperationException("This project manager can only hold 2 active projects at a time.");
            }
        }

        if (parsedStatus == ProjectStatus.Rejected)
        {
            project.RejectedAt = DateTimeOffset.UtcNow;
            project.RejectionReason = request.RejectionReason;

            _dbContext.Notifications.Add(new Notification
            {
                UserId = project.CreatedByUserId,
                Type = NotificationType.Alert,
                Title = "Project draft rejected",
                Message = string.IsNullOrWhiteSpace(request.RejectionReason)
                    ? $"Your project '{project.Name}' was rejected by General Manager. Please revise and resubmit."
                    : $"Your project '{project.Name}' was rejected by General Manager. Reason: {request.RejectionReason}",
                IsRead = false,
                SourceEntityType = nameof(Project),
                SourceEntityId = project.Id
            });
        }

        if (parsedStatus == ProjectStatus.Submitted)
        {
            project.SubmittedAt = DateTimeOffset.UtcNow;
        }

        if (parsedStatus == ProjectStatus.Approved)
        {
            project.ApprovedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProjectStatusResponse
        {
            ProjectId = project.Id,
            Status = project.Status.ToString()
        };
    }
}