using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjectManager;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjectManager;

public class GetProjectManagerProjectTeamRequestHandler : IRequestHandler<GetProjectManagerProjectTeamRequest, GetProjectManagerProjectTeamResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectManagerProjectTeamRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProjectManagerProjectTeamResponse> Handle(GetProjectManagerProjectTeamRequest request, CancellationToken cancellationToken)
    {
        var activeAssignmentStatuses = new[]
        {
            AssignmentStatus.Pending,
            AssignmentStatus.Approved,
            AssignmentStatus.Accepted,
            AssignmentStatus.InProgress,
        };

        var projectExists = await _dbContext.Projects.AnyAsync(
            item => item.Id == request.ProjectId && item.PmOwnerUserId == request.PmUserId,
            cancellationToken);

        if (!projectExists)
        {
            throw new KeyNotFoundException("Project not found for this project manager.");
        }

        var teamMembers = await _dbContext.Assignments
            .AsNoTracking()
            .Where(item => item.ProjectId == request.ProjectId)
            .Where(item => activeAssignmentStatuses.Contains(item.Status))
            .Where(item => item.AllocationPercent > 0)
            .Include(item => item.Employee)
                .ThenInclude(item => item.User)
            .OrderBy(item => item.Employee.User.FullName)
            .Select(item => new ProjectManagerTeamMemberItemResponse
            {
                AssignmentId = item.Id,
                EmployeeId = item.EmployeeId,
                FullName = item.Employee.User.FullName,
                JobTitle = item.Employee.JobTitle,
                RoleName = item.RoleName,
                AllocationPercent = item.AllocationPercent,
                AssignmentStatus = item.Status.ToString(),
                AvailabilityPercent = item.Employee.AvailabilityPercent,
                WorkloadPercent = item.Employee.WorkloadPercent,
                AssignedHours = item.Employee.AssignedHours,
                EmployeeStatus = item.Employee.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new GetProjectManagerProjectTeamResponse
        {
            ProjectId = request.ProjectId,
            TeamMembers = teamMembers
        };
    }
}