using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
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
        var assignments = await _dbContext.Assignments
            .AsNoTracking()
            .Include(a => a.Project)
            .Where(a => a.EmployeeId == request.EmployeeId)
            .ToListAsync(cancellationToken);

        Console.WriteLine($"[DEBUG] Found {assignments.Count} assignments for EmployeeId: {request.EmployeeId}");
        foreach (var a in assignments)
        {
            Console.WriteLine($"[DEBUG] Assignment ID: {a.Id}, Status: {a.Status}");
        }

        var pendingStatuses = new[] { AssignmentStatus.Pending, AssignmentStatus.GmApproved, AssignmentStatus.Approved };
        var activeStatuses = new[] { AssignmentStatus.Accepted, AssignmentStatus.InProgress };

        var pendingCount = assignments.Count(a => pendingStatuses.Contains(a.Status));
        var activeCount = assignments.Count(a => activeStatuses.Contains(a.Status));

        var response = new GetEmployeeDashboardResponse
        {
            PendingAssignmentsCount = pendingCount,
            ActiveProjectsCount = activeCount,
            Assignments = assignments.Select(a => new EmployeeDashboardAssignmentResponse
            {
                Id = a.Id,
                ProjectName = a.Project?.Name ?? "Unknown Project",
                ProjectDescription = a.Project?.Description ?? string.Empty,
                RoleName = a.RoleName,
                Status = a.Status.ToString(),
                ProjectProgressPercent = a.Project?.ProgressPercent ?? 0,
                ProgressPercent = a.ProgressPercent,
                AllocationPercent = (int)a.AllocationPercent,
                EndDate = a.EndDate
            }).ToList()
        };

        return response;
    }
}
