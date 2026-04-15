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
        var now = DateTimeOffset.UtcNow;

        // Combine Assignment queries: fetch all GM-approved assignments once
        var allPendingAssignments = await _dbContext.Assignments
            .AsNoTracking()
            .Where(item => item.Status == AssignmentStatus.GmApproved)
            .OrderByDescending(item => item.CreatedAt)
            .Include(item => item.Employee.User)
            .Include(item => item.Project)
            .ToListAsync(cancellationToken);

        var pendingCount = allPendingAssignments.Count;
        var recentRequests = allPendingAssignments
            .Take(5)
            .Select(item => new HRRecentValidationRequestResponse
            {
                Id = item.Id,
                EmployeeName = item.Employee.User.FullName,
                ProjectName = item.Project.Name,
                HasConflict = !string.IsNullOrEmpty(item.ConflictWarning),
                DaysWaiting = (int)(now - item.CreatedAt).TotalDays
            })
            .ToList();

        var totalEmployees = await _dbContext.Employees
            .AsNoTracking()
            .CountAsync(item => item.Status == EmploymentStatus.Active, cancellationToken);

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

        // GM Decisions tracking: fetch into memory to avoid multiple round-trips
        var allGmDecisions = await _dbContext.GmDecisions
            .AsNoTracking()
            .OrderByDescending(item => item.SubmittedAt)
            .ToListAsync(cancellationToken);

        var pendingGmDecisionsCount = allGmDecisions.Count(item => item.Status == DecisionStatus.Executed);
        var pendingClarificationsCount = allGmDecisions.Count(item => item.Status == DecisionStatus.ClarificationRequested);
            
        var recentGmDecisions = allGmDecisions
            .Take(5)
            .Select(item => new RecentGmDecisionResponse
            {
                Id = item.Id,
                Type = item.DecisionType.ToString(),
                Details = item.Details,
                Date = item.SubmittedAt,
                Status = item.Status.ToString()
            })
            .ToList();

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
