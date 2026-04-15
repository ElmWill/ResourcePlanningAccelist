using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageTaskAssignments;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;

public class GetEmployeeDashboardRequestHandler : IRequestHandler<GetEmployeeDashboardRequest, GetEmployeeDashboardResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetEmployeeDashboardRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetEmployeeDashboardResponse> Handle(GetEmployeeDashboardRequest request, CancellationToken cancellationToken)
    {
        // 1. Fetch basic assignments first to avoid circular Include cycles
        var userAssignments = await _dbContext.Assignments
            .AsNoTracking()
            .Include(a => a.Employee)
            .Include(a => a.Project)
                .ThenInclude(p => p!.PmOwnerUser)
            .Where(a => a.Employee.UserId == request.EmployeeId)
            .ToListAsync(cancellationToken);

        if (userAssignments.Count == 0)
        {
            return new GetEmployeeDashboardResponse();
        }

        var projectIds = userAssignments.Select(a => a.ProjectId).Distinct().ToList();
        var employeeId = userAssignments.First().EmployeeId;

        // 2. Fetch extra details (milestones, tasks, team) separately to avoid EF Core cycles
        var milestones = await _dbContext.ProjectMilestones
            .AsNoTracking()
            .Where(m => projectIds.Contains(m.ProjectId))
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

        var taskAssignments = await _dbContext.TaskAssignments
            .AsNoTracking()
            .Where(ta => projectIds.Contains(ta.ProjectId) && ta.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);

        var allProjectAssignments = await _dbContext.Assignments
            .AsNoTracking()
            .Include(a => a.Employee)
                .ThenInclude(e => e.User)
            .Where(a => projectIds.Contains(a.ProjectId) && 
                (a.Status == AssignmentStatus.Accepted ||
                 a.Status == AssignmentStatus.Approved ||
                 a.Status == AssignmentStatus.InProgress ||
                 a.Status == AssignmentStatus.Completed))
            .ToListAsync(cancellationToken);

        var pendingStatuses = new[] { AssignmentStatus.Pending, AssignmentStatus.GmApproved, AssignmentStatus.Approved };
        var activeStatuses = new[] { AssignmentStatus.Accepted, AssignmentStatus.InProgress };

        var pendingCount = userAssignments.Count(a => pendingStatuses.Contains(a.Status));
        var activeCount = userAssignments.Count(a => activeStatuses.Contains(a.Status));

        var response = new GetEmployeeDashboardResponse
        {
            PendingAssignmentsCount = pendingCount,
            ActiveProjectsCount = activeCount,
            Assignments = userAssignments.Select(a =>
            {
                var project = a.Project!;
                var projectMilestones = milestones.Where(m => m.ProjectId == a.ProjectId).ToList();
                var projectTasks = taskAssignments.Where(ta => ta.ProjectId == a.ProjectId).ToList();
                var projectTeam = allProjectAssignments
                    .Where(pa => pa.ProjectId == a.ProjectId && pa.EmployeeId != employeeId)
                    .ToList();

                return new EmployeeDashboardAssignmentResponse
                {
                    Id = a.Id,
                    ProjectName = project.Name,
                    ProjectDescription = project.Description ?? string.Empty,
                    ProjectManagerName = project.PmOwnerUser?.FullName ?? "Not Assigned",
                    RoleName = a.RoleName,
                    Status = a.Status.ToString(),
                    ProjectProgressPercent = project.ProgressPercent,
                    ProgressPercent = a.ProgressPercent,
                    AllocationPercent = (int)a.AllocationPercent,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    TeamMembers = projectTeam
                        .Select(pa => new EmployeeDashboardTeamMemberResponse
                        {
                            FullName = pa.Employee.User.FullName,
                            RoleName = pa.RoleName,
                            JobTitle = pa.Employee.JobTitle
                        })
                        .DistinctBy(tm => tm.FullName)
                        .ToList(),
                    Milestones = projectMilestones
                        .Select(m => new ProjectMilestoneItemResponse
                        {
                            MilestoneId = m.Id,
                            Title = m.Title,
                            Description = m.Description,
                            DueDate = m.DueDate,
                            IsCompleted = m.IsCompleted,
                            SortOrder = m.SortOrder
                        }).ToList(),
                    Tasks = projectTasks
                        .OrderBy(ta => ta.Status == Entities.TaskStatus.InProgress ? 0 : 1)
                        .ThenByDescending(ta => ta.CreatedAt)
                        .Select(ta => new TaskAssignmentDto
                        {
                            TaskId = ta.Id,
                            ProjectId = ta.ProjectId,
                            ProjectName = project.Name,
                            EmployeeId = ta.EmployeeId,
                            EmployeeName = a.Employee.User?.FullName ?? string.Empty,
                            TaskName = ta.TaskName,
                            Description = ta.Description,
                            Priority = ta.Priority.ToString(),
                            Status = ta.Status.ToString(),
                            WorkloadHours = ta.WorkloadHours,
                            DueDate = ta.DueDate.ToString("yyyy-MM-dd"),
                            AssignedDate = ta.CreatedAt.ToString("yyyy-MM-dd")
                        }).ToList()
                };
            }).ToList()
        };

        return response;
    }
}
