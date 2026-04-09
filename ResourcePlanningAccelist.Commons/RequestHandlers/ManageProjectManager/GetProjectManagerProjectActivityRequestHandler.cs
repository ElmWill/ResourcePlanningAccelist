using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjectManager;

public class GetProjectManagerProjectActivityRequestHandler : IRequestHandler<GetProjectManagerProjectActivityRequest, GetProjectManagerProjectActivityResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectManagerProjectActivityRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProjectManagerProjectActivityResponse> Handle(GetProjectManagerProjectActivityRequest request, CancellationToken cancellationToken)
    {
        var effectiveLimit = request.Limit ?? PaginationDefaults.PageSize;
        var limit = Math.Clamp(effectiveLimit, 1, PaginationDefaults.MaxPageSize);

        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.Id == request.ProjectId && item.PmOwnerUserId == request.PmUserId)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.CreatedAt,
                item.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Project not found for this project manager.");

        var activities = new List<ProjectManagerActivityItemResponse>
        {
            new()
            {
                OccurredAt = project.CreatedAt,
                Message = $"Project '{project.Name}' was created."
            }
        };

        if (project.UpdatedAt.HasValue && project.UpdatedAt.Value > project.CreatedAt)
        {
            activities.Add(new ProjectManagerActivityItemResponse
            {
                OccurredAt = project.UpdatedAt.Value,
                Message = $"Project '{project.Name}' was updated."
            });
        }

        var assignmentActivities = await _dbContext.Assignments
            .AsNoTracking()
            .Where(item => item.ProjectId == request.ProjectId)
            .Include(item => item.Employee)
                .ThenInclude(item => item.User)
            .Select(item => new
            {
                item.CreatedAt,
                item.UpdatedAt,
                EmployeeName = item.Employee.User.FullName,
                item.RoleName,
                item.Status
            })
            .ToListAsync(cancellationToken);

        activities.AddRange(assignmentActivities.Select(item => new ProjectManagerActivityItemResponse
        {
            OccurredAt = item.CreatedAt,
            Message = $"{item.EmployeeName} was assigned as {item.RoleName}."
        }));

        activities.AddRange(
            assignmentActivities
                .Where(item => item.UpdatedAt.HasValue && item.UpdatedAt.Value > item.CreatedAt)
                .Select(item => new ProjectManagerActivityItemResponse
                {
                    OccurredAt = item.UpdatedAt!.Value,
                    Message = $"{item.EmployeeName}'s assignment status changed to {item.Status}."
                }));

        var ordered = activities
            .OrderByDescending(item => item.OccurredAt)
            .Take(limit)
            .ToList();

        return new GetProjectManagerProjectActivityResponse
        {
            ProjectId = request.ProjectId,
            Activities = ordered
        };
    }
}