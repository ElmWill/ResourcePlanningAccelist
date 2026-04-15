using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjects;

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(request => request.CreatedByUserId)
            .NotEmpty();

        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.ClientName)
            .MaximumLength(200);

        RuleFor(request => request.Description)
            .MaximumLength(2000);

        RuleFor(request => request.Status)
            .NotEmpty()
            .Must(status => status.ToLower() == "draft" || status.ToLower() == "submitted")
            .WithMessage("Status must be either 'Draft' or 'Submitted'.");

        RuleFor(request => request.StartDate)
            .LessThanOrEqualTo(request => request.EndDate)
            .WithMessage("StartDate must be less than or equal to EndDate.");
    }
}