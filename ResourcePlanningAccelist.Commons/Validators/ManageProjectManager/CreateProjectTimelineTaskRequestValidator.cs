using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjectManager;

public class CreateProjectTimelineTaskRequestValidator : AbstractValidator<CreateProjectTimelineTaskRequest>
{
    public CreateProjectTimelineTaskRequestValidator()
    {
        RuleFor(request => request.PmUserId)
            .NotEmpty();

        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.StartOffsetDays)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.DurationDays)
            .GreaterThan(0);

        RuleFor(request => request.ColorTag)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}