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

    public CreateAssignmentRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateAssignmentResponse> Handle(CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects.AnyAsync(project => project.Id == request.ProjectId, cancellationToken);

        if (!projectExists)
        {
            throw new InvalidOperationException("Project does not exist.");
        }

        var targetEmployeeId = request.EmployeeId;

        if (targetEmployeeId == Guid.Empty)
        {
            var requiredSkills = request.RequiredSkills
                .Where(skill => !string.IsNullOrWhiteSpace(skill))
                .Select(skill => skill.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            var candidateEmployees = await _dbContext.Employees
                .AsNoTracking()
                .Include(employee => employee.User)
                .Include(employee => employee.EmployeeSkills)
                    .ThenInclude(employeeSkill => employeeSkill.Skill)
                .Where(employee => employee.Status == EmploymentStatus.Active)
                .ToListAsync(cancellationToken);

            var rankedCandidates = candidateEmployees
                .Select(employee =>
                {
                    var normalizedEmployeeSkills = employee.EmployeeSkills
                        .Select(employeeSkill => employeeSkill.Skill.Name.ToLowerInvariant())
                        .ToList();

                    var matchedSkillsCount = requiredSkills.Count(requiredSkill =>
                        normalizedEmployeeSkills.Any(employeeSkill =>
                            employeeSkill.Contains(requiredSkill) || requiredSkill.Contains(employeeSkill)));

                    var skillCoverage = requiredSkills.Count == 0
                        ? 0
                        : (matchedSkillsCount / (decimal)requiredSkills.Count) * 100m;

                    var score = skillCoverage + employee.AvailabilityPercent - employee.WorkloadPercent;

                    return new
                    {
                        EmployeeId = employee.Id,
                        Score = score,
                        MatchedSkillsCount = matchedSkillsCount,
                    };
                })
                .OrderByDescending(candidate => candidate.MatchedSkillsCount)
                .ThenByDescending(candidate => candidate.Score)
                .ToList();

            var selectedCandidate = rankedCandidates.FirstOrDefault();

            if (selectedCandidate is null)
            {
                throw new InvalidOperationException("No employee available for assignment request.");
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
            AllocationPercent = request.AllocationPercent,
            Status = AssignmentStatus.Pending,
            ConflictWarning = metadataNotes.Count == 0 ? null : string.Join(" | ", metadataNotes)
        };

        _dbContext.Assignments.Add(assignment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateAssignmentResponse
        {
            AssignmentId = assignment.Id
        };
    }
}