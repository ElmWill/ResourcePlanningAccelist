using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageTaskAssignments;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageTaskAssignments;

public class GetTaskAssignmentsRequestHandler : IRequestHandler<GetTaskAssignmentsRequest, GetTaskAssignmentsResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetTaskAssignmentsRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetTaskAssignmentsResponse> Handle(
        GetTaskAssignmentsRequest request,
        CancellationToken cancellationToken)
    {
        // Start with all task assignments that belong to projects managed by the PM
        var query = _dbContext.TaskAssignments
            .Include(t => t.Project)
            .Include(t => t.Employee)
            .ThenInclude(e => e.User)
            .AsQueryable();

        // If ProjectId is specified, filter by project
        if (request.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == request.ProjectId.Value && t.Project.PmOwnerUserId == request.PmUserId);
        }
        else
        {
            // Filter by projects managed by this PM
            query = query.Where(t => t.Project.PmOwnerUserId == request.PmUserId);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var taskDtos = tasks.Select(t => new TaskAssignmentDto
        {
            TaskId = t.Id,
            ProjectId = t.ProjectId,
            ProjectName = t.Project?.Name ?? "Unknown Project",
            EmployeeId = t.EmployeeId,
            EmployeeName = t.Employee?.User?.FullName ?? "Unknown Employee",
            TaskName = t.TaskName,
            Description = t.Description,
            Priority = t.Priority.ToString(),
            WorkloadHours = t.WorkloadHours,
            DueDate = t.DueDate.ToString("yyyy-MM-dd"),
            Status = t.Status.ToString(),
            CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            AssignedDate = t.CreatedAt.ToString("yyyy-MM-dd"),
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new GetTaskAssignmentsResponse
        {
            Data = taskDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        };
    }
}
