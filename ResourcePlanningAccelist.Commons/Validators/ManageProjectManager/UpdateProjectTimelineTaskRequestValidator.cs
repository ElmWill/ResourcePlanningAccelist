using FluentValidation;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjectManager;

public class UpdateProjectTimelineTaskRequestValidator : AbstractValidator<UpdateProjectTimelineTaskRequest>
{
    public UpdateProjectTimelineTaskRequestValidator()
    {
        RuleFor(request => request.PmUserId)
            .NotEmpty();

        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.TimelineTaskId)
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

        RuleFor(request => request.Status)
            .NotEmpty()
            .Must(status => Enum.TryParse<TimelineTaskStatus>(status, true, out _))
            .WithMessage("Status must be a valid TimelineTaskStatus value.");
    }
}