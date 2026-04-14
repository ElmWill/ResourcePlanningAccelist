using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class UpdateRevisedProjectRequestHandler : IRequestHandler<UpdateRevisedProjectRequest, UpdateRevisedProjectResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateRevisedProjectRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateRevisedProjectResponse> Handle(UpdateRevisedProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FindAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");
        }

        // Update basic project fields
        project.Name = request.Name;
        project.ClientName = request.ClientName;
        project.Description = request.Description;
        project.Notes = request.Notes;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.Status = ProjectStatus.Draft;
        project.TotalRequiredResources = request.ResourceRequirements.Sum(r => r.Quantity);

        // Update project-level skills
        await UpdateProjectSkillsAsync(request.ProjectId, request.SkillIds, cancellationToken);

        // Update resource requirements
        await UpdateResourceRequirementsAsync(request.ProjectId, request.ResourceRequirements, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateRevisedProjectResponse
        {
            ProjectId = project.Id
        };
    }

    private async Task UpdateProjectSkillsAsync(Guid projectId, List<Guid> newSkillIds, CancellationToken cancellationToken)
    {
        var existingSkills = await _dbContext.ProjectSkills
            .Where(ps => ps.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        var existingSkillIds = existingSkills.Select(ps => ps.SkillId).ToHashSet();

        // Remove skills that are no longer needed
        var skillsToRemove = existingSkills.Where(ps => !newSkillIds.Contains(ps.SkillId)).ToList();
        _dbContext.ProjectSkills.RemoveRange(skillsToRemove);

        // Add new skills
        var skillsToAdd = newSkillIds.Where(skillId => !existingSkillIds.Contains(skillId))
            .Select(skillId => new ProjectSkill
            {
                ProjectId = projectId,
                SkillId = skillId
            })
            .ToList();

        _dbContext.ProjectSkills.AddRange(skillsToAdd);
    }

    private async Task UpdateResourceRequirementsAsync(Guid projectId, List<UpdateProjectResourceRequirementItem> requirementItems, CancellationToken cancellationToken)
    {
        var existingRequirements = await _dbContext.ProjectResourceRequirements
            .Include(rr => rr.RequiredSkills)
            .Where(rr => rr.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        var existingRequirementIds = existingRequirements.Select(rr => rr.Id).ToHashSet();
        var updatedRequirementIds = requirementItems
            .Where(item => item.RequirementId.HasValue)
            .Select(item => item.RequirementId!.Value)
            .ToHashSet();

        // Remove requirements that are no longer in the list
        var requirementsToRemove = existingRequirements
            .Where(rr => !updatedRequirementIds.Contains(rr.Id))
            .ToList();
        _dbContext.ProjectResourceRequirements.RemoveRange(requirementsToRemove);

        // Process each requirement item
        foreach (var item in requirementItems)
        {
            if (item.RequirementId.HasValue)
            {
                // Update existing requirement
                var existingRequirement = existingRequirements.First(rr => rr.Id == item.RequirementId.Value);
                existingRequirement.RoleName = item.RoleName;
                existingRequirement.Quantity = item.Quantity;
                existingRequirement.ExperienceLevel = Enum.Parse<ExperienceLevel>(item.ExperienceLevel, ignoreCase: true);
                existingRequirement.Notes = item.Notes;
                existingRequirement.SortOrder = item.SortOrder;

                // Update skills for this requirement
                await UpdateRequirementSkillsAsync(existingRequirement.Id, item.SkillIds, cancellationToken);
            }
            else
            {
                // Create new requirement
                var newRequirement = new ProjectResourceRequirement
                {
                    ProjectId = projectId,
                    RoleName = item.RoleName,
                    Quantity = item.Quantity,
                    ExperienceLevel = Enum.Parse<ExperienceLevel>(item.ExperienceLevel, ignoreCase: true),
                    Notes = item.Notes,
                    SortOrder = item.SortOrder
                };

                _dbContext.ProjectResourceRequirements.Add(newRequirement);

                // Add skills for the new requirement
                foreach (var skillId in item.SkillIds)
                {
                    _dbContext.ProjectRequirementSkills.Add(new ProjectRequirementSkill
                    {
                        RequirementId = newRequirement.Id,
                        SkillId = skillId
                    });
                }
            }
        }
    }

    private async Task UpdateRequirementSkillsAsync(Guid requirementId, List<Guid> newSkillIds, CancellationToken cancellationToken)
    {
        var existingSkills = await _dbContext.ProjectRequirementSkills
            .Where(prs => prs.RequirementId == requirementId)
            .ToListAsync(cancellationToken);

        var existingSkillIds = existingSkills.Select(prs => prs.SkillId).ToHashSet();

        // Remove skills that are no longer needed
        var skillsToRemove = existingSkills.Where(prs => !newSkillIds.Contains(prs.SkillId)).ToList();
        _dbContext.ProjectRequirementSkills.RemoveRange(skillsToRemove);

        // Add new skills
        var skillsToAdd = newSkillIds.Where(skillId => !existingSkillIds.Contains(skillId))
            .Select(skillId => new ProjectRequirementSkill
            {
                RequirementId = requirementId,
                SkillId = skillId
            })
            .ToList();

        _dbContext.ProjectRequirementSkills.AddRange(skillsToAdd);
    }
}
