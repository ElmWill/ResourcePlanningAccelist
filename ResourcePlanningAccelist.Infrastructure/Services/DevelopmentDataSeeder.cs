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

        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var engineeringDepartment = new Department
        {
            Name = "Engineering",
            Description = "Engineering department"
        };

        var productDepartment = new Department
        {
            Name = "Product",
            Description = "Product management and delivery"
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

        dbContext.Departments.AddRange(engineeringDepartment, productDepartment, marketingDepartment, hrDepartment);

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
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "pm.demo@accelist.local",
            FullName = "Peter PM",
            Role = UserRole.Pm,
            Department = engineeringDepartment,
            IsActive = true
        };

        var gmUser = new AppUser
        {
            Email = "gm.demo@accelist.local",
            FullName = "Grace GM",
            Role = UserRole.Gm,
            Department = productDepartment,
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

        var backendUser = new AppUser
        {
            Email = "backend.demo@accelist.local",
            FullName = "Ben Backend",
            Role = UserRole.Employee,
            Department = engineeringDepartment,
            IsActive = true
        };

        var frontendUser = new AppUser
        {
            Email = "frontend.demo@accelist.local",
            FullName = "Fiona Frontend",
            Role = UserRole.Employee,
            Department = engineeringDepartment,
            IsActive = true
        };

        var designerUser = new AppUser
        {
            Email = "designer.demo@accelist.local",
            FullName = "Diana Designer",
            Role = UserRole.Employee,
            Department = engineeringDepartment,
            IsActive = true
        };

        var productUser = new AppUser
        {
            Email = "product.demo@accelist.local",
            FullName = "Paula Product",
            Role = UserRole.Employee,
            Department = productDepartment,
            IsActive = true
        };

        dbContext.Users.AddRange(
            marketingUser,
            pmUser,
            gmUser,
            hrUser,
            backendUser,
            frontendUser,
            designerUser,
            productUser);

        var nodeSkill = new Skill { Name = "Node.js", Category = SkillCategory.Technical };
        var postgreSqlSkill = new Skill { Name = "PostgreSQL", Category = SkillCategory.Technical };
        var reactSkill = new Skill { Name = "React", Category = SkillCategory.Technical };
        var figmaSkill = new Skill { Name = "Figma", Category = SkillCategory.Technical };
        var productManagementSkill = new Skill { Name = "Product Management", Category = SkillCategory.Business };
        var agileSkill = new Skill { Name = "Agile", Category = SkillCategory.Soft };

        dbContext.Skills.AddRange(
            nodeSkill,
            postgreSqlSkill,
            reactSkill,
            figmaSkill,
            productManagementSkill,
            agileSkill);

        var backendEmployee = new Employee
        {
            User = backendUser,
            Department = engineeringDepartment,
            JobTitle = "Backend Developer",
            Status = EmploymentStatus.Active,
            AvailabilityPercent = 65,
            WorkloadPercent = 35,
            WorkloadState = WorkloadStatus.Available,
            AssignedHours = 2.8m,
            HireDate = today.AddMonths(-18)
        };

        var frontendEmployee = new Employee
        {
            User = frontendUser,
            Department = engineeringDepartment,
            JobTitle = "Frontend Developer",
            Status = EmploymentStatus.Active,
            AvailabilityPercent = 78,
            WorkloadPercent = 22,
            WorkloadState = WorkloadStatus.Available,
            AssignedHours = 1.8m,
            HireDate = today.AddMonths(-14)
        };

        var designerEmployee = new Employee
        {
            User = designerUser,
            Department = engineeringDepartment,
            JobTitle = "UI/UX Designer",
            Status = EmploymentStatus.Active,
            AvailabilityPercent = 72,
            WorkloadPercent = 28,
            WorkloadState = WorkloadStatus.Available,
            AssignedHours = 2.2m,
            HireDate = today.AddMonths(-12)
        };

        var productEmployee = new Employee
        {
            User = productUser,
            Department = productDepartment,
            JobTitle = "Product Manager",
            Status = EmploymentStatus.Active,
            AvailabilityPercent = 86,
            WorkloadPercent = 14,
            WorkloadState = WorkloadStatus.Available,
            AssignedHours = 1.1m,
            HireDate = today.AddMonths(-20)
        };

        dbContext.Employees.AddRange(
            backendEmployee,
            frontendEmployee,
            designerEmployee,
            productEmployee);

        dbContext.EmployeeSkills.AddRange(
            new EmployeeSkill { Employee = backendEmployee, Skill = nodeSkill, Proficiency = 5, IsPrimary = true },
            new EmployeeSkill { Employee = backendEmployee, Skill = postgreSqlSkill, Proficiency = 4, IsPrimary = false },
            new EmployeeSkill { Employee = backendEmployee, Skill = agileSkill, Proficiency = 3, IsPrimary = false },
            new EmployeeSkill { Employee = frontendEmployee, Skill = reactSkill, Proficiency = 5, IsPrimary = true },
            new EmployeeSkill { Employee = frontendEmployee, Skill = figmaSkill, Proficiency = 3, IsPrimary = false },
            new EmployeeSkill { Employee = designerEmployee, Skill = figmaSkill, Proficiency = 5, IsPrimary = true },
            new EmployeeSkill { Employee = designerEmployee, Skill = reactSkill, Proficiency = 2, IsPrimary = false },
            new EmployeeSkill { Employee = productEmployee, Skill = productManagementSkill, Proficiency = 5, IsPrimary = true },
            new EmployeeSkill { Employee = productEmployee, Skill = agileSkill, Proficiency = 4, IsPrimary = false });

        dbContext.EmployeeContracts.AddRange(
            new EmployeeContract
            {
                Employee = backendEmployee,
                StartDate = today.AddMonths(-12),
                EndDate = today.AddMonths(6),
                Status = ContractStatus.Active,
                Notes = "Backend lead on active delivery work"
            },
            new EmployeeContract
            {
                Employee = frontendEmployee,
                StartDate = today.AddMonths(-10),
                EndDate = today.AddMonths(4),
                Status = ContractStatus.Active,
                Notes = "Frontend specialist on active delivery work"
            },
            new EmployeeContract
            {
                Employee = designerEmployee,
                StartDate = today.AddMonths(-9),
                EndDate = today.AddMonths(3),
                Status = ContractStatus.Extended,
                Notes = "Designer contract extended for current project"
            },
            new EmployeeContract
            {
                Employee = productEmployee,
                StartDate = today.AddMonths(-18),
                EndDate = today.AddMonths(8),
                Status = ContractStatus.Active,
                Notes = "Product lead covering roadmap and delivery"
            });

        var historicalProject = new Project
        {
            CreatedByUser = marketingUser,
            PmOwnerUser = pmUser,
            ApprovedByUser = gmUser,
            Name = "Legacy API Stabilization",
            ClientName = "Internal Platform",
            Description = "Completed project used to build historical staffing data",
            StartDate = today.AddMonths(-7),
            EndDate = today.AddMonths(-4),
            Status = ProjectStatus.Completed,
            ProgressPercent = 100,
            RiskLevel = ProjectRiskLevel.Low,
            ResourceUtilizationPercent = 92,
            TotalRequiredResources = 3,
            SubmittedAt = now.AddMonths(-7),
            ApprovedAt = now.AddMonths(-7).AddDays(2)
        };

        var currentProject = new Project
        {
            CreatedByUser = marketingUser,
            PmOwnerUser = pmUser,
            ApprovedByUser = gmUser,
            Name = "Website Redesign",
            ClientName = "Accelist Internal",
            Description = "Complete overhaul of company website with modern design",
            StartDate = today.AddDays(10),
            EndDate = today.AddMonths(3),
            Status = ProjectStatus.InProgress,
            ProgressPercent = 65,
            RiskLevel = ProjectRiskLevel.Medium,
            ResourceUtilizationPercent = 78,
            TotalRequiredResources = 4,
            SubmittedAt = now.AddDays(-3),
            ApprovedAt = now.AddDays(-2)
        };

        dbContext.Projects.AddRange(historicalProject, currentProject);

        var historicalBackendRequirement = new ProjectResourceRequirement
        {
            Project = historicalProject,
            RoleName = "Backend Developer",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Senior,
            SortOrder = 1,
            Notes = "Historical backend delivery role"
        };

        var historicalFrontendRequirement = new ProjectResourceRequirement
        {
            Project = historicalProject,
            RoleName = "Frontend Developer",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Mid,
            SortOrder = 2,
            Notes = "Historical frontend delivery role"
        };

        var historicalProductRequirement = new ProjectResourceRequirement
        {
            Project = historicalProject,
            RoleName = "Product Manager",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Mid,
            SortOrder = 3,
            Notes = "Historical product coordination role"
        };

        var currentBackendRequirement = new ProjectResourceRequirement
        {
            Project = currentProject,
            RoleName = "Backend Developer",
            Quantity = 2,
            ExperienceLevel = ExperienceLevel.Senior,
            SortOrder = 1,
            Notes = "API and database delivery"
        };

        var currentFrontendRequirement = new ProjectResourceRequirement
        {
            Project = currentProject,
            RoleName = "Frontend Developer",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Mid,
            SortOrder = 2,
            Notes = "Responsive UI implementation"
        };

        var currentDesignerRequirement = new ProjectResourceRequirement
        {
            Project = currentProject,
            RoleName = "UI/UX Designer",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Mid,
            SortOrder = 3,
            Notes = "Design system and screens"
        };

        var currentProductRequirement = new ProjectResourceRequirement
        {
            Project = currentProject,
            RoleName = "Product Manager",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Mid,
            SortOrder = 4,
            Notes = "Stakeholder coordination and planning"
        };

        dbContext.ProjectResourceRequirements.AddRange(
            historicalBackendRequirement,
            historicalFrontendRequirement,
            historicalProductRequirement,
            currentBackendRequirement,
            currentFrontendRequirement,
            currentDesignerRequirement,
            currentProductRequirement);

        dbContext.ProjectRequirementSkills.AddRange(
            new ProjectRequirementSkill { Requirement = historicalBackendRequirement, Skill = nodeSkill },
            new ProjectRequirementSkill { Requirement = historicalBackendRequirement, Skill = postgreSqlSkill },
            new ProjectRequirementSkill { Requirement = historicalFrontendRequirement, Skill = reactSkill },
            new ProjectRequirementSkill { Requirement = historicalProductRequirement, Skill = productManagementSkill },
            new ProjectRequirementSkill { Requirement = historicalProductRequirement, Skill = agileSkill },
            new ProjectRequirementSkill { Requirement = currentBackendRequirement, Skill = nodeSkill },
            new ProjectRequirementSkill { Requirement = currentBackendRequirement, Skill = postgreSqlSkill },
            new ProjectRequirementSkill { Requirement = currentFrontendRequirement, Skill = reactSkill },
            new ProjectRequirementSkill { Requirement = currentDesignerRequirement, Skill = figmaSkill },
            new ProjectRequirementSkill { Requirement = currentProductRequirement, Skill = productManagementSkill },
            new ProjectRequirementSkill { Requirement = currentProductRequirement, Skill = agileSkill });

        dbContext.ProjectMilestones.AddRange(
            new ProjectMilestone
            {
                Project = historicalProject,
                Title = "Requirements Signed Off",
                Description = "Scope approved for historical delivery",
                DueDate = today.AddMonths(-6),
                IsCompleted = true,
                CompletedAt = now.AddMonths(-6).AddDays(1),
                SortOrder = 1
            },
            new ProjectMilestone
            {
                Project = historicalProject,
                Title = "Backend Delivered",
                Description = "Core APIs completed",
                DueDate = today.AddMonths(-5),
                IsCompleted = true,
                CompletedAt = now.AddMonths(-5).AddDays(2),
                SortOrder = 2
            },
            new ProjectMilestone
            {
                Project = historicalProject,
                Title = "Project Closed",
                Description = "Historical project closed successfully",
                DueDate = today.AddMonths(-4),
                IsCompleted = true,
                CompletedAt = now.AddMonths(-4).AddDays(1),
                SortOrder = 3
            },
            new ProjectMilestone
            {
                Project = currentProject,
                Title = "Requirements Gathering",
                Description = "Scope and acceptance criteria finalized",
                DueDate = today.AddDays(20),
                IsCompleted = true,
                CompletedAt = now.AddDays(-1),
                SortOrder = 1
            },
            new ProjectMilestone
            {
                Project = currentProject,
                Title = "Design System Creation",
                Description = "Core design system and wireframes",
                DueDate = today.AddDays(35),
                IsCompleted = true,
                CompletedAt = now,
                SortOrder = 2
            },
            new ProjectMilestone
            {
                Project = currentProject,
                Title = "Frontend Development",
                Description = "Implementation of key UI flows",
                DueDate = today.AddDays(55),
                IsCompleted = false,
                SortOrder = 3
            },
            new ProjectMilestone
            {
                Project = currentProject,
                Title = "Backend Integration",
                Description = "API integration and business rules",
                DueDate = today.AddDays(65),
                IsCompleted = false,
                SortOrder = 4
            },
            new ProjectMilestone
            {
                Project = currentProject,
                Title = "Testing & QA",
                Description = "Regression and acceptance testing",
                DueDate = today.AddDays(80),
                IsCompleted = false,
                SortOrder = 5
            },
            new ProjectMilestone
            {
                Project = currentProject,
                Title = "Deployment",
                Description = "Production release",
                DueDate = today.AddMonths(3),
                IsCompleted = false,
                SortOrder = 6
            });

        dbContext.ProjectTimelineTasks.AddRange(
            new ProjectTimelineTask
            {
                Project = historicalProject,
                Name = "API Refactor",
                StartOffsetDays = 0,
                DurationDays = 20,
                ColorTag = "blue",
                Status = TimelineTaskStatus.Completed,
                SortOrder = 1
            },
            new ProjectTimelineTask
            {
                Project = historicalProject,
                Name = "Integration QA",
                StartOffsetDays = 20,
                DurationDays = 14,
                ColorTag = "green",
                Status = TimelineTaskStatus.Completed,
                SortOrder = 2
            },
            new ProjectTimelineTask
            {
                Project = currentProject,
                Name = "Design Phase",
                StartOffsetDays = 10,
                DurationDays = 20,
                ColorTag = "blue",
                Status = TimelineTaskStatus.InProgress,
                SortOrder = 1
            },
            new ProjectTimelineTask
            {
                Project = currentProject,
                Name = "Development",
                StartOffsetDays = 25,
                DurationDays = 40,
                ColorTag = "green",
                Status = TimelineTaskStatus.Pending,
                SortOrder = 2
            },
            new ProjectTimelineTask
            {
                Project = currentProject,
                Name = "Testing",
                StartOffsetDays = 60,
                DurationDays = 15,
                ColorTag = "yellow",
                Status = TimelineTaskStatus.Pending,
                SortOrder = 3
            },
            new ProjectTimelineTask
            {
                Project = currentProject,
                Name = "Deployment",
                StartOffsetDays = 75,
                DurationDays = 10,
                ColorTag = "purple",
                Status = TimelineTaskStatus.Pending,
                SortOrder = 4
            });

        dbContext.Assignments.AddRange(
            new Assignment
            {
                Project = historicalProject,
                Employee = backendEmployee,
                AssignedByUser = pmUser,
                RoleName = "Backend Developer",
                StartDate = historicalProject.StartDate,
                EndDate = historicalProject.EndDate,
                AllocationPercent = 100,
                Priority = PriorityLevel.High,
                Status = AssignmentStatus.Completed,
                ProgressPercent = 100,
                AcceptedAt = now.AddMonths(-7).AddDays(1)
            },
            new Assignment
            {
                Project = historicalProject,
                Employee = frontendEmployee,
                AssignedByUser = pmUser,
                RoleName = "Frontend Developer",
                StartDate = historicalProject.StartDate,
                EndDate = historicalProject.EndDate,
                AllocationPercent = 90,
                Priority = PriorityLevel.High,
                Status = AssignmentStatus.Completed,
                ProgressPercent = 100,
                AcceptedAt = now.AddMonths(-7).AddDays(1)
            },
            new Assignment
            {
                Project = historicalProject,
                Employee = productEmployee,
                AssignedByUser = pmUser,
                RoleName = "Product Manager",
                StartDate = historicalProject.StartDate,
                EndDate = historicalProject.EndDate,
                AllocationPercent = 70,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.Completed,
                ProgressPercent = 100,
                AcceptedAt = now.AddMonths(-7).AddDays(1)
            },
            new Assignment
            {
                Project = currentProject,
                Employee = backendEmployee,
                AssignedByUser = pmUser,
                RoleName = "Backend Developer",
                StartDate = currentProject.StartDate,
                EndDate = currentProject.EndDate,
                AllocationPercent = 80,
                Priority = PriorityLevel.High,
                Status = AssignmentStatus.InProgress,
                ProgressPercent = 60,
                AcceptedAt = now.AddDays(-1)
            },
            new Assignment
            {
                Project = currentProject,
                Employee = frontendEmployee,
                AssignedByUser = pmUser,
                RoleName = "Frontend Developer",
                StartDate = currentProject.StartDate,
                EndDate = currentProject.EndDate,
                AllocationPercent = 60,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.Accepted,
                ProgressPercent = 35,
                AcceptedAt = now.AddDays(-1)
            },
            new Assignment
            {
                Project = currentProject,
                Employee = designerEmployee,
                AssignedByUser = pmUser,
                RoleName = "UI/UX Designer",
                StartDate = currentProject.StartDate,
                EndDate = currentProject.EndDate,
                AllocationPercent = 50,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.Pending,
                ProgressPercent = 10
            },
            new Assignment
            {
                Project = currentProject,
                Employee = productEmployee,
                AssignedByUser = pmUser,
                RoleName = "Product Manager",
                StartDate = currentProject.StartDate,
                EndDate = currentProject.EndDate,
                AllocationPercent = 45,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.Approved,
                ProgressPercent = 25,
                AcceptedAt = now.AddDays(-1)
            });

        dbContext.GmDecisions.AddRange(
            new GmDecision
            {
                Project = currentProject,
                DecisionType = DecisionType.ExtendContract,
                Title = "Extend frontend contract",
                Details = "Frontend delivery is on track and the role remains needed for the next project phase.",
                Deadline = today.AddDays(21),
                Status = DecisionStatus.Pending,
                SubmittedByUser = gmUser,
                SubmittedAt = now.AddDays(-2),
                AffectedEmployees =
                {
                    new GmDecisionEmployee
                    {
                        Employee = frontendEmployee
                    }
                }
            },
            new GmDecision
            {
                Project = currentProject,
                DecisionType = DecisionType.TerminateContract,
                Title = "Review designer contract renewal",
                Details = "Design demand is expected to drop after the current delivery milestone.",
                Deadline = today.AddDays(28),
                Status = DecisionStatus.Pending,
                SubmittedByUser = gmUser,
                SubmittedAt = now.AddDays(-1),
                AffectedEmployees =
                {
                    new GmDecisionEmployee
                    {
                        Employee = designerEmployee
                    }
                }
            });

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Development seed data inserted successfully.");
    }
}
