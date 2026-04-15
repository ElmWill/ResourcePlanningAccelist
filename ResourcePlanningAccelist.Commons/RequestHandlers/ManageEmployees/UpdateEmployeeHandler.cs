using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class UpdateEmployeeHandler : IRequestHandler<UpdateEmployeeRequest, UpdateEmployeeResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateEmployeeHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateEmployeeResponse> Handle(UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(item => item.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return new UpdateEmployeeResponse
            {
                Success = false,
                Message = "Employee not found."
            };
        }

        // Update User info
        employee.User.FullName = request.FullName;
        employee.User.DepartmentId = request.DepartmentId;

        // Update Employee profile
        employee.EmployeeCode = request.EmployeeCode;
        employee.Phone = request.Phone;
        employee.Location = request.Location;
        employee.DepartmentId = request.DepartmentId;
        employee.JobTitle = request.JobTitle;
        if (Enum.TryParse<EmploymentStatus>(request.Status, true, out var status))
        {
            employee.Status = status;
        }
        employee.HireDate = request.HireDate;

        // Update Skills (Sync)
        if (request.Skills != null)
        {
            var currentSkills = await _dbContext.EmployeeSkills
                .Include(es => es.Skill)
                .Where(es => es.EmployeeId == employee.Id)
                .ToListAsync(cancellationToken);

            // 1. Remove skills not in the request
            var skillsToRemove = currentSkills
                .Where(cs => !request.Skills.Contains(cs.Skill.Name))
                .ToList();

            if (skillsToRemove.Any())
            {
                _dbContext.EmployeeSkills.RemoveRange(skillsToRemove);
            }

            // 2. Add skills from request that aren't already there
            var skillNamesToAdd = request.Skills
                .Where(name => !currentSkills.Any(cs => cs.Skill.Name == name))
                .ToList();

            if (skillNamesToAdd.Any())
            {
                var existingSkillsInDb = await _dbContext.Skills
                    .Where(s => skillNamesToAdd.Contains(s.Name))
                    .ToDictionaryAsync(s => s.Name, s => s, cancellationToken);

                var newSkillsToCreate = new List<Skill>();
                var newEmployeeSkills = new List<EmployeeSkill>();

                foreach (var skillName in skillNamesToAdd)
                {
                    if (!existingSkillsInDb.TryGetValue(skillName, out var skill))
                    {
                        skill = new Skill { Name = skillName, Category = SkillCategory.Technical };
                        newSkillsToCreate.Add(skill);
                        existingSkillsInDb[skillName] = skill; // Prevent duplicates
                    }

                    newEmployeeSkills.Add(new EmployeeSkill
                    {
                        EmployeeId = employee.Id,
                        Skill = skill,
                        Proficiency = 3
                    });
                }

                if (newSkillsToCreate.Any()) _dbContext.Skills.AddRange(newSkillsToCreate);
                if (newEmployeeSkills.Any()) _dbContext.EmployeeSkills.AddRange(newEmployeeSkills);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateEmployeeResponse
        {
            Success = true,
            Message = "Employee profile updated successfully."
        };
    }
}
