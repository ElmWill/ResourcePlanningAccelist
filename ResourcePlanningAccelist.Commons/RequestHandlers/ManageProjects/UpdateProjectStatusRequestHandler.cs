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

        if (parsedStatus == ProjectStatus.Completed)
        {
            project.ProgressPercent = 100;

            // 1. Fetch active assignments to close them
            var activeAssignments = await _dbContext.Assignments
                .Where(a => a.ProjectId == project.Id && (a.Status == AssignmentStatus.Accepted || a.Status == AssignmentStatus.InProgress))
                .ToListAsync(cancellationToken);

            foreach (var assignment in activeAssignments)
            {
                assignment.Status = AssignmentStatus.Completed;
            }

            // 2. Notifications for Marketing (Creator) and PM
            var recipients = new HashSet<Guid> { project.CreatedByUserId };
            if (project.PmOwnerUserId.HasValue) recipients.Add(project.PmOwnerUserId.Value);

            var notifications = recipients.Select(userId => new Notification
            {
                UserId = userId,
                Type = NotificationType.Alert,
                Title = "Project Completed",
                Message = $"Congratulations! The project '{project.Name}' has reached 100% progress and is officially completed.",
                IsRead = false,
                SourceEntityType = nameof(Project),
                SourceEntityId = project.Id
            }).ToList();

            // 3. Notifications for Employees (Batch fetch query outside the loop)
            var activeEmployeeIds = activeAssignments.Select(a => a.EmployeeId).Distinct().ToList();
            var employeeDict = await _dbContext.Employees
                .AsNoTracking()
                .Where(e => activeEmployeeIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, cancellationToken);

            foreach (var assignment in activeAssignments)
            {
                if (employeeDict.TryGetValue(assignment.EmployeeId, out var employee))
                {
                    // add one notification in loop
                    notifications.Add(new Notification
                    {
                        UserId = employee.UserId,
                        Type = NotificationType.Assignment,
                        Title = "Goal Achieved!",
                        Message = $"The project '{project.Name}' is finished. Your contribution has been marked as completed. Thank you!",
                        IsRead = false,
                        SourceEntityType = nameof(Assignment),
                        SourceEntityId = assignment.Id
                    });
                }
            }

            _dbContext.Notifications.AddRange(notifications);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 4. Recalculate workload for freed-up employees (Batch call)
            await ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments.AssignmentWorkloadUpdater.RecalculateEmployeeWorkloadsAsync(
                _dbContext, 
                activeEmployeeIds, 
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProjectStatusResponse
        {
            ProjectId = project.Id,
            Status = project.Status.ToString()
        };
    }
}