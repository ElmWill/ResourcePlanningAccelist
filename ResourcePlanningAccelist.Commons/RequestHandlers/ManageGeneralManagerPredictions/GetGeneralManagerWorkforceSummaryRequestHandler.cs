using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageGeneralManagerPredictions;

public class GetGeneralManagerWorkforceSummaryRequestHandler : IRequestHandler<GetGeneralManagerWorkforceSummaryRequest, GetGeneralManagerWorkforceSummaryResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetGeneralManagerWorkforceSummaryRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetGeneralManagerWorkforceSummaryResponse> Handle(GetGeneralManagerWorkforceSummaryRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Employees
            .AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.Department)
            .Include(item => item.Contract)
            .Include(item => item.EmployeeSkills)
                .ThenInclude(item => item.Skill)
            .AsQueryable();

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(item => item.DepartmentId == request.DepartmentId.Value);
        }

        var employees = await query.ToListAsync(cancellationToken);
        var activeEmployees = employees
            .Where(item => item.Status == EmploymentStatus.Active)
            .Where(item => item.Contract == null || item.Contract.Status is ContractStatus.Active or ContractStatus.Extended)
            .ToList();

        var topSkillLimit = request.TopSkillLimit ?? 10;

        var skillCoverage = activeEmployees
            .SelectMany(item => item.EmployeeSkills)
            .GroupBy(item => new
            {
                item.SkillId,
                item.Skill.Name,
                Category = item.Skill.Category.ToString()
            })
            .Select(group => new GeneralManagerSkillCoverageResponse
            {
                SkillId = group.Key.SkillId,
                SkillName = group.Key.Name,
                Category = group.Key.Category,
                EmployeeCount = group.Select(item => item.EmployeeId).Distinct().Count(),
                CoveragePercent = activeEmployees.Count == 0
                    ? 0m
                    : Math.Round(group.Select(item => item.EmployeeId).Distinct().Count() * 100m / activeEmployees.Count, 2, MidpointRounding.AwayFromZero)
            })
            .OrderByDescending(item => item.EmployeeCount)
            .ThenBy(item => item.SkillName)
            .Take(topSkillLimit)
            .ToList();

        var averageAvailability = activeEmployees.Count == 0
            ? 0m
            : activeEmployees.Average(item => item.AvailabilityPercent);

        var averageWorkload = activeEmployees.Count == 0
            ? 0m
            : activeEmployees.Average(item => item.WorkloadPercent);

        var overloadedCount = activeEmployees.Count(item => item.WorkloadPercent > 100m);

        return new GetGeneralManagerWorkforceSummaryResponse
        {
            DepartmentId = request.DepartmentId,
            DepartmentName = employees.FirstOrDefault()?.Department?.Name,
            TotalEmployeeCount = employees.Count,
            ActiveEmployeeCount = activeEmployees.Count,
            AverageAvailabilityPercent = RoundScore(averageAvailability),
            AverageWorkloadPercent = RoundScore(averageWorkload),
            OverloadedEmployeeCount = overloadedCount,
            TopSkills = skillCoverage
        };
    }

    private static decimal RoundScore(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
