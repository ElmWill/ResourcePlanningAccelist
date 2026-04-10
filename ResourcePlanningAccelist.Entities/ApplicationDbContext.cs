using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Entities.Abstractions;

namespace ResourcePlanningAccelist.Entities;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectReview> ProjectReviews => Set<ProjectReview>();

    public DbSet<ProjectSkill> ProjectSkills => Set<ProjectSkill>();

    public DbSet<ProjectResourceRequirement> ProjectResourceRequirements => Set<ProjectResourceRequirement>();

    public DbSet<ProjectRequirementSkill> ProjectRequirementSkills => Set<ProjectRequirementSkill>();

    public DbSet<ProjectAttachment> ProjectAttachments => Set<ProjectAttachment>();

    public DbSet<ProjectMilestone> ProjectMilestones => Set<ProjectMilestone>();

    public DbSet<ProjectTimelineTask> ProjectTimelineTasks => Set<ProjectTimelineTask>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<AssignmentReview> AssignmentReviews => Set<AssignmentReview>();

    public DbSet<EmployeeContract> EmployeeContracts => Set<EmployeeContract>();

    public DbSet<GmDecision> GmDecisions => Set<GmDecision>();

    public DbSet<GmDecisionEmployee> GmDecisionEmployees => Set<GmDecisionEmployee>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<HiringRequest> HiringRequests => Set<HiringRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureEnumConversions(modelBuilder);
        ConfigureRelationships(modelBuilder);
        ConfigureIndexes(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }

    private static void ConfigureEnumConversions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().Property(entity => entity.Role).HasConversion<string>();
        modelBuilder.Entity<Skill>().Property(entity => entity.Category).HasConversion<string>();
        modelBuilder.Entity<Employee>().Property(entity => entity.Status).HasConversion<string>();
        modelBuilder.Entity<Employee>().Property(entity => entity.WorkloadState).HasConversion<string>();
        modelBuilder.Entity<Project>().Property(entity => entity.Status).HasConversion<string>();
        modelBuilder.Entity<Project>().Property(entity => entity.RiskLevel).HasConversion<string>();
        modelBuilder.Entity<ProjectResourceRequirement>().Property(entity => entity.ExperienceLevel).HasConversion<string>();
        modelBuilder.Entity<Assignment>().Property(entity => entity.Priority).HasConversion<string>();
        modelBuilder.Entity<Assignment>().Property(entity => entity.Status).HasConversion<string>();
        modelBuilder.Entity<AssignmentReview>().Property(entity => entity.Status).HasConversion<string>();
        modelBuilder.Entity<EmployeeContract>().Property(entity => entity.Status).HasConversion<string>();
        modelBuilder.Entity<GmDecision>().Property(entity => entity.DecisionType).HasConversion<string>();
        modelBuilder.Entity<GmDecision>().Property(entity => entity.Status).HasConversion<string>();
        modelBuilder.Entity<Notification>().Property(entity => entity.Type).HasConversion<string>();
        modelBuilder.Entity<HiringRequest>().Property(entity => entity.Status).HasConversion<string>();
        modelBuilder.Entity<ProjectReview>().Property(entity => entity.Decision).HasConversion<string>();
        modelBuilder.Entity<ProjectTimelineTask>().Property(entity => entity.Status).HasConversion<string>();
    }

    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasMany(item => item.Users)
                .WithOne(item => item.Department)
                .HasForeignKey(item => item.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(item => item.Employees)
                .WithOne(item => item.Department)
                .HasForeignKey(item => item.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasMany(item => item.CreatedProjects)
                .WithOne(item => item.CreatedByUser)
                .HasForeignKey(item => item.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(item => item.ApprovedProjects)
                .WithOne(item => item.ApprovedByUser)
                .HasForeignKey(item => item.ApprovedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(item => item.ManagedProjects)
                .WithOne(item => item.PmOwnerUser)
                .HasForeignKey(item => item.PmOwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(item => item.ProjectReviews)
                .WithOne(item => item.ReviewerUser)
                .HasForeignKey(item => item.ReviewerUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(item => item.UploadedProjectAttachments)
                .WithOne(item => item.UploadedByUser)
                .HasForeignKey(item => item.UploadedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(item => item.CreatedAssignments)
                .WithOne(item => item.AssignedByUser)
                .HasForeignKey(item => item.AssignedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(item => item.AssignmentReviews)
                .WithOne(item => item.ReviewedByUser)
                .HasForeignKey(item => item.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(item => item.SubmittedDecisions)
                .WithOne(item => item.SubmittedByUser)
                .HasForeignKey(item => item.SubmittedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(item => item.ExecutedDecisions)
                .WithOne(item => item.ExecutedByUser)
                .HasForeignKey(item => item.ExecutedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(item => item.Notifications)
                .WithOne(item => item.User)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasOne(item => item.User)
                .WithOne(item => item.EmployeeProfile)
                .HasForeignKey<Employee>(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.EmployeeSkills)
                .WithOne(item => item.Employee)
                .HasForeignKey(item => item.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.Assignments)
                .WithOne(item => item.Employee)
                .HasForeignKey(item => item.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.Contracts)
                .WithOne(item => item.Employee)
                .HasForeignKey(item => item.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasMany(item => item.EmployeeSkills)
                .WithOne(item => item.Skill)
                .HasForeignKey(item => item.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.ProjectSkills)
                .WithOne(item => item.Skill)
                .HasForeignKey(item => item.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.RequirementSkills)
                .WithOne(item => item.Skill)
                .HasForeignKey(item => item.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasMany(item => item.Reviews)
                .WithOne(item => item.Project)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.ProjectSkills)
                .WithOne(item => item.Project)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.ResourceRequirements)
                .WithOne(item => item.Project)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.Attachments)
                .WithOne(item => item.Project)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.Milestones)
                .WithOne(item => item.Project)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.TimelineTasks)
                .WithOne(item => item.Project)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.Assignments)
                .WithOne(item => item.Project)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(item => item.Decisions)
                .WithOne(item => item.Project)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(item => item.CurrentContracts)
                .WithOne(item => item.CurrentProject)
                .HasForeignKey(item => item.CurrentProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProjectResourceRequirement>(entity =>
        {
            entity.HasMany(item => item.RequiredSkills)
                .WithOne(item => item.Requirement)
                .HasForeignKey(item => item.RequirementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasOne(item => item.Review)
                .WithOne(item => item.Assignment)
                .HasForeignKey<AssignmentReview>(item => item.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GmDecision>(entity =>
        {
            entity.HasMany(item => item.AffectedEmployees)
                .WithOne(item => item.Decision)
                .HasForeignKey(item => item.DecisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HiringRequest>(entity =>
        {
            entity.HasOne(item => item.GmDecision)
                .WithOne()
                .HasForeignKey<HiringRequest>(item => item.GmDecisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>().HasIndex(entity => entity.Name).IsUnique();
        modelBuilder.Entity<AppUser>().HasIndex(entity => entity.Email).IsUnique();
        modelBuilder.Entity<Employee>().HasIndex(entity => entity.UserId).IsUnique();
        modelBuilder.Entity<Employee>().HasIndex(entity => entity.EmployeeCode).IsUnique();
        modelBuilder.Entity<Skill>().HasIndex(entity => entity.Name).IsUnique();
        modelBuilder.Entity<ProjectSkill>().HasKey(entity => new { entity.ProjectId, entity.SkillId });
        modelBuilder.Entity<EmployeeSkill>().HasKey(entity => new { entity.EmployeeId, entity.SkillId });
        modelBuilder.Entity<ProjectRequirementSkill>().HasKey(entity => new { entity.RequirementId, entity.SkillId });
        modelBuilder.Entity<GmDecisionEmployee>().HasKey(entity => new { entity.DecisionId, entity.EmployeeId });

        modelBuilder.Entity<ProjectReview>().HasIndex(entity => entity.ProjectId);
        modelBuilder.Entity<ProjectResourceRequirement>().HasIndex(entity => entity.ProjectId);
        modelBuilder.Entity<ProjectAttachment>().HasIndex(entity => entity.ProjectId);
        modelBuilder.Entity<ProjectMilestone>().HasIndex(entity => entity.ProjectId);
        modelBuilder.Entity<ProjectMilestone>().HasIndex(entity => new { entity.ProjectId, entity.SortOrder });
        modelBuilder.Entity<ProjectTimelineTask>().HasIndex(entity => entity.ProjectId);
        modelBuilder.Entity<ProjectTimelineTask>().HasIndex(entity => new { entity.ProjectId, entity.SortOrder });
        modelBuilder.Entity<Assignment>().HasIndex(entity => entity.ProjectId);
        modelBuilder.Entity<Assignment>().HasIndex(entity => entity.EmployeeId);
        modelBuilder.Entity<Assignment>().HasIndex(entity => entity.Status);
        modelBuilder.Entity<AssignmentReview>().HasIndex(entity => entity.Status);
        modelBuilder.Entity<EmployeeContract>().HasIndex(entity => entity.EmployeeId);
        modelBuilder.Entity<GmDecision>().HasIndex(entity => entity.Status);
        modelBuilder.Entity<GmDecision>().HasIndex(entity => entity.DecisionType);
        modelBuilder.Entity<Notification>().HasIndex(entity => new { entity.UserId, entity.IsRead });
    }
}