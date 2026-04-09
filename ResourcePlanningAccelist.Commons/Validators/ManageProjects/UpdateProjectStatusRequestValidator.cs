using FluentValidation;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjects;

public class UpdateProjectStatusRequestValidator : AbstractValidator<UpdateProjectStatusRequest>
{
    public UpdateProjectStatusRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.Status)
            .NotEmpty()
            .Must(status => Enum.TryParse<ProjectStatus>(status, true, out _))
            .WithMessage("Status must be a valid ProjectStatus value.");

        RuleFor(request => request.RejectionReason)
            .MaximumLength(2000);
    }
}