using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerDecisions;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerDecisions;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageGeneralManagerDecisions;

public class GetGeneralManagerDecisionListRequestHandler : IRequestHandler<GetGeneralManagerDecisionListRequest, GetGeneralManagerDecisionListResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetGeneralManagerDecisionListRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetGeneralManagerDecisionListResponse> Handle(
        GetGeneralManagerDecisionListRequest request,
        CancellationToken cancellationToken)
    {
        var decisions = await _dbContext.GmDecisions
            .AsNoTracking()
            .Include(item => item.Project)
            .Include(item => item.AffectedEmployees)
                .ThenInclude(affected => affected.Employee.User)
            .OrderByDescending(item => item.SubmittedAt)
            .Select(item => new GeneralManagerDecisionListItemResponse
            {
                Id = item.Id,
                Type = item.DecisionType.ToString(),
                Title = item.Title,
                Details = item.Details,
                ProjectName = item.Project != null ? item.Project.Name : "General",
                AffectedEmployees = item.AffectedEmployees
                    .Select(ae => ae.Employee.User.FullName)
                    .ToList(),
                Deadline = item.Deadline,
                SubmittedAt = item.SubmittedAt,
                Status = item.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Fetch GM-approved assignments and map them as "ProjectAssignment" decisions for HR visibility
        var approvedAssignments = await _dbContext.Assignments
            .AsNoTracking()
            .Where(item => item.Status == AssignmentStatus.GmApproved)
            .Include(item => item.Project)
            .Include(item => item.Employee.User)
            .Select(item => new GeneralManagerDecisionListItemResponse
            {
                Id = item.Id,
                Type = DecisionType.ProjectAssignment.ToString(),
                Title = $"Resource Allocation: {item.Employee.User.FullName}",
                Details = $"Request to assign {item.Employee.User.FullName} to {item.Project.Name} as {item.RoleName} ({item.AllocationPercent}%).",
                ProjectName = item.Project.Name,
                AffectedEmployees = new List<string> { item.Employee.User.FullName },
                Deadline = null,
                SubmittedAt = item.CreatedAt,
                Status = item.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        decisions.AddRange(approvedAssignments);

        return new GetGeneralManagerDecisionListResponse
        {
            Decisions = decisions.OrderByDescending(d => d.SubmittedAt).ToList()
        };
    }
}
