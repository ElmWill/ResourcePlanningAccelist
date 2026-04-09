using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjectManager;

public class AssignProjectManagerRequestValidator : AbstractValidator<AssignProjectManagerRequest>
{
    public AssignProjectManagerRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.PmUserId)
            .NotEmpty();
    }
}