using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class CreateEmployeeHandler : IRequestHandler<CreateEmployeeRequest, CreateEmployeeResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateEmployeeHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateEmployeeResponse> Handle(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        // Consider normalize email pakai ToLower
        // Check if user with same email exists
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            return new CreateEmployeeResponse
            {
                Success = false,
                Message = $"A user with email {request.Email} already exists."
            };
        }

        // 1. Create the AppUser first (as requested: "yes immediately add account")
        var user = new AppUser
        {
            Email = request.Email,
            FullName = request.FullName,
            Role = UserRole.Employee,
            DepartmentId = request.DepartmentId,
            IsActive = true
        };
        _dbContext.Users.Add(user);

        // 2. Create the Employee profile linked to the user
        var employee = new Employee
        {
            UserId = user.Id, // EF will handle the relation
            User = user,
            EmployeeCode = request.EmployeeCode,
            Phone = request.Phone,
            Location = request.Location,
            DepartmentId = request.DepartmentId,
            JobTitle = request.JobTitle,
            Status = Enum.TryParse<EmploymentStatus>(request.Status, true, out var status) ? status : EmploymentStatus.Active,
            HireDate = request.HireDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            AvailabilityPercent = 100,
            WorkloadPercent = 0,
            AssignedHours = 0
        };
        _dbContext.Employees.Add(employee);

        var now = DateTime.UtcNow;

        var contract = new EmployeeContract
        {
            Id = Guid.NewGuid(),
            Employee = employee,
            EmployeeId = employee.Id,
            Notes = "Contract Initialized by HR",
            CreatedAt = now,
            CreatedBy = "73101e8c-98a8-489d-ae4d-2e549eec1d85", //hardcoded HR
            StartDate = employee.HireDate,
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            Status = ContractStatus.Active,
            UpdatedAt = now,
            UpdatedBy = "73101e8c-98a8-489d-ae4d-2e549eec1d85"
        };
        _dbContext.EmployeeContracts.Add(contract);

        // Add Skills
        if (request.Skills != null && request.Skills.Any())
        {
            // Kalau looping, lebih baik add ke var dulu baru nanti pakai AddRange
            foreach (var skillName in request.Skills)
            {
                var skill = await _dbContext.Skills
                    .FirstOrDefaultAsync(s => s.Name == skillName, cancellationToken);

                if (skill == null)
                {
                    skill = new Skill { Name = skillName, Category = SkillCategory.Technical };
                    _dbContext.Skills.Add(skill);
                }

                _dbContext.EmployeeSkills.Add(new EmployeeSkill
                {
                    Employee = employee,
                    Skill = skill,
                    Proficiency = 3
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateEmployeeResponse
        {
            EmployeeId = employee.Id,
            Success = true,
            Message = "Employee profile and user account created successfully."
        };
    }
}
