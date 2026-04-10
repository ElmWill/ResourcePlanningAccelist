using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerDecisions;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerDecisions;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageGeneralManagerDecisions;

public class GetGeneralManagerContractDecisionSummaryRequestHandler : IRequestHandler<GetGeneralManagerContractDecisionSummaryRequest, GetGeneralManagerContractDecisionSummaryResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetGeneralManagerContractDecisionSummaryRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetGeneralManagerContractDecisionSummaryResponse> Handle(
        GetGeneralManagerContractDecisionSummaryRequest request,
        CancellationToken cancellationToken)
    {
        var decisions = await _dbContext.GmDecisionEmployees
            .AsNoTracking()
            .Where(item => item.Decision.DecisionType == DecisionType.ExtendContract || item.Decision.DecisionType == DecisionType.TerminateContract)
            .OrderBy(item => item.Decision.Deadline ?? DateOnly.MaxValue)
            .ThenBy(item => item.Decision.SubmittedAt)
            .Select(item => new GeneralManagerContractDecisionItemResponse
            {
                DecisionId = item.DecisionId,
                EmployeeId = item.EmployeeId,
                EmployeeName = item.Employee.User.FullName,
                EmployeeAvatar = BuildInitials(item.Employee.User.FullName),
                JobTitle = item.Employee.JobTitle,
                ContractEndDate = item.Employee.Contracts
                    .OrderByDescending(c => c.EndDate)
                    .Select(c => (DateOnly?)c.EndDate)
                    .FirstOrDefault(),
                AvailabilityPercent = item.Employee.AvailabilityPercent,
                WorkloadPercent = item.Employee.WorkloadPercent,
                ActiveAssignmentCount = item.Employee.Assignments.Count(assignment =>
                    assignment.Status == AssignmentStatus.Pending ||
                    assignment.Status == AssignmentStatus.Approved ||
                    assignment.Status == AssignmentStatus.Accepted ||
                    assignment.Status == AssignmentStatus.InProgress),
                DecisionType = item.Decision.DecisionType.ToString(),
                DecisionStatus = item.Decision.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new GetGeneralManagerContractDecisionSummaryResponse
        {
            Decisions = decisions
        };
    }

    private static string BuildInitials(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return string.Empty;
        }

        var firstInitial = parts[0][0];
        var secondInitial = parts.Length > 1 ? parts[^1][0] : parts[0][Math.Min(1, parts[0].Length - 1)];
        return new string(new[] { char.ToUpperInvariant(firstInitial), char.ToUpperInvariant(secondInitial) });
    }
}