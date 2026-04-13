using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectRiskRequestHandler : IRequestHandler<GetGeneralManagerProjectRiskRequest, GetGeneralManagerProjectRiskResponse>
{
    private readonly ApplicationDbContext _dbContext;

    private static readonly AssignmentStatus[] ActiveAssignmentStatuses =
    {
        AssignmentStatus.Pending,
        AssignmentStatus.Approved,
        AssignmentStatus.Accepted,
        AssignmentStatus.InProgress,
    };

    public GetGeneralManagerProjectRiskRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetGeneralManagerProjectRiskResponse> Handle(GetGeneralManagerProjectRiskRequest request, CancellationToken cancellationToken)
    {
        var candidateLimit = Math.Clamp(request.CandidateLimit ?? GeneralManagerPredictionConstants.DefaultCandidateLimit, 1, GeneralManagerPredictionConstants.MaxCandidateLimit);

        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.Id == request.ProjectId)
            .Include(item => item.ResourceRequirements)
                .ThenInclude(item => item.RequiredSkills)
                    .ThenInclude(item => item.Skill)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");

        var employees = await _dbContext.Employees
            .AsNoTracking()
            .Where(item => item.Status == EmploymentStatus.Active)
            .Include(item => item.EmployeeSkills)
                .ThenInclude(item => item.Skill)
            .Include(item => item.Contract)
            .ToListAsync(cancellationToken);

        employees = employees
            .Where(item => item.Contract == null || item.Contract.Status is ContractStatus.Active or ContractStatus.Extended)
            .ToList();

        var timelineWorkloadByEmployee = await _dbContext.Assignments
            .AsNoTracking()
            .Where(item => ActiveAssignmentStatuses.Contains(item.Status))
            .Where(item => item.StartDate <= project.EndDate && item.EndDate >= project.StartDate)
            .GroupBy(item => item.EmployeeId)
            .Select(group => new
            {
                EmployeeId = group.Key,
                AllocationPercent = group.Sum(item => item.AllocationPercent)
            })
            .ToDictionaryAsync(
                item => item.EmployeeId,
                item => Math.Max(0m, item.AllocationPercent),
                cancellationToken);

        var activeCandidateCount = employees.Count(item =>
            Math.Max(0m, 100m - (timelineWorkloadByEmployee.TryGetValue(item.Id, out var workload) ? workload : 0m)) > 0m);
        var averageWorkloadPercent = employees.Count == 0
            ? 0m
            : employees.Average(item => timelineWorkloadByEmployee.TryGetValue(item.Id, out var workload) ? workload : 0m);

        var totalRequiredResources = Math.Max(project.ResourceRequirements.Sum(item => item.Quantity), 1);
        var totalCoverage = 0m;
        var requirementBreakdown = new List<GeneralManagerProjectRiskRequirementResponse>();
        var riskReasons = new List<string>();

        foreach (var requirement in project.ResourceRequirements.OrderBy(item => item.SortOrder))
        {
            var requiredSkills = requirement.RequiredSkills.Select(item => item.Skill).DistinctBy(item => item.Id).ToList();
            var candidateScores = employees
                .Select(employee =>
                {
                    var skillScore = CalculateRequirementScore(employee, requiredSkills);
                    var workloadPercent = timelineWorkloadByEmployee.TryGetValue(employee.Id, out var workload) ? workload : 0m;
                    var availabilityScore = Math.Max(0m, 100m - Math.Max(0m, workloadPercent)) / 100m;

                    return (skillScore * 0.7m) + (availabilityScore * 0.3m);
                })
                .OrderByDescending(score => score)
                .ToList();

            var coverageScore = requirement.Quantity > 0
                ? candidateScores.Take(requirement.Quantity).Sum() / requirement.Quantity
                : 0m;
            var bestCandidateScore = candidateScores.Count > 0 ? candidateScores[0] : 0m;
            totalCoverage += coverageScore * requirement.Quantity;

            if (coverageScore < 0.5m)
            {
                riskReasons.Add($"{requirement.RoleName} coverage is weak.");
            }

            requirementBreakdown.Add(new GeneralManagerProjectRiskRequirementResponse
            {
                RequirementId = requirement.Id,
                RoleName = requirement.RoleName,
                Quantity = requirement.Quantity,
                CoverageScore = RoundScore(coverageScore * 100m),
                BestCandidateScore = RoundScore(bestCandidateScore * 100m),
                GapSummary = coverageScore >= 0.75m ? "Healthy" : coverageScore >= 0.5m ? "Moderate" : "Critical"
            });
        }

        var coverageScorePercent = totalCoverage / totalRequiredResources * 100m;
        var staffingRiskScore = Math.Clamp((100m - coverageScorePercent) * 0.7m + averageWorkloadPercent * 0.3m, 0m, 100m);
        var riskLevel = staffingRiskScore >= 70m ? "High" : staffingRiskScore >= 40m ? "Medium" : "Low";

        if (averageWorkloadPercent > 75m)
        {
            riskReasons.Add("Average workload is above 75%.");
        }

        if (activeCandidateCount < totalRequiredResources)
        {
            riskReasons.Add("Available candidate count is below required resources.");
        }

        return new GetGeneralManagerProjectRiskResponse
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            StaffingRiskScore = RoundScore(staffingRiskScore),
            CoverageScore = RoundScore(coverageScorePercent),
            RiskLevel = riskLevel,
            RequiredResourceCount = totalRequiredResources,
            ActiveCandidateCount = activeCandidateCount,
            AverageWorkloadPercent = RoundScore(averageWorkloadPercent),
            MainRiskReasons = riskReasons.Distinct().ToList(),
            Requirements = requirementBreakdown
        };
    }

    private static decimal CalculateRequirementScore(Employee employee, IReadOnlyCollection<Skill> requiredSkills)
    {
        if (requiredSkills.Count == 0)
        {
            return 1m;
        }

        var employeeSkills = employee.EmployeeSkills.ToDictionary(item => item.SkillId, item => item);
        var total = 0m;

        foreach (var skill in requiredSkills)
        {
            if (!employeeSkills.TryGetValue(skill.Id, out var employeeSkill))
            {
                continue;
            }

            var explicitScore = Math.Min(employeeSkill.Proficiency / (decimal)GeneralManagerPredictionConstants.ExplicitSkillMaxProficiency, 1m);
            var primaryBonus = employeeSkill.IsPrimary ? GeneralManagerPredictionConstants.PrimarySkillBonus : 0m;
            total += Math.Min(explicitScore + primaryBonus, 1m);
        }

        return Math.Min(total / requiredSkills.Count, 1m);
    }

    private static decimal RoundScore(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
