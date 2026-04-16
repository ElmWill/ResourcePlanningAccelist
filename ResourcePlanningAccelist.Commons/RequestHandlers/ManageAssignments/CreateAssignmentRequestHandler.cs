using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;

public class CreateAssignmentRequestHandler : IRequestHandler<CreateAssignmentRequest, CreateAssignmentResponse>
{
    private readonly ApplicationDbContext _dbContext;

    private static readonly AssignmentStatus[] ActiveAssignmentStatuses =
    {
        AssignmentStatus.Pending,
        AssignmentStatus.GmApproved,
        AssignmentStatus.Approved,
        AssignmentStatus.Accepted,
        AssignmentStatus.InProgress,
    };

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

    public CreateAssignmentRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateAssignmentResponse> Handle(CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(project => project.Id == request.ProjectId, cancellationToken);

        if (project is null)
        {
            throw new InvalidOperationException("Project does not exist.");
        }

    var targetEmployeeId = request.EmployeeId;
    var isSystemRecommendation = targetEmployeeId == Guid.Empty;
    var assignmentAllocationPercent = isSystemRecommendation ? 0m : request.AllocationPercent;

        if (targetEmployeeId == Guid.Empty)
        {
            var requiredSkills = request.RequiredSkills
                .Where(skill => !string.IsNullOrWhiteSpace(skill))
                .Select(skill => skill.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            var roleKeywords = request.RoleName
                .Split([' ', '-', '/', '(', ')', ',', '.'], StringSplitOptions.RemoveEmptyEntries)
                .Select(keyword => keyword.Trim().ToLowerInvariant())
                .Where(keyword => keyword.Length >= 3)
                .Where(keyword => !RoleStopWords.Contains(keyword))
                .Distinct()
                .ToList();

            var requiredSkillKeywords = requiredSkills
                .SelectMany(skill => skill.Split([' ', '-', '/', '(', ')', ',', '.'], StringSplitOptions.RemoveEmptyEntries))
                .Select(keyword => keyword.Trim().ToLowerInvariant())
                .Where(keyword => keyword.Length >= 3)
                .Where(keyword => !RoleStopWords.Contains(keyword))
                .Distinct()
                .ToList();

            var departmentHints = roleKeywords
                .Concat(requiredSkillKeywords)
                .Distinct()
                .SelectMany(keyword => DepartmentHintMap.TryGetValue(keyword, out var hints) ? hints : [])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var allocationByEmployee = await _dbContext.Assignments
                .AsNoTracking()
                .Where(assignment => ActiveAssignmentStatuses.Contains(assignment.Status))
                .Where(assignment => assignment.StartDate <= request.EndDate && assignment.EndDate >= request.StartDate)
                .GroupBy(assignment => assignment.EmployeeId)
                .Select(group => new
                {
                    EmployeeId = group.Key,
                    AllocationPercent = group.Sum(assignment => assignment.AllocationPercent)
                })
                .ToDictionaryAsync(item => item.EmployeeId, item => Math.Max(0m, item.AllocationPercent), cancellationToken);

            var sameProjectEmployeeIds = await _dbContext.Assignments
                .AsNoTracking()
                .Where(assignment => assignment.ProjectId == request.ProjectId)
                .Where(assignment => ActiveAssignmentStatuses.Contains(assignment.Status))
                .Select(assignment => assignment.EmployeeId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var candidateEmployees = await _dbContext.Employees
                .AsNoTracking()
                .Include(employee => employee.User)
                .Include(employee => employee.EmployeeSkills)
                    .ThenInclude(employeeSkill => employeeSkill.Skill)
                .Include(employee => employee.Department)
                .Include(employee => employee.Contracts)
                .Where(employee => employee.Status == EmploymentStatus.Active)
                .ToListAsync(cancellationToken);

            candidateEmployees = candidateEmployees
                .Where(employee =>
                    !employee.Contracts.Any() ||
                    employee.Contracts.Any(contract => contract.Status is ContractStatus.Active or ContractStatus.Extended))
                .Where(employee => !sameProjectEmployeeIds.Contains(employee.Id))
                .Where(employee =>
                {
                    var existingAllocation = allocationByEmployee.TryGetValue(employee.Id, out var allocation)
                        ? allocation
                        : 0m;
                    var remainingCapacity = Math.Max(0m, 100m - existingAllocation);
                    return remainingCapacity >= assignmentAllocationPercent;
                })
                .ToList();

            var candidateScores = candidateEmployees
                .Select(employee =>
                {
                    var existingAllocation = allocationByEmployee.TryGetValue(employee.Id, out var allocation)
                        ? allocation
                        : 0m;
                    var remainingCapacity = Math.Max(0m, 100m - existingAllocation);

                    var normalizedEmployeeSkills = employee.EmployeeSkills
                        .Select(employeeSkill => employeeSkill.Skill.Name.ToLowerInvariant())
                        .Append(employee.JobTitle.ToLowerInvariant())
                        .Where(skill => !string.IsNullOrWhiteSpace(skill))
                        .Distinct()
                        .ToList();

                    var roleMatch = roleKeywords.Count == 0 || roleKeywords.Any(keyword =>
                        employee.JobTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        normalizedEmployeeSkills.Any(employeeSkill => employeeSkill.Contains(keyword, StringComparison.OrdinalIgnoreCase)));

                    var departmentName = employee.Department?.Name ?? string.Empty;
                    var departmentMatch = departmentHints.Count == 0 || departmentHints.Any(hint =>
                        departmentName.Contains(hint, StringComparison.OrdinalIgnoreCase));

                    var matchedSkillsCount = requiredSkills.Count(requiredSkill =>
                    {
                        var requiredTokens = requiredSkill
                            .Split([' ', '-', '/', '(', ')', ',', '.'], StringSplitOptions.RemoveEmptyEntries)
                            .Select(token => token.Trim())
                            .Where(token => token.Length >= 3)
                            .ToList();

                        return normalizedEmployeeSkills.Any(employeeSkill =>
                            employeeSkill.Equals(requiredSkill, StringComparison.OrdinalIgnoreCase) ||
                            employeeSkill.Contains(requiredSkill, StringComparison.OrdinalIgnoreCase) ||
                            (requiredTokens.Count > 0 && requiredTokens.All(token => employeeSkill.Contains(token, StringComparison.OrdinalIgnoreCase))));
                    });

                    var skillCoverage = requiredSkills.Count == 0
                        ? 0
                        : (matchedSkillsCount / (decimal)requiredSkills.Count) * 100m;

                    var score = skillCoverage + remainingCapacity;

                    return new
                    {
                        EmployeeId = employee.Id,
                        Score = score,
                        MatchedSkillsCount = matchedSkillsCount,
                        RoleMatch = roleMatch,
                        DepartmentMatch = departmentMatch,
                    };
                })
                .ToList();

            var rankedCandidates = candidateScores
                .Where(candidate => candidate.RoleMatch)
                .Where(candidate => candidate.DepartmentMatch)
                .Where(candidate => requiredSkills.Count == 0 || candidate.MatchedSkillsCount > 0)
                .OrderByDescending(candidate => candidate.MatchedSkillsCount)
                .ThenByDescending(candidate => candidate.Score)
                .ToList();

            if (rankedCandidates.Count == 0 && requiredSkills.Count == 0)
            {
                rankedCandidates = candidateScores
                    .Where(candidate => candidate.RoleMatch)
                    .Where(candidate => candidate.DepartmentMatch)
                    .OrderByDescending(candidate => candidate.Score)
                    .ToList();
            }

            if (rankedCandidates.Count == 0 && requiredSkills.Count == 0)
            {
                rankedCandidates = candidateScores
                    .Where(candidate => candidate.DepartmentMatch)
                    .OrderByDescending(candidate => candidate.Score)
                    .ToList();
            }

            var selectedCandidate = rankedCandidates.FirstOrDefault();

            if (selectedCandidate is null)
            {
                throw new InvalidOperationException("No suitable employee available for the requested role.");
            }

            targetEmployeeId = selectedCandidate.EmployeeId;
        }
        else
        {
            var employeeExists = await _dbContext.Employees.AnyAsync(employee => employee.Id == targetEmployeeId, cancellationToken);
            if (!employeeExists)
            {
                throw new InvalidOperationException("Employee does not exist.");
            }
        }

        var metadataNotes = new List<string>();
        if (request.RequiredSkills.Count > 0)
        {
            metadataNotes.Add($"Requested skills: {string.Join(", ", request.RequiredSkills)}");
        }

        if (!string.IsNullOrWhiteSpace(request.AdditionalNeeds))
        {
            metadataNotes.Add($"Additional needs: {request.AdditionalNeeds.Trim()}");
        }

        var assignment = new Assignment
        {
            ProjectId = request.ProjectId,
            EmployeeId = targetEmployeeId,
            AssignedByUserId = request.AssignedByUserId,
            RoleName = request.RoleName,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AllocationPercent = assignmentAllocationPercent,
            Status = AssignmentStatus.Pending,
            ConflictWarning = metadataNotes.Count == 0 ? null : string.Join(" | ", metadataNotes)
        };

        var currentActiveDistinctMembers = await _dbContext.Assignments
            .AsNoTracking()
            .Where(item => item.ProjectId == request.ProjectId)
            .Where(item => ActiveAssignmentStatuses.Contains(item.Status))
            .Select(item => item.EmployeeId)
            .Distinct()
            .CountAsync(cancellationToken);

        if (currentActiveDistinctMembers >= project.TotalRequiredResources)
        {
            project.TotalRequiredResources = currentActiveDistinctMembers + 1;
        }

        _dbContext.Assignments.Add(assignment);

        await AssignmentWorkloadUpdater.RecalculateEmployeeWorkloadAsync(_dbContext, assignment.EmployeeId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateAssignmentResponse
        {
            AssignmentId = assignment.Id
        };
    }
}