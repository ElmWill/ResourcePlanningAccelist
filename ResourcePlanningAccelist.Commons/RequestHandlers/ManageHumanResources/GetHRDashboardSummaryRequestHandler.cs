using System;
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
            .Where(item => item.Status == AssignmentStatus.GmApproved);

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

        // Calculate Active Hiring Requests
        var activeHiringCount = await _dbContext.HiringRequests
            .AsNoTracking()
            .CountAsync(item => item.Status != HiringRequestStatus.Completed && item.Status != HiringRequestStatus.Cancelled, cancellationToken);

        // Calculate Expiring Contracts (within 31 days)
        var today = DateOnly.FromDateTime(DateTime.Today);
        var endThreshold = today.AddDays(31);

        var expiringContracts = await _dbContext.Employees
            .AsNoTracking()
            .SelectMany(e => e.Contracts)
            .Where(c => c.Status == ContractStatus.Active && c.EndDate != null && c.EndDate >= today && c.EndDate <= endThreshold)
            .Select(c => new HRExpiringContractResponse
            {
                EmployeeId = c.EmployeeId,
                EmployeeName = c.Employee.User.FullName,
                EndDate = c.EndDate
            })
            .ToListAsync(cancellationToken);

        // GM Decisions tracking
        var gmDecisionsQuery = _dbContext.GmDecisions.AsNoTracking();

        var pendingGmDecisionsCount = await gmDecisionsQuery
            .CountAsync(item => item.Status == DecisionStatus.Executed, cancellationToken);

        var pendingClarificationsCount = await gmDecisionsQuery
            .CountAsync(item => item.Status == DecisionStatus.ClarificationRequested, cancellationToken);
            
        var recentGmDecisions = await gmDecisionsQuery
            .OrderByDescending(item => item.SubmittedAt)
            .Take(5)
            .Select(item => new RecentGmDecisionResponse
            {
                Id = item.Id,
                Type = item.DecisionType.ToString(),
                Details = item.Details,
                Date = item.SubmittedAt,
                Status = item.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new GetHRDashboardSummaryResponse
        {
            PendingValidationsCount = pendingCount,
            TotalEmployeeCount = totalEmployees,
            ActiveHiringRequestsCount = activeHiringCount,
            ExpiringContractsCount = expiringContracts.Count,
            PendingGmDecisionsCount = pendingGmDecisionsCount,
            PendingClarificationsCount = pendingClarificationsCount,
            RecentRequests = recentRequests,
            ExpiringContracts = expiringContracts,
            RecentGmDecisions = recentGmDecisions
        };
    }
}
