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

    private const decimal MinimumEligibleSkillScorePercent = 30m;

    private static readonly HashSet<string> RoleStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "and",
        "the",
        "for",
        "with",
        "resource",
        "staff",
    };

    private static readonly Dictionary<string, string[]> DepartmentHintMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["backend"] = ["engineering", "technology", "it"],
        ["frontend"] = ["engineering", "technology", "it"],
        ["fullstack"] = ["engineering", "technology", "it"],
        ["developer"] = ["engineering", "technology", "it"],
        ["engineer"] = ["engineering", "technology", "it"],
        ["qa"] = ["engineering", "technology", "it"],
        ["tester"] = ["engineering", "technology", "it"],
        ["devops"] = ["engineering", "technology", "it"],
        ["sre"] = ["engineering", "technology", "it"],
        ["data"] = ["engineering", "technology", "it"],
        ["design"] = ["design", "product"],
        ["ui"] = ["design", "product"],
        ["ux"] = ["design", "product"],
        ["marketing"] = ["marketing"],
        ["sales"] = ["sales", "business"],
        ["hr"] = ["hr", "human resource"],
        ["recruit"] = ["hr", "human resource"],
        ["finance"] = ["finance", "accounting"],
        ["account"] = ["finance", "accounting"],
    };

    private static readonly AssignmentStatus[] ActiveAssignmentStatuses =
    {
        AssignmentStatus.Pending,
        AssignmentStatus.Approved,
        AssignmentStatus.Accepted,
        AssignmentStatus.InProgress,
    };

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
            .Include(item => item.Contracts)
            .ToListAsync(cancellationToken);

        employees = employees
            .Where(item => !item.Contracts.Any() || item.Contracts.Any(c => c.Status is ContractStatus.Active or ContractStatus.Extended))
            .Where(item => item.UserId != project.PmOwnerUserId)
            .ToList();

        var assignedCountByRole = await _dbContext.Assignments
            .AsNoTracking()
            .Where(item => item.ProjectId == project.Id)
            .Where(item => ActiveAssignmentStatuses.Contains(item.Status))
            .Where(item => item.AllocationPercent > 0)
            .GroupBy(item => item.RoleName.ToLower())
            .Select(group => new
            {
                RoleName = group.Key,
                AssignedCount = group.Select(item => item.EmployeeId).Distinct().Count()
            })
            .ToDictionaryAsync(item => item.RoleName, item => item.AssignedCount, cancellationToken);

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
        var averageTimelineWorkloadPercent = employees.Count == 0
            ? 0m
            : employees.Average(item => timelineWorkloadByEmployee.TryGetValue(item.Id, out var workload) ? workload : 0m);

        foreach (var requirement in project.ResourceRequirements.OrderBy(item => item.SortOrder))
        {
            var assignedCountForRequirement = assignedCountByRole.TryGetValue(requirement.RoleName.ToLower(), out var assignedCount)
                ? assignedCount
                : 0;

            var requirementResponse = BuildRequirementPrediction(
                requirement,
                employees,
                timelineWorkloadByEmployee,
                historicalSkillExposure,
                completedAssignmentsByEmployee,
                completedAssignmentsByRole,
                assignedCountForRequirement,
                candidateLimit);

            requirementResponses.Add(requirementResponse);
            weightedCoverageTotal += requirementResponse.CoverageScore * requirement.Quantity;
            weightedQuantityTotal += requirement.Quantity;
        }

        var overallCoverageScore = weightedQuantityTotal > 0
            ? weightedCoverageTotal / weightedQuantityTotal
            : 0m;

        var staffingRiskScore = Math.Clamp((100m - overallCoverageScore) * 0.7m + averageTimelineWorkloadPercent * 0.3m, 0m, 100m);
        var activeCandidateCount = employees.Count(item =>
            Math.Max(0m, 100m - (timelineWorkloadByEmployee.TryGetValue(item.Id, out var workload) ? workload : 0m)) > 0m);

        return new GetGeneralManagerProjectPredictionResponse
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            OverallCoverageScore = RoundScore(overallCoverageScore),
            StaffingRiskScore = RoundScore(staffingRiskScore),
            RequiredResourceCount = project.ResourceRequirements.Sum(item => item.Quantity),
            CandidatePoolSize = activeCandidateCount,
            CandidateLimit = candidateLimit,
            Requirements = requirementResponses
        };
    }

    private static GeneralManagerProjectRequirementPredictionResponse BuildRequirementPrediction(
        ProjectResourceRequirement requirement,
        IReadOnlyCollection<Employee> employees,
        IReadOnlyDictionary<Guid, decimal> timelineWorkloadByEmployee,
        IReadOnlyDictionary<(Guid EmployeeId, Guid SkillId), decimal> historicalSkillExposure,
        IReadOnlyDictionary<Guid, int> completedAssignmentsByEmployee,
        IReadOnlyDictionary<(Guid EmployeeId, string RoleName), int> completedAssignmentsByRole,
        int assignedCountForRequirement,
        int candidateLimit)
    {
        var fulfilledSlots = Math.Min(Math.Max(0, assignedCountForRequirement), requirement.Quantity);
        var remainingSlots = Math.Max(0, requirement.Quantity - fulfilledSlots);

        if (remainingSlots == 0)
        {
            return new GeneralManagerProjectRequirementPredictionResponse
            {
                RequirementId = requirement.Id,
                RoleName = requirement.RoleName,
                Quantity = requirement.Quantity,
                ExperienceLevel = requirement.ExperienceLevel.ToString(),
                RequiredSkills = requirement.RequiredSkills
                    .Select(item => item.Skill.Name)
                    .Distinct()
                    .OrderBy(item => item)
                    .ToList(),
                CoverageScore = 100m,
                BestCandidateScore = 0m,
                RecommendedCandidates = []
            };
        }

        var requiredSkillItems = requirement.RequiredSkills
            .Select(item => item.Skill)
            .DistinctBy(item => item.Id)
            .ToList();

        var roleKeywords = BuildRoleKeywords(requirement.RoleName, requiredSkillItems);
        var departmentHints = BuildDepartmentHints(roleKeywords);

        var roleAndDepartmentCandidates = employees
            .Where(employee => EmployeeMatchesRoleAndDepartment(employee, roleKeywords, departmentHints))
            .ToList();

        var roleOnlyCandidates = employees
            .Where(employee => EmployeeMatchesRole(employee, roleKeywords))
            .ToList();

        var candidatePool = roleAndDepartmentCandidates.Count > 0
            ? roleAndDepartmentCandidates
            : roleOnlyCandidates.Count > 0
                ? roleOnlyCandidates
                : employees;

        var candidateResponses = candidatePool
            .Select(employee => BuildCandidatePrediction(
                employee,
                requirement.RoleName,
                requiredSkillItems,
                timelineWorkloadByEmployee.TryGetValue(employee.Id, out var workload) ? workload : 0m,
                historicalSkillExposure,
                completedAssignmentsByEmployee,
                completedAssignmentsByRole))
            .OrderByDescending(item => item.FitScore)
            .ThenBy(item => item.FullName)
            .ToList();

        var eligibleCandidateResponses = requiredSkillItems.Count > 0
            ? candidateResponses
                .Where(item => item.MatchedSkills.Count > 0)
                .Where(item => item.SkillScore >= MinimumEligibleSkillScorePercent)
                .ToList()
            : candidateResponses;

        var recommendationLimit = Math.Min(candidateLimit, remainingSlots);
        var recommendedCandidates = eligibleCandidateResponses.Take(recommendationLimit).ToList();
        var bestCandidateScore = eligibleCandidateResponses.Count > 0 ? eligibleCandidateResponses[0].FitScore : 0m;
        var coverageScore = requirement.Quantity > 0
            ? ((fulfilledSlots * 100m) + eligibleCandidateResponses.Take(remainingSlots).Sum(item => item.FitScore)) / requirement.Quantity
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
        decimal timelineWorkloadPercent,
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

        var normalizedWorkloadPercent = Math.Max(0m, timelineWorkloadPercent);
        var normalizedAvailabilityPercent = Math.Max(0m, 100m - normalizedWorkloadPercent);

        var skillScore = totalSkillScore / totalSkillCount;
        var capacityScore = normalizedAvailabilityPercent / 100m;
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
            AvailabilityPercent = RoundScore(normalizedAvailabilityPercent),
            WorkloadPercent = RoundScore(normalizedWorkloadPercent),
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

            var roleMatchedSkills = assignment.Project.ResourceRequirements
                .Where(item => string.Equals(item.RoleName, assignment.RoleName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(item => item.RequiredSkills)
                .Select(item => item.Skill)
                .DistinctBy(item => item.Id)
                .ToList();

            var requiredSkills = roleMatchedSkills.Count > 0
                ? roleMatchedSkills
                : assignment.Project.ResourceRequirements
                    .SelectMany(item => item.RequiredSkills)
                    .Select(item => item.Skill)
                    .DistinctBy(item => item.Id)
                    .ToList();

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

    private static bool EmployeeMatchesRoleAndDepartment(Employee employee, IReadOnlyCollection<string> roleKeywords, IReadOnlyCollection<string> departmentHints)
    {
        return EmployeeMatchesRole(employee, roleKeywords)
            && EmployeeMatchesDepartment(employee, departmentHints);
    }

    private static bool EmployeeMatchesRole(Employee employee, IReadOnlyCollection<string> roleKeywords)
    {
        if (roleKeywords.Count == 0)
        {
            return true;
        }

        var employeeSignals = employee.EmployeeSkills
            .Select(item => item.Skill.Name)
            .Append(employee.JobTitle)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.ToLowerInvariant())
            .ToList();

        return roleKeywords.Any(keyword => employeeSignals.Any(signal => signal.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
    }

    private static bool EmployeeMatchesDepartment(Employee employee, IReadOnlyCollection<string> departmentHints)
    {
        if (departmentHints.Count == 0)
        {
            return true;
        }

        var departmentName = employee.Department?.Name ?? string.Empty;
        return departmentHints.Any(hint => departmentName.Contains(hint, StringComparison.OrdinalIgnoreCase));
    }

    private static List<string> BuildRoleKeywords(string roleName, IReadOnlyCollection<Skill> requiredSkills)
    {
        var roleNameKeywords = roleName
            .Split([' ', '-', '/', '(', ')', ',', '.'], StringSplitOptions.RemoveEmptyEntries)
            .Select(keyword => keyword.Trim().ToLowerInvariant())
            .Where(keyword => keyword.Length >= 3)
            .Where(keyword => !RoleStopWords.Contains(keyword));

        var requiredSkillKeywords = requiredSkills
            .Select(item => item.Name)
            .SelectMany(skill => skill.Split([' ', '-', '/', '(', ')', ',', '.'], StringSplitOptions.RemoveEmptyEntries))
            .Select(keyword => keyword.Trim().ToLowerInvariant())
            .Where(keyword => keyword.Length >= 3)
            .Where(keyword => !RoleStopWords.Contains(keyword));

        return roleNameKeywords
            .Concat(requiredSkillKeywords)
            .Distinct()
            .ToList();
    }

    private static List<string> BuildDepartmentHints(IReadOnlyCollection<string> roleKeywords)
    {
        return roleKeywords
            .SelectMany(keyword => DepartmentHintMap.TryGetValue(keyword, out var hints) ? hints : [])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}