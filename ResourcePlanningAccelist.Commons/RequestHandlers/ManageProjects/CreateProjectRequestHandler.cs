using MediatR;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class CreateProjectRequestHandler : IRequestHandler<CreateProjectRequest, CreateProjectResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateProjectRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateProjectResponse> Handle(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            CreatedByUserId = request.CreatedByUserId,
            Name = request.Name,
            ClientName = request.ClientName,
            Description = request.Description,
            Notes = request.Notes,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = ProjectStatus.Draft,
            TotalRequiredResources = request.ResourceRequirements.Sum(r => r.Quantity)
        };

        _dbContext.Projects.Add(project);

        // Add project-level skills
        foreach (var skillId in request.SkillIds)
        {
            _dbContext.ProjectSkills.Add(new ProjectSkill
            {
                ProjectId = project.Id,
                SkillId = skillId
            });
        }

        // Add resource requirements and their per-requirement skills
        foreach (var reqItem in request.ResourceRequirements)
        {
            var requirement = new ProjectResourceRequirement
            {
                ProjectId = project.Id,
                RoleName = reqItem.RoleName,
                Quantity = reqItem.Quantity,
                ExperienceLevel = Enum.Parse<ExperienceLevel>(reqItem.ExperienceLevel, ignoreCase: true),
                Notes = reqItem.Notes,
                SortOrder = reqItem.SortOrder
            };

            _dbContext.ProjectResourceRequirements.Add(requirement);

            foreach (var skillId in reqItem.SkillIds)
            {
                _dbContext.ProjectRequirementSkills.Add(new ProjectRequirementSkill
                {
                    RequirementId = requirement.Id,
                    SkillId = skillId
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProjectResponse
        {
            ProjectId = project.Id
        };
    }
}