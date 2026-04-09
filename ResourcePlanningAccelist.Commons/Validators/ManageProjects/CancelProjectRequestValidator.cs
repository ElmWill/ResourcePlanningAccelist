using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjects;

public class CancelProjectRequestValidator : AbstractValidator<CancelProjectRequest>
{
    public CancelProjectRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.Reason)
            .NotEmpty()
            .MaximumLength(2000);
    }
}