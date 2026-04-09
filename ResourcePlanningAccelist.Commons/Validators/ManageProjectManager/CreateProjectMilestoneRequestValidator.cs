using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjectManager;

public class CreateProjectMilestoneRequestValidator : AbstractValidator<CreateProjectMilestoneRequest>
{
    public CreateProjectMilestoneRequestValidator()
    {
        RuleFor(request => request.PmUserId)
            .NotEmpty();

        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Description)
            .MaximumLength(1000);

        RuleFor(request => request.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}