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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateEmployeeResponse
        {
            EmployeeId = employee.Id,
            Success = true,
            Message = "Employee profile and user account created successfully."
        };
    }
}
