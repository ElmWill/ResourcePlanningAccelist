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
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var hasExistingData = await dbContext.Users.AnyAsync(cancellationToken) ||
                              await dbContext.Projects.AnyAsync(cancellationToken) ||
                              await dbContext.Employees.AnyAsync(cancellationToken);

        if (hasExistingData)
        {
            logger.LogInformation("Existing data found. Running supplemental employee top-up only.");
            await SeedSupplementalEmployeesForExistingDataAsync();
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        async Task SeedSupplementalEmployeesForExistingDataAsync()
        {
            var engineeringDepartmentExisting = await dbContext.Departments.FirstOrDefaultAsync(d => d.Name == "Engineering", cancellationToken);
            var productDepartmentExisting = await dbContext.Departments.FirstOrDefaultAsync(d => d.Name == "Product", cancellationToken);
            var marketingDepartmentExisting = await dbContext.Departments.FirstOrDefaultAsync(d => d.Name == "Marketing", cancellationToken);
            var hrDepartmentExisting = await dbContext.Departments.FirstOrDefaultAsync(d => d.Name == "Human Resources", cancellationToken);

            if (engineeringDepartmentExisting is null ||
                productDepartmentExisting is null ||
                marketingDepartmentExisting is null ||
                hrDepartmentExisting is null)
            {
                logger.LogWarning("Skipping supplemental employee top-up because one or more departments were not found.");
                return;
            }

            var nodeSkillExisting = await dbContext.Skills.FirstOrDefaultAsync(s => s.Name == "Node.js", cancellationToken);
            var reactSkillExisting = await dbContext.Skills.FirstOrDefaultAsync(s => s.Name == "React", cancellationToken);
            var productManagementSkillExisting = await dbContext.Skills.FirstOrDefaultAsync(s => s.Name == "Product Management", cancellationToken);
            var analyticsSkillExisting = await dbContext.Skills.FirstOrDefaultAsync(s => s.Name == "Analytics", cancellationToken);
            var agileSkillExisting = await dbContext.Skills.FirstOrDefaultAsync(s => s.Name == "Agile", cancellationToken);

            if (nodeSkillExisting is null ||
                reactSkillExisting is null ||
                productManagementSkillExisting is null ||
                analyticsSkillExisting is null ||
                agileSkillExisting is null)
            {
                logger.LogWarning("Skipping supplemental employee top-up because one or more skills were not found.");
                return;
            }

            static WorkloadStatus ResolveExistingWorkloadState(int workloadPercent)
            {
                if (workloadPercent <= 40)
                {
                    return WorkloadStatus.Available;
                }

                if (workloadPercent <= 70)
                {
                    return WorkloadStatus.Moderate;
                }

                if (workloadPercent <= 100)
                {
                    return WorkloadStatus.Busy;
                }

                return WorkloadStatus.Overloaded;
            }

            async Task TopUpDepartmentAsync(
                Department department,
                string emailPrefix,
                string namePrefix,
                string jobTitle,
                Skill primarySkill,
                Skill secondarySkill,
                int targetCount)
            {
                var existingCount = await dbContext.Employees.CountAsync(e => e.DepartmentId == department.Id, cancellationToken);
                var required = Math.Max(0, targetCount - existingCount);
                var created = 0;
                var candidateSequence = existingCount + 1;

                while (created < required)
                {
                    var email = $"{emailPrefix}.staff{candidateSequence:00}@accelist.local";
                    candidateSequence++;

                    if (await dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken))
                    {
                        continue;
                    }

                    var workloadPercent = 28 + ((candidateSequence * 9) % 56);
                    var availabilityPercent = Math.Max(0, 100 - workloadPercent);
                    var assignedHours = Math.Round((decimal)workloadPercent / 100m * 8m, 1);

                    var user = new AppUser
                    {
                        Email = email,
                        FullName = $"{namePrefix} Staff {candidateSequence:00}",
                        Role = UserRole.Employee,
                        Department = department,
                        IsActive = true
                    };

                    var employee = new Employee
                    {
                        User = user,
                        Department = department,
                        JobTitle = jobTitle,
                        Status = EmploymentStatus.Active,
                        AvailabilityPercent = availabilityPercent,
                        WorkloadPercent = workloadPercent,
                        WorkloadState = ResolveExistingWorkloadState(workloadPercent),
                        AssignedHours = assignedHours,
                        HireDate = today.AddMonths(-(6 + candidateSequence))
                    };

                    dbContext.Users.Add(user);
                    dbContext.Employees.Add(employee);
                    dbContext.EmployeeSkills.AddRange(
                        new EmployeeSkill
                        {
                            Employee = employee,
                            Skill = primarySkill,
                            Proficiency = 3 + (candidateSequence % 3),
                            IsPrimary = true
                        },
                        new EmployeeSkill
                        {
                            Employee = employee,
                            Skill = secondarySkill,
                            Proficiency = 2 + (candidateSequence % 3),
                            IsPrimary = false
                        });

                    dbContext.EmployeeContracts.Add(
                        new EmployeeContract
                        {
                            Employee = employee,
                            StartDate = today.AddMonths(-(4 + candidateSequence)),
                            EndDate = today.AddMonths(8 + (candidateSequence % 5)),
                            Status = ContractStatus.Active,
                            Notes = $"Supplemental seeded employee for {department.Name} load testing"
                        });

                    created++;
                }
            }

            await TopUpDepartmentAsync(
                engineeringDepartmentExisting,
                "engineering",
                "Engineering",
                "Software Engineer",
                nodeSkillExisting,
                reactSkillExisting,
                targetCount: 10);

            await TopUpDepartmentAsync(
                productDepartmentExisting,
                "product",
                "Product",
                "Product Specialist",
                productManagementSkillExisting,
                analyticsSkillExisting,
                targetCount: 10);

            await TopUpDepartmentAsync(
                marketingDepartmentExisting,
                "marketing",
                "Marketing",
                "Marketing Specialist",
                analyticsSkillExisting,
                agileSkillExisting,
                targetCount: 10);

            await TopUpDepartmentAsync(
                hrDepartmentExisting,
                "hr",
                "HR",
                "HR Specialist",
                agileSkillExisting,
                productManagementSkillExisting,
                targetCount: 10);
        }

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

        var qaUser = new AppUser
        {
            Email = "qa.demo@accelist.local",
            FullName = "Quinn QA",
            Role = UserRole.Employee,
            Department = engineeringDepartment,
            IsActive = true
        };

        var devopsUser = new AppUser
        {
            Email = "devops.demo@accelist.local",
            FullName = "Devon Ops",
            Role = UserRole.Employee,
            Department = engineeringDepartment,
            IsActive = true
        };

        var analystUser = new AppUser
        {
            Email = "analyst.demo@accelist.local",
            FullName = "Avery Analyst",
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
            productUser,
            qaUser,
            devopsUser,
            analystUser);

        var nodeSkill = new Skill { Name = "Node.js", Category = SkillCategory.Technical };
        var postgreSqlSkill = new Skill { Name = "PostgreSQL", Category = SkillCategory.Technical };
        var reactSkill = new Skill { Name = "React", Category = SkillCategory.Technical };
        var figmaSkill = new Skill { Name = "Figma", Category = SkillCategory.Technical };
        var productManagementSkill = new Skill { Name = "Product Management", Category = SkillCategory.Business };
        var agileSkill = new Skill { Name = "Agile", Category = SkillCategory.Soft };
        var qaAutomationSkill = new Skill { Name = "QA Automation", Category = SkillCategory.Technical };
        var devOpsSkill = new Skill { Name = "DevOps", Category = SkillCategory.Technical };
        var analyticsSkill = new Skill { Name = "Analytics", Category = SkillCategory.Business };

        dbContext.Skills.AddRange(
            nodeSkill,
            postgreSqlSkill,
            reactSkill,
            figmaSkill,
            productManagementSkill,
            agileSkill,
            qaAutomationSkill,
            devOpsSkill,
            analyticsSkill);

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

        var qaEmployee = new Employee
        {
            User = qaUser,
            Department = engineeringDepartment,
            JobTitle = "QA Engineer",
            Status = EmploymentStatus.Active,
            AvailabilityPercent = 74,
            WorkloadPercent = 26,
            WorkloadState = WorkloadStatus.Available,
            AssignedHours = 2.0m,
            HireDate = today.AddMonths(-16)
        };

        var devopsEmployee = new Employee
        {
            User = devopsUser,
            Department = engineeringDepartment,
            JobTitle = "DevOps Engineer",
            Status = EmploymentStatus.Active,
            AvailabilityPercent = 69,
            WorkloadPercent = 31,
            WorkloadState = WorkloadStatus.Available,
            AssignedHours = 2.4m,
            HireDate = today.AddMonths(-22)
        };

        var analystEmployee = new Employee
        {
            User = analystUser,
            Department = productDepartment,
            JobTitle = "Business Analyst",
            Status = EmploymentStatus.Active,
            AvailabilityPercent = 82,
            WorkloadPercent = 18,
            WorkloadState = WorkloadStatus.Available,
            AssignedHours = 1.6m,
            HireDate = today.AddMonths(-11)
        };

        dbContext.Employees.AddRange(
            backendEmployee,
            frontendEmployee,
            designerEmployee,
            productEmployee,
            qaEmployee,
            devopsEmployee,
            analystEmployee);

        dbContext.EmployeeSkills.AddRange(
            new EmployeeSkill { Employee = backendEmployee, Skill = nodeSkill, Proficiency = 5, IsPrimary = true },
            new EmployeeSkill { Employee = backendEmployee, Skill = postgreSqlSkill, Proficiency = 4, IsPrimary = false },
            new EmployeeSkill { Employee = backendEmployee, Skill = agileSkill, Proficiency = 3, IsPrimary = false },
            new EmployeeSkill { Employee = frontendEmployee, Skill = reactSkill, Proficiency = 5, IsPrimary = true },
            new EmployeeSkill { Employee = frontendEmployee, Skill = figmaSkill, Proficiency = 3, IsPrimary = false },
            new EmployeeSkill { Employee = designerEmployee, Skill = figmaSkill, Proficiency = 5, IsPrimary = true },
            new EmployeeSkill { Employee = designerEmployee, Skill = reactSkill, Proficiency = 2, IsPrimary = false },
            new EmployeeSkill { Employee = productEmployee, Skill = productManagementSkill, Proficiency = 5, IsPrimary = true },
            new EmployeeSkill { Employee = productEmployee, Skill = agileSkill, Proficiency = 4, IsPrimary = false },
            new EmployeeSkill { Employee = qaEmployee, Skill = qaAutomationSkill, Proficiency = 5, IsPrimary = true },
            new EmployeeSkill { Employee = qaEmployee, Skill = agileSkill, Proficiency = 4, IsPrimary = false },
            new EmployeeSkill { Employee = devopsEmployee, Skill = devOpsSkill, Proficiency = 5, IsPrimary = true },
            new EmployeeSkill { Employee = devopsEmployee, Skill = nodeSkill, Proficiency = 3, IsPrimary = false },
            new EmployeeSkill { Employee = analystEmployee, Skill = analyticsSkill, Proficiency = 5, IsPrimary = true },
            new EmployeeSkill { Employee = analystEmployee, Skill = productManagementSkill, Proficiency = 3, IsPrimary = false });

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
            },
            new EmployeeContract
            {
                Employee = qaEmployee,
                StartDate = today.AddMonths(-14),
                EndDate = today.AddMonths(7),
                Status = ContractStatus.Active,
                Notes = "QA coverage for testing and quality gates"
            },
            new EmployeeContract
            {
                Employee = devopsEmployee,
                StartDate = today.AddMonths(-20),
                EndDate = today.AddMonths(10),
                Status = ContractStatus.Active,
                Notes = "Environment and deployment support"
            },
            new EmployeeContract
            {
                Employee = analystEmployee,
                StartDate = today.AddMonths(-10),
                EndDate = today.AddMonths(5),
                Status = ContractStatus.Active,
                Notes = "Requirement analysis and acceptance support"
            });

        static WorkloadStatus ResolveWorkloadState(int workloadPercent)
        {
            if (workloadPercent <= 40)
            {
                return WorkloadStatus.Available;
            }

            if (workloadPercent <= 70)
            {
                return WorkloadStatus.Moderate;
            }

            if (workloadPercent <= 100)
            {
                return WorkloadStatus.Busy;
            }

            return WorkloadStatus.Overloaded;
        }

        void SeedSupplementalDepartmentEmployees(
            Department department,
            string emailPrefix,
            string namePrefix,
            string primaryJobTitle,
            Skill primarySkill,
            Skill secondarySkill,
            int existingCount,
            int targetCount)
        {
            var required = Math.Max(0, targetCount - existingCount);

            for (var i = 0; i < required; i++)
            {
                var sequence = existingCount + i + 1;
                var workloadPercent = 28 + ((sequence * 9) % 56);
                var availabilityPercent = Math.Max(0, 100 - workloadPercent);
                var assignedHours = Math.Round((decimal)workloadPercent / 100m * 8m, 1);

                var user = new AppUser
                {
                    Email = $"{emailPrefix}.staff{sequence:00}@accelist.local",
                    FullName = $"{namePrefix} Staff {sequence:00}",
                    Role = UserRole.Employee,
                    Department = department,
                    IsActive = true
                };

                var employee = new Employee
                {
                    User = user,
                    Department = department,
                    JobTitle = primaryJobTitle,
                    Status = EmploymentStatus.Active,
                    AvailabilityPercent = availabilityPercent,
                    WorkloadPercent = workloadPercent,
                    WorkloadState = ResolveWorkloadState(workloadPercent),
                    AssignedHours = assignedHours,
                    HireDate = today.AddMonths(-(6 + sequence))
                };

                dbContext.Users.Add(user);
                dbContext.Employees.Add(employee);

                dbContext.EmployeeSkills.AddRange(
                    new EmployeeSkill
                    {
                        Employee = employee,
                        Skill = primarySkill,
                        Proficiency = 3 + (sequence % 3),
                        IsPrimary = true
                    },
                    new EmployeeSkill
                    {
                        Employee = employee,
                        Skill = secondarySkill,
                        Proficiency = 2 + (sequence % 3),
                        IsPrimary = false
                    });

                dbContext.EmployeeContracts.Add(
                    new EmployeeContract
                    {
                        Employee = employee,
                        StartDate = today.AddMonths(-(4 + sequence)),
                        EndDate = today.AddMonths(8 + (sequence % 5)),
                        Status = ContractStatus.Active,
                        Notes = $"Supplemental seeded employee for {department.Name} load testing"
                    });
            }
        }

        // Ensure each department has at least 10 employees for richer conflict and workload testing.
        SeedSupplementalDepartmentEmployees(
            engineeringDepartment,
            "engineering",
            "Engineering",
            "Software Engineer",
            nodeSkill,
            reactSkill,
            existingCount: 5,
            targetCount: 10);

        SeedSupplementalDepartmentEmployees(
            productDepartment,
            "product",
            "Product",
            "Product Specialist",
            productManagementSkill,
            analyticsSkill,
            existingCount: 2,
            targetCount: 10);

        SeedSupplementalDepartmentEmployees(
            marketingDepartment,
            "marketing",
            "Marketing",
            "Marketing Specialist",
            analyticsSkill,
            agileSkill,
            existingCount: 0,
            targetCount: 10);

        SeedSupplementalDepartmentEmployees(
            hrDepartment,
            "hr",
            "HR",
            "HR Specialist",
            agileSkill,
            productManagementSkill,
            existingCount: 0,
            targetCount: 10);

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

        var mobileProject = new Project
        {
            CreatedByUser = marketingUser,
            PmOwnerUser = pmUser,
            ApprovedByUser = gmUser,
            Name = "Mobile App Refresh",
            ClientName = "Accelist Internal",
            Description = "Modernize the internal mobile app experience and stabilize API integrations.",
            StartDate = today.AddDays(-12),
            EndDate = today.AddMonths(2),
            Status = ProjectStatus.InProgress,
            ProgressPercent = 42,
            RiskLevel = ProjectRiskLevel.Medium,
            ResourceUtilizationPercent = 70,
            TotalRequiredResources = 3,
            SubmittedAt = now.AddDays(-15),
            ApprovedAt = now.AddDays(-13)
        };

        var portalProject = new Project
        {
            CreatedByUser = marketingUser,
            PmOwnerUser = pmUser,
            ApprovedByUser = gmUser,
            Name = "Customer Portal Improvements",
            ClientName = "Enterprise Customer Success",
            Description = "Deliver a new customer self-service flow with analytics and notification upgrades.",
            StartDate = today.AddDays(7),
            EndDate = today.AddMonths(4),
            Status = ProjectStatus.Assigned,
            ProgressPercent = 18,
            RiskLevel = ProjectRiskLevel.Low,
            ResourceUtilizationPercent = 48,
            TotalRequiredResources = 4,
            SubmittedAt = now.AddDays(-6),
            ApprovedAt = now.AddDays(-5)
        };

        dbContext.Projects.AddRange(historicalProject, currentProject, mobileProject, portalProject);

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

        var mobileBackendRequirement = new ProjectResourceRequirement
        {
            Project = mobileProject,
            RoleName = "Backend Developer",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Mid,
            SortOrder = 1,
            Notes = "API integration and service stabilization"
        };

        var mobileFrontendRequirement = new ProjectResourceRequirement
        {
            Project = mobileProject,
            RoleName = "Frontend Developer",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Mid,
            SortOrder = 2,
            Notes = "Mobile UI and performance enhancements"
        };

        var mobileProductRequirement = new ProjectResourceRequirement
        {
            Project = mobileProject,
            RoleName = "Product Manager",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Mid,
            SortOrder = 3,
            Notes = "Scope alignment and release planning"
        };

        var portalBackendRequirement = new ProjectResourceRequirement
        {
            Project = portalProject,
            RoleName = "Backend Developer",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Senior,
            SortOrder = 1,
            Notes = "Notification and analytics APIs"
        };

        var portalFrontendRequirement = new ProjectResourceRequirement
        {
            Project = portalProject,
            RoleName = "Frontend Developer",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Mid,
            SortOrder = 2,
            Notes = "Customer portal UX and dashboard updates"
        };

        var portalDesignerRequirement = new ProjectResourceRequirement
        {
            Project = portalProject,
            RoleName = "UI/UX Designer",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Mid,
            SortOrder = 3,
            Notes = "Design system updates and usability improvements"
        };

        var portalProductRequirement = new ProjectResourceRequirement
        {
            Project = portalProject,
            RoleName = "Product Manager",
            Quantity = 1,
            ExperienceLevel = ExperienceLevel.Mid,
            SortOrder = 4,
            Notes = "Stakeholder cadence and acceptance planning"
        };

        dbContext.ProjectResourceRequirements.AddRange(
            historicalBackendRequirement,
            historicalFrontendRequirement,
            historicalProductRequirement,
            currentBackendRequirement,
            currentFrontendRequirement,
            currentDesignerRequirement,
            currentProductRequirement,
            mobileBackendRequirement,
            mobileFrontendRequirement,
            mobileProductRequirement,
            portalBackendRequirement,
            portalFrontendRequirement,
            portalDesignerRequirement,
            portalProductRequirement);

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
            new ProjectRequirementSkill { Requirement = currentProductRequirement, Skill = agileSkill },
            new ProjectRequirementSkill { Requirement = currentProductRequirement, Skill = analyticsSkill },
            new ProjectRequirementSkill { Requirement = mobileBackendRequirement, Skill = nodeSkill },
            new ProjectRequirementSkill { Requirement = mobileBackendRequirement, Skill = postgreSqlSkill },
            new ProjectRequirementSkill { Requirement = mobileFrontendRequirement, Skill = reactSkill },
            new ProjectRequirementSkill { Requirement = mobileProductRequirement, Skill = productManagementSkill },
            new ProjectRequirementSkill { Requirement = mobileProductRequirement, Skill = agileSkill },
            new ProjectRequirementSkill { Requirement = portalBackendRequirement, Skill = nodeSkill },
            new ProjectRequirementSkill { Requirement = portalBackendRequirement, Skill = postgreSqlSkill },
            new ProjectRequirementSkill { Requirement = portalFrontendRequirement, Skill = reactSkill },
            new ProjectRequirementSkill { Requirement = portalDesignerRequirement, Skill = figmaSkill },
            new ProjectRequirementSkill { Requirement = portalProductRequirement, Skill = productManagementSkill },
            new ProjectRequirementSkill { Requirement = portalProductRequirement, Skill = agileSkill },
            new ProjectRequirementSkill { Requirement = portalProductRequirement, Skill = analyticsSkill });

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
            },
            new ProjectMilestone
            {
                Project = mobileProject,
                Title = "API Audit",
                Description = "Audit existing mobile API compatibility",
                DueDate = today.AddDays(-2),
                IsCompleted = true,
                CompletedAt = now.AddDays(-2),
                SortOrder = 1
            },
            new ProjectMilestone
            {
                Project = mobileProject,
                Title = "Core Flow Refresh",
                Description = "Refresh onboarding and profile flows",
                DueDate = today.AddDays(14),
                IsCompleted = false,
                SortOrder = 2
            },
            new ProjectMilestone
            {
                Project = mobileProject,
                Title = "Release Candidate",
                Description = "Finalize release candidate for QA",
                DueDate = today.AddDays(38),
                IsCompleted = false,
                SortOrder = 3
            },
            new ProjectMilestone
            {
                Project = portalProject,
                Title = "Kickoff Approved",
                Description = "Kickoff and plan approved by stakeholders",
                DueDate = today.AddDays(10),
                IsCompleted = false,
                SortOrder = 1
            },
            new ProjectMilestone
            {
                Project = portalProject,
                Title = "Prototype Review",
                Description = "Review first clickable prototype",
                DueDate = today.AddDays(30),
                IsCompleted = false,
                SortOrder = 2
            },
            new ProjectMilestone
            {
                Project = portalProject,
                Title = "Analytics Release",
                Description = "Release first analytics-enabled portal flow",
                DueDate = today.AddDays(62),
                IsCompleted = false,
                SortOrder = 3
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
            },
            new ProjectTimelineTask
            {
                Project = mobileProject,
                Name = "API Stabilization",
                StartOffsetDays = 0,
                DurationDays = 18,
                ColorTag = "blue",
                Status = TimelineTaskStatus.InProgress,
                SortOrder = 1
            },
            new ProjectTimelineTask
            {
                Project = mobileProject,
                Name = "UI Refresh",
                StartOffsetDays = 12,
                DurationDays = 24,
                ColorTag = "green",
                Status = TimelineTaskStatus.Pending,
                SortOrder = 2
            },
            new ProjectTimelineTask
            {
                Project = mobileProject,
                Name = "QA and Rollout",
                StartOffsetDays = 34,
                DurationDays = 18,
                ColorTag = "yellow",
                Status = TimelineTaskStatus.Pending,
                SortOrder = 3
            },
            new ProjectTimelineTask
            {
                Project = portalProject,
                Name = "Discovery",
                StartOffsetDays = 0,
                DurationDays = 14,
                ColorTag = "blue",
                Status = TimelineTaskStatus.Pending,
                SortOrder = 1
            },
            new ProjectTimelineTask
            {
                Project = portalProject,
                Name = "Feature Build",
                StartOffsetDays = 14,
                DurationDays = 42,
                ColorTag = "green",
                Status = TimelineTaskStatus.Pending,
                SortOrder = 2
            },
            new ProjectTimelineTask
            {
                Project = portalProject,
                Name = "Launch Prep",
                StartOffsetDays = 56,
                DurationDays = 20,
                ColorTag = "purple",
                Status = TimelineTaskStatus.Pending,
                SortOrder = 3
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
            },
            new Assignment
            {
                Project = currentProject,
                Employee = qaEmployee,
                AssignedByUser = pmUser,
                RoleName = "QA Engineer",
                StartDate = currentProject.StartDate,
                EndDate = currentProject.EndDate,
                AllocationPercent = 35,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.Accepted,
                ProgressPercent = 32,
                AcceptedAt = now.AddDays(-2)
            },
            new Assignment
            {
                Project = currentProject,
                Employee = devopsEmployee,
                AssignedByUser = pmUser,
                RoleName = "DevOps Engineer",
                StartDate = currentProject.StartDate,
                EndDate = currentProject.EndDate,
                AllocationPercent = 30,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.InProgress,
                ProgressPercent = 40,
                AcceptedAt = now.AddDays(-2)
            },
            new Assignment
            {
                Project = mobileProject,
                Employee = backendEmployee,
                AssignedByUser = pmUser,
                RoleName = "Backend Developer",
                StartDate = mobileProject.StartDate,
                EndDate = mobileProject.EndDate,
                AllocationPercent = 55,
                Priority = PriorityLevel.High,
                Status = AssignmentStatus.InProgress,
                ProgressPercent = 45,
                AcceptedAt = now.AddDays(-10)
            },
            new Assignment
            {
                Project = mobileProject,
                Employee = frontendEmployee,
                AssignedByUser = pmUser,
                RoleName = "Frontend Developer",
                StartDate = mobileProject.StartDate,
                EndDate = mobileProject.EndDate,
                AllocationPercent = 40,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.Accepted,
                ProgressPercent = 38,
                AcceptedAt = now.AddDays(-9)
            },
            new Assignment
            {
                Project = mobileProject,
                Employee = productEmployee,
                AssignedByUser = pmUser,
                RoleName = "Product Manager",
                StartDate = mobileProject.StartDate,
                EndDate = mobileProject.EndDate,
                AllocationPercent = 35,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.InProgress,
                ProgressPercent = 40,
                AcceptedAt = now.AddDays(-8)
            },
            new Assignment
            {
                Project = mobileProject,
                Employee = qaEmployee,
                AssignedByUser = pmUser,
                RoleName = "QA Engineer",
                StartDate = mobileProject.StartDate,
                EndDate = mobileProject.EndDate,
                AllocationPercent = 28,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.Pending,
                ProgressPercent = 15
            },
            new Assignment
            {
                Project = portalProject,
                Employee = designerEmployee,
                AssignedByUser = pmUser,
                RoleName = "UI/UX Designer",
                StartDate = portalProject.StartDate,
                EndDate = portalProject.EndDate,
                AllocationPercent = 35,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.Pending,
                ProgressPercent = 12
            },
            new Assignment
            {
                Project = portalProject,
                Employee = productEmployee,
                AssignedByUser = pmUser,
                RoleName = "Product Manager",
                StartDate = portalProject.StartDate,
                EndDate = portalProject.EndDate,
                AllocationPercent = 30,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.Approved,
                ProgressPercent = 16,
                AcceptedAt = now.AddDays(-3)
            },
            new Assignment
            {
                Project = portalProject,
                Employee = analystEmployee,
                AssignedByUser = pmUser,
                RoleName = "Business Analyst",
                StartDate = portalProject.StartDate,
                EndDate = portalProject.EndDate,
                AllocationPercent = 32,
                Priority = PriorityLevel.Medium,
                Status = AssignmentStatus.Accepted,
                ProgressPercent = 20,
                AcceptedAt = now.AddDays(-3)
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
