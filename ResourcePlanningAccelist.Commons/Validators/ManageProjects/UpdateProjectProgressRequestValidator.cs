using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjects;

public class UpdateProjectProgressRequestValidator : AbstractValidator<UpdateProjectProgressRequest>
{
    public UpdateProjectProgressRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.ProgressPercent)
            .InclusiveBetween(0, 100);
    }
}