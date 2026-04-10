using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageHumanResources;

public class GetHRDashboardSummaryRequestHandler : IRequestHandler<GetHRDashboardSummaryRequest, GetHRDashboardSummaryResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetHRDashboardSummaryRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetHRDashboardSummaryResponse> Handle(GetHRDashboardSummaryRequest request, CancellationToken cancellationToken)
    {
        var pendingAssignmentsQuery = _dbContext.Assignments
            .AsNoTracking()
            .Where(item => item.Status == AssignmentStatus.Pending);

        var pendingCount = await pendingAssignmentsQuery.CountAsync(cancellationToken);

        var totalEmployees = await _dbContext.Employees
            .AsNoTracking()
            .CountAsync(item => item.Status == EmploymentStatus.Active, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var recentRequests = await pendingAssignmentsQuery
            .OrderByDescending(item => item.CreatedAt)
            .Take(5)
            .Select(item => new HRRecentValidationRequestResponse
            {
                Id = item.Id,
                EmployeeName = item.Employee.User.FullName,
                ProjectName = item.Project.Name,
                HasConflict = !string.IsNullOrEmpty(item.ConflictWarning),
                DaysWaiting = (int)(now - item.CreatedAt).TotalDays
            })
            .ToListAsync(cancellationToken);

        return new GetHRDashboardSummaryResponse
        {
            PendingValidationsCount = pendingCount,
            TotalEmployeeCount = totalEmployees,
            RecentRequests = recentRequests
        };
    }
}
