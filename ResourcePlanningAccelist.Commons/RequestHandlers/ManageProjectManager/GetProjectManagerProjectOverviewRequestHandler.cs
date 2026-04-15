using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjectManager;

public class GetProjectManagerProjectOverviewRequestHandler : IRequestHandler<GetProjectManagerProjectOverviewRequest, GetProjectManagerProjectOverviewResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectManagerProjectOverviewRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProjectManagerProjectOverviewResponse> Handle(GetProjectManagerProjectOverviewRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.Id == request.ProjectId)
            .Where(item => request.PmUserId == null || request.PmUserId == Guid.Empty || item.PmOwnerUserId == request.PmUserId)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.Description,
                item.ProgressPercent,
                item.Status,
                item.RiskLevel,
                item.StartDate,
                item.EndDate,
                TeamSize = item.TotalRequiredResources > 0
                    ? item.TotalRequiredResources
                    : item.Assignments
                        .Where(assignment => assignment.Status != AssignmentStatus.Pending)
                        .Select(assignment => assignment.EmployeeId)
                        .Distinct()
                        .Count(),
                TotalAssignments = item.Assignments.Count(assignment => assignment.Status != AssignmentStatus.Pending),
                CompletedAssignments = item.Assignments.Count(assignment => assignment.Status == AssignmentStatus.Completed)
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Project not found for this project manager.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysRemaining = project.EndDate.DayNumber - today.DayNumber;

        return new GetProjectManagerProjectOverviewResponse
        {
            ProjectId = project.Id,
            Name = project.Name,
            Description = project.Description,
            ProgressPercent = project.ProgressPercent,
            Status = project.Status.ToString(),
            RiskLevel = project.RiskLevel.ToString(),
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            DaysRemaining = Math.Max(daysRemaining, 0),
            TeamSize = project.TeamSize,
            TotalAssignments = project.TotalAssignments,
            CompletedAssignments = project.CompletedAssignments
        };
    }
}