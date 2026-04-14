using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class GetProjectRevisionRequestHandler : IRequestHandler<GetProjectRevisionRequest, GetProjectRevisionResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectRevisionRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProjectRevisionResponse> Handle(GetProjectRevisionRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(p => p.Id == request.ProjectId)
            .Select(p => new GetProjectRevisionResponse
            {
                Id = p.Id,
                Name = p.Name,
                ClientName = p.ClientName,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                RejectionReason = p.RejectionReason,
                ProjectSkills = p.ProjectSkills.Select(ps => new ProjectSkillResponse
                {
                    SkillId = ps.SkillId,
                    SkillName = ps.Skill.Name
                }).ToList(),
                ResourceRequirements = p.ResourceRequirements
                    .OrderBy(r => r.SortOrder)
                    .Select(r => new ProjectResourceRequirementResponse
                    {
                        RequirementId = r.Id,
                        RoleName = r.RoleName,
                        Quantity = r.Quantity,
                        ExperienceLevel = r.ExperienceLevel.ToString(),
                        Notes = r.Notes,
                        SortOrder = r.SortOrder,
                        RequiredSkills = r.RequiredSkills.Select(rs => new ProjectRequirementSkillResponse
                        {
                            SkillId = rs.SkillId,
                            SkillName = rs.Skill.Name
                        }).ToList()
                    }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return project ?? throw new KeyNotFoundException("Project not found.");
    }
}
