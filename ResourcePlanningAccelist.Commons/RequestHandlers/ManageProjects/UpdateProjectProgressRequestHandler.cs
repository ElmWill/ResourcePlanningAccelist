using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class UpdateProjectProgressRequestHandler : IRequestHandler<UpdateProjectProgressRequest, UpdateProjectProgressResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateProjectProgressRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateProjectProgressResponse> Handle(UpdateProjectProgressRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(item => item.Id == request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");

        project.ProgressPercent = request.ProgressPercent;

        if (request.ProgressPercent == 100)
        {
            project.Status = ProjectStatus.Completed;

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
            // add one notification
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
        else if (request.ProgressPercent > 0 && project.Status is ProjectStatus.Assigned or ProjectStatus.Approved)
        {
            project.Status = ProjectStatus.InProgress;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProjectProgressResponse
        {
            ProjectId = project.Id,
            ProgressPercent = project.ProgressPercent,
            Status = project.Status.ToString()
        };
    }
}