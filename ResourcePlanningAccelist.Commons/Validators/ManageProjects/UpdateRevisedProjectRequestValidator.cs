using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjects;

public class UpdateRevisedProjectRequestValidator : AbstractValidator<UpdateRevisedProjectRequest>
{
    public UpdateRevisedProjectRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.CreatedByUserId)
            .NotEmpty();

        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.ClientName)
            .MaximumLength(200);

        RuleFor(request => request.Description)
            .MaximumLength(2000);

        RuleFor(request => request.StartDate)
            .LessThanOrEqualTo(request => request.EndDate)
            .WithMessage("StartDate must be less than or equal to EndDate.");

        RuleFor(request => request.Status)
            .NotEmpty()
            .Must(s => s.Equals("Draft", StringComparison.OrdinalIgnoreCase)
                    || s.Equals("Submitted", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Status must be 'Draft' or 'Submitted'.");
    }
}
