using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

public class UpdateRevisedProjectRequest : IRequest<UpdateRevisedProjectResponse>
{
    public Guid ProjectId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ClientName { get; set; }

    public string? Description { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = "Submitted";

    public List<Guid> SkillIds { get; set; } = new();

    public List<UpdateProjectResourceRequirementItem> ResourceRequirements { get; set; } = new();
}

public class UpdateProjectResourceRequirementItem
{
    public Guid? RequirementId { get; set; } // Null for new requirements, existing ID for updates

    public string RoleName { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public string ExperienceLevel { get; set; } = "Mid";

    public string? Notes { get; set; }

    public int SortOrder { get; set; }

    public List<Guid> SkillIds { get; set; } = new();
}
