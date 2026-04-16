using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageGeneralManagerPredictions;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectPmRecommendationRequestHandler : IRequestHandler<GetGeneralManagerProjectPmRecommendationRequest, GetGeneralManagerProjectPmRecommendationResponse>
{
    private readonly ApplicationDbContext _dbContext;

    private static readonly ProjectStatus[] ActiveProjectStatuses =
    {
        ProjectStatus.Assigned,
        ProjectStatus.InProgress,
    };

    public GetGeneralManagerProjectPmRecommendationRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetGeneralManagerProjectPmRecommendationResponse> Handle(GetGeneralManagerProjectPmRecommendationRequest request, CancellationToken cancellationToken)
    {
        var candidateLimit = Math.Clamp(request.CandidateLimit ?? GeneralManagerPredictionConstants.DefaultCandidateLimit, 1, GeneralManagerPredictionConstants.MaxCandidateLimit);

        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.Id == request.ProjectId)
            .Include(item => item.ResourceRequirements)
                .ThenInclude(item => item.RequiredSkills)
                    .ThenInclude(item => item.Skill)
            .Include(item => item.ProjectSkills)
                .ThenInclude(item => item.Skill)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");

        var roleCountsByName = await _dbContext.Assignments
            .AsNoTracking()
            .Where(item => item.ProjectId == project.Id)
            .Where(item => item.AllocationPercent > 0)
            .Where(item => item.Status == AssignmentStatus.Pending || item.Status == AssignmentStatus.Approved || item.Status == AssignmentStatus.Accepted || item.Status == AssignmentStatus.InProgress)
            .GroupBy(item => item.RoleName.ToLower())
            .Select(group => new
            {
                RoleName = group.Key,
                EmployeeCount = group.Select(item => item.EmployeeId).Distinct().Count()
            })
            .ToDictionaryAsync(item => item.RoleName, item => item.EmployeeCount, cancellationToken);

        var pmRequirement = project.ResourceRequirements
            .OrderBy(item => item.SortOrder)
            .FirstOrDefault(item => item.RoleName.Contains("project manager", StringComparison.OrdinalIgnoreCase)
                || item.RoleName.Equals("pm", StringComparison.OrdinalIgnoreCase)
                || item.RoleName.Contains(" pm", StringComparison.OrdinalIgnoreCase));

        var pmRequirementAlreadyFull = false;
        if (pmRequirement is not null)
        {
            var assignedPmCount = roleCountsByName.TryGetValue(pmRequirement.RoleName.ToLower(), out var count) ? count : 0;
            pmRequirementAlreadyFull = assignedPmCount >= pmRequirement.Quantity;
        }

        var requiredSkillNames = (pmRequirement?.RequiredSkills ?? [])
            .Select(item => item.Skill.Name)
            .Concat(project.ProjectSkills.Select(item => item.Skill.Name))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .ToList();

        var pmUsers = await _dbContext.Users
            .AsNoTracking()
            .Where(item => item.IsActive)
            .Where(item => item.Role == UserRole.Pm)
            .Include(item => item.Department)
            .Include(item => item.EmployeeProfile)
                .ThenInclude(item => item!.EmployeeSkills)
                    .ThenInclude(item => item.Skill)
            .ToListAsync(cancellationToken);

            var existingTeamMemberUserIds = await _dbContext.Assignments
                .AsNoTracking()
                .Where(item => item.ProjectId == project.Id)
                .Where(item => item.AllocationPercent > 0)
                .Where(item => item.Status == AssignmentStatus.Pending || item.Status == AssignmentStatus.Approved || item.Status == AssignmentStatus.Accepted || item.Status == AssignmentStatus.InProgress)
                .Select(item => item.Employee.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var existingTeamMemberUserSet = existingTeamMemberUserIds.Count == 0
                ? []
                : existingTeamMemberUserIds.ToHashSet();

        var activeProjectCountByPmUserId = await _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.PmOwnerUserId.HasValue)
            .Where(item => ActiveProjectStatuses.Contains(item.Status))
            .GroupBy(item => item.PmOwnerUserId!.Value)
            .Select(group => new
            {
                PmOwnerUserId = group.Key,
                ActiveProjectCount = group.Count()
            })
            .ToDictionaryAsync(item => item.PmOwnerUserId, item => item.ActiveProjectCount, cancellationToken);

        var completedProjectCountByPmUserId = await _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.PmOwnerUserId.HasValue)
            .Where(item => item.Status == ProjectStatus.Completed)
            .GroupBy(item => item.PmOwnerUserId!.Value)
            .Select(group => new
            {
                PmOwnerUserId = group.Key,
                CompletedProjectCount = group.Count()
            })
            .ToDictionaryAsync(item => item.PmOwnerUserId, item => item.CompletedProjectCount, cancellationToken);

        var candidateResponses = pmUsers
            .Where(item => !existingTeamMemberUserSet.Contains(item.Id))
            .Where(item => item.Id != project.PmOwnerUserId)
            .Where(item => {
                var activeCount = activeProjectCountByPmUserId.TryGetValue(item.Id, out var count) ? count : 0;
                return activeCount < 2;
            })
            .Select(item => BuildCandidatePrediction(
                item,
                requiredSkillNames,
                activeProjectCountByPmUserId.TryGetValue(item.Id, out var activeCount) ? activeCount : 0,
                completedProjectCountByPmUserId.TryGetValue(item.Id, out var completedCount) ? completedCount : 0))
            .OrderByDescending(item => item.FitScore)
            .ThenBy(item => item.WorkloadPercent)
            .ThenBy(item => item.FullName)
            .ToList();

        return new GetGeneralManagerProjectPmRecommendationResponse
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            CandidateLimit = candidateLimit,
            CandidatePoolSize = candidateResponses.Count,
            HasPmRequirement = pmRequirement is not null,
            PmRequirementAlreadyFull = pmRequirementAlreadyFull,
            RecommendedPm = candidateResponses.FirstOrDefault(),
            Candidates = candidateResponses.Take(candidateLimit).ToList()
        };
    }

    private static GeneralManagerProjectCandidatePredictionResponse BuildCandidatePrediction(
        AppUser pmUser,
        IReadOnlyCollection<string> requiredSkillNames,
        int activeProjectCount,
        int completedProjectCount)
    {
        var employeeProfile = pmUser.EmployeeProfile;
        var availableSkillNames = employeeProfile?.EmployeeSkills
            .Select(item => item.Skill.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            ?? [];

        var matchedSkills = requiredSkillNames
            .Where(skill => availableSkillNames.Contains(skill, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .ToList();

        var missingSkills = requiredSkillNames
            .Where(skill => !matchedSkills.Contains(skill, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .ToList();

        var skillScore = requiredSkillNames.Count == 0
            ? 85m
            : (matchedSkills.Count / (decimal)requiredSkillNames.Count) * 100m;

        var derivedWorkload = activeProjectCount * 50m;
        var workloadPercent = employeeProfile?.WorkloadPercent ?? Math.Min(derivedWorkload, 100m);
        var availabilityPercent = employeeProfile?.AvailabilityPercent ?? Math.Max(0m, 100m - workloadPercent);
        var capacityScore = Math.Clamp(availabilityPercent, 0m, 100m);

        var historyScore = Math.Min((completedProjectCount / 6m) * 100m, 100m);
        var roleExperienceScore = Math.Min((completedProjectCount / 4m) * 100m, 100m);

        var fitScore = (skillScore * 0.4m)
            + (capacityScore * 0.35m)
            + (historyScore * 0.15m)
            + (roleExperienceScore * 0.10m);

        return new GeneralManagerProjectCandidatePredictionResponse
        {
            EmployeeId = pmUser.Id,
            FullName = pmUser.FullName,
            DepartmentName = pmUser.Department?.Name,
            JobTitle = employeeProfile?.JobTitle ?? "Project Manager",
            FitScore = RoundScore(fitScore),
            SkillScore = RoundScore(skillScore),
            CapacityScore = RoundScore(capacityScore),
            HistoryScore = RoundScore(historyScore),
            RoleExperienceScore = RoundScore(roleExperienceScore),
            AvailabilityPercent = RoundScore(availabilityPercent),
            WorkloadPercent = RoundScore(workloadPercent),
            MatchedSkills = matchedSkills,
            InferredSkills = [],
            MissingSkills = missingSkills,
            Reason = $"active projects {activeProjectCount}/2, completed projects {completedProjectCount}, matched PM skills {matchedSkills.Count}"
        };
    }

    private static decimal RoundScore(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
