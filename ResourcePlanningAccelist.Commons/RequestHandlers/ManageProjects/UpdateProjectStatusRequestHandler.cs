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