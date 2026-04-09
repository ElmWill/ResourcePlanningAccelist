using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectPredictionRequestHandler : IRequestHandler<GetGeneralManagerProjectPredictionRequest, GetGeneralManagerProjectPredictionResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetGeneralManagerProjectPredictionRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetGeneralManagerProjectPredictionResponse> Handle(GetGeneralManagerProjectPredictionRequest request, CancellationToken cancellationToken)
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
            .Include(item => item.User)
            .Include(item => item.Department)
            .Include(item => item.EmployeeSkills)
                .ThenInclude(item => item.Skill)
            .Include(item => item.Contract)
            .ToListAsync(cancellationToken);

        employees = employees
            .Where(item => item.Contract == null || item.Contract.Status is ContractStatus.Active or ContractStatus.Extended)
            .ToList();

        var historicalAssignments = await _dbContext.Assignments
            .AsNoTracking()
            .Where(item => item.Status == AssignmentStatus.Completed)
            .Include(item => item.Project)
                .ThenInclude(item => item.ResourceRequirements)
                    .ThenInclude(item => item.RequiredSkills)
                        .ThenInclude(item => item.Skill)
            .ToListAsync(cancellationToken);

        var historicalSkillExposure = BuildHistoricalSkillExposure(historicalAssignments);
        var completedAssignmentsByEmployee = historicalAssignments
            .GroupBy(item => item.EmployeeId)
            .ToDictionary(group => group.Key, group => group.Count());

        var completedAssignmentsByRole = historicalAssignments
            .GroupBy(item => new { item.EmployeeId, item.RoleName })
            .ToDictionary(group => (group.Key.EmployeeId, group.Key.RoleName), group => group.Count());

        var requirementResponses = new List<GeneralManagerProjectRequirementPredictionResponse>();
        var weightedCoverageTotal = 0m;
        var weightedQuantityTotal = 0m;

        foreach (var requirement in project.ResourceRequirements.OrderBy(item => item.SortOrder))
        {
            var requirementResponse = BuildRequirementPrediction(
                requirement,
                employees,
                historicalSkillExposure,
                completedAssignmentsByEmployee,
                completedAssignmentsByRole,
                candidateLimit);

            requirementResponses.Add(requirementResponse);
            weightedCoverageTotal += requirementResponse.CoverageScore * requirement.Quantity;
            weightedQuantityTotal += requirement.Quantity;
        }

        var overallCoverageScore = weightedQuantityTotal > 0
            ? weightedCoverageTotal / weightedQuantityTotal
            : 0m;

        return new GetGeneralManagerProjectPredictionResponse
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            OverallCoverageScore = RoundScore(overallCoverageScore),
            StaffingRiskScore = RoundScore(100m - overallCoverageScore),
            RequiredResourceCount = project.ResourceRequirements.Sum(item => item.Quantity),
            CandidatePoolSize = employees.Count,
            CandidateLimit = candidateLimit,
            Requirements = requirementResponses
        };
    }

    private static GeneralManagerProjectRequirementPredictionResponse BuildRequirementPrediction(
        ProjectResourceRequirement requirement,
        IReadOnlyCollection<Employee> employees,
        IReadOnlyDictionary<(Guid EmployeeId, Guid SkillId), decimal> historicalSkillExposure,
        IReadOnlyDictionary<Guid, int> completedAssignmentsByEmployee,
        IReadOnlyDictionary<(Guid EmployeeId, string RoleName), int> completedAssignmentsByRole,
        int candidateLimit)
    {
        var requiredSkillItems = requirement.RequiredSkills
            .Select(item => item.Skill)
            .DistinctBy(item => item.Id)
            .ToList();

        var candidateResponses = employees
            .Select(employee => BuildCandidatePrediction(
                employee,
                requirement.RoleName,
                requiredSkillItems,
                historicalSkillExposure,
                completedAssignmentsByEmployee,
                completedAssignmentsByRole))
            .OrderByDescending(item => item.FitScore)
            .ThenBy(item => item.FullName)
            .ToList();

        var recommendedCandidates = candidateResponses.Take(candidateLimit).ToList();
        var bestCandidateScore = recommendedCandidates.Count > 0 ? recommendedCandidates[0].FitScore : 0m;
        var coverageScore = recommendedCandidates.Count > 0
            ? recommendedCandidates.Take(requirement.Quantity).Average(item => item.FitScore)
            : 0m;

        return new GeneralManagerProjectRequirementPredictionResponse
        {
            RequirementId = requirement.Id,
            RoleName = requirement.RoleName,
            Quantity = requirement.Quantity,
            ExperienceLevel = requirement.ExperienceLevel.ToString(),
            RequiredSkills = requiredSkillItems.Select(item => item.Name).OrderBy(item => item).ToList(),
            CoverageScore = RoundScore(coverageScore),
            BestCandidateScore = RoundScore(bestCandidateScore),
            RecommendedCandidates = recommendedCandidates
        };
    }

    private static GeneralManagerProjectCandidatePredictionResponse BuildCandidatePrediction(
        Employee employee,
        string requirementRoleName,
        IReadOnlyCollection<Skill> requiredSkills,
        IReadOnlyDictionary<(Guid EmployeeId, Guid SkillId), decimal> historicalSkillExposure,
        IReadOnlyDictionary<Guid, int> completedAssignmentsByEmployee,
        IReadOnlyDictionary<(Guid EmployeeId, string RoleName), int> completedAssignmentsByRole)
    {
        var employeeSkills = employee.EmployeeSkills.ToDictionary(item => item.SkillId, item => item);
        var matchedSkills = new List<string>();
        var inferredSkills = new List<string>();
        var missingSkills = new List<string>();

        var totalSkillScore = 0m;
        var totalSkillCount = requiredSkills.Count == 0 ? 1 : requiredSkills.Count;

        foreach (var skill in requiredSkills)
        {
            employeeSkills.TryGetValue(skill.Id, out var employeeSkill);
            historicalSkillExposure.TryGetValue((employee.Id, skill.Id), out var historicalExposure);

            var explicitSkillScore = employeeSkill is null
                ? 0m
                : Math.Min(employeeSkill.Proficiency / (decimal)GeneralManagerPredictionConstants.ExplicitSkillMaxProficiency, 1m)
                    + (employeeSkill.IsPrimary ? GeneralManagerPredictionConstants.PrimarySkillBonus : 0m);

            explicitSkillScore = Math.Min(explicitSkillScore, 1m);

            var historicalSkillScore = historicalExposure <= 0m
                ? 0m
                : historicalExposure / (historicalExposure + GeneralManagerPredictionConstants.HistoryNormalizationFactor);

            var combinedSkillScore = (explicitSkillScore * 0.65m) + (historicalSkillScore * 0.35m);
            totalSkillScore += combinedSkillScore;

            if (explicitSkillScore > 0.4m || historicalSkillScore > 0.4m)
            {
                matchedSkills.Add(skill.Name);
            }

            if (employeeSkill is null && historicalSkillScore > 0.3m)
            {
                inferredSkills.Add(skill.Name);
            }

            if (combinedSkillScore < 0.25m)
            {
                missingSkills.Add(skill.Name);
            }
        }

        var skillScore = totalSkillScore / totalSkillCount;
        var capacityScore = Math.Max(0m, 1m - (employee.WorkloadPercent / 100m)) * (employee.AvailabilityPercent / 100m);
        var completedAssignments = completedAssignmentsByEmployee.TryGetValue(employee.Id, out var completedCount)
            ? completedCount
            : 0;
        var historyScore = Math.Min(completedAssignments / GeneralManagerPredictionConstants.CompletionNormalizationAssignments, 1m);

        var completedSameRole = completedAssignmentsByRole.TryGetValue((employee.Id, requirementRoleName), out var sameRoleCount)
            ? sameRoleCount
            : 0;
        var roleExperienceScore = Math.Min(completedSameRole / GeneralManagerPredictionConstants.ExperienceNormalizationAssignments, 1m);

        var fitScore = (skillScore * GeneralManagerPredictionConstants.SkillWeight)
            + (capacityScore * GeneralManagerPredictionConstants.CapacityWeight)
            + (historyScore * GeneralManagerPredictionConstants.HistoryWeight)
            + (roleExperienceScore * GeneralManagerPredictionConstants.RoleExperienceWeight);

        var reasonParts = new List<string>
        {
            $"skills {RoundScore(skillScore * 100m)}%",
            $"capacity {RoundScore(capacityScore * 100m)}%",
            $"history {RoundScore(historyScore * 100m)}%",
            $"role experience {RoundScore(roleExperienceScore * 100m)}%"
        };

        return new GeneralManagerProjectCandidatePredictionResponse
        {
            EmployeeId = employee.Id,
            FullName = employee.User.FullName,
            DepartmentName = employee.Department?.Name,
            JobTitle = employee.JobTitle,
            FitScore = RoundScore(fitScore * 100m),
            SkillScore = RoundScore(skillScore * 100m),
            CapacityScore = RoundScore(capacityScore * 100m),
            HistoryScore = RoundScore(historyScore * 100m),
            RoleExperienceScore = RoundScore(roleExperienceScore * 100m),
            AvailabilityPercent = employee.AvailabilityPercent,
            WorkloadPercent = employee.WorkloadPercent,
            MatchedSkills = matchedSkills.Distinct().OrderBy(item => item).ToList(),
            InferredSkills = inferredSkills.Distinct().OrderBy(item => item).ToList(),
            MissingSkills = missingSkills.Distinct().OrderBy(item => item).ToList(),
            Reason = string.Join(", ", reasonParts)
        };
    }

    private static Dictionary<(Guid EmployeeId, Guid SkillId), decimal> BuildHistoricalSkillExposure(IEnumerable<Assignment> historicalAssignments)
    {
        var exposure = new Dictionary<(Guid EmployeeId, Guid SkillId), decimal>();

        foreach (var assignment in historicalAssignments)
        {
            var referenceDate = assignment.UpdatedAt ?? assignment.CreatedAt;
            var ageDays = Math.Max((DateTimeOffset.UtcNow - referenceDate).TotalDays, 0);
            var decay = 1m / (1m + (decimal)ageDays / GeneralManagerPredictionConstants.HistoricalDecayWindowDays);

            var requiredSkills = assignment.Project.ResourceRequirements
                .SelectMany(item => item.RequiredSkills)
                .Select(item => item.Skill)
                .DistinctBy(item => item.Id);

            foreach (var skill in requiredSkills)
            {
                var key = (assignment.EmployeeId, skill.Id);
                exposure[key] = exposure.TryGetValue(key, out var existing)
                    ? existing + decay
                    : decay;
            }
        }

        return exposure;
    }

    private static decimal RoundScore(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}