using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Infrastructure.Services;

internal static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken) ||
            await dbContext.Projects.AnyAsync(cancellationToken) ||
            await dbContext.Employees.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipping development seed because data already exists.");
            return;
        }

        var engineeringDepartment = new Department
        {
            Name = "Engineering",
            Description = "Engineering department"
        };

        var marketingDepartment = new Department
        {
            Name = "Marketing",
            Description = "Marketing department"
        };

        var hrDepartment = new Department
        {
            Name = "Human Resources",
            Description = "HR department"
        };

        dbContext.Departments.AddRange(engineeringDepartment, marketingDepartment, hrDepartment);

        var marketingUser = new AppUser
        {
            Email = "marketing.demo@accelist.local",
            FullName = "Maya Marketing",
            Role = UserRole.Marketing,
            Department = marketingDepartment,
            IsActive = true
        };

        var pmUser = new AppUser
        {
            Email = "pm.demo@accelist.local",
            FullName = "Peter PM",
            Role = UserRole.Pm,
            Department = engineeringDepartment,
            IsActive = true
        };

        var hrUser = new AppUser
        {
            Email = "hr.demo@accelist.local",
            FullName = "Helen HR",
            Role = UserRole.Hr,
            Department = hrDepartment,
            IsActive = true
        };

        var employeeUser = new AppUser
        {
            Email = "employee.demo@accelist.local",
            FullName = "Ethan Employee",
            Role = UserRole.Employee,
            Department = engineeringDepartment,
            IsActive = true
        };

        dbContext.Users.AddRange(marketingUser, pmUser, hrUser, employeeUser);

        var employee = new Employee
        {
            User = employeeUser,
            Department = engineeringDepartment,
            JobTitle = "Backend Developer",
            Status = EmploymentStatus.Active,
            AvailabilityPercent = 80,
            WorkloadPercent = 20,
            WorkloadState = WorkloadStatus.Available,
            AssignedHours = 1.6m,
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6))
        };

        dbContext.Employees.Add(employee);

        var project = new Project
        {
            CreatedByUser = marketingUser,
            PmOwnerUser = pmUser,
            Name = "Q3 Campaign Launch",
            ClientName = "Accelist Internal",
            Description = "Demo project for API testing",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(2)),
            Status = ProjectStatus.Approved,
            ProgressPercent = 10,
            RiskLevel = ProjectRiskLevel.Medium,
            ResourceUtilizationPercent = 25,
            TotalRequiredResources = 2,
            SubmittedAt = DateTimeOffset.UtcNow.AddDays(-2),
            ApprovedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        dbContext.Projects.Add(project);

        var assignment = new Assignment
        {
            Project = project,
            Employee = employee,
            AssignedByUser = pmUser,
            RoleName = "Backend Developer",
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            AllocationPercent = 50,
            Priority = PriorityLevel.Medium,
            Status = AssignmentStatus.Pending,
            ProgressPercent = 0
        };

        dbContext.Assignments.Add(assignment);

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Development seed data inserted successfully.");
    }
}