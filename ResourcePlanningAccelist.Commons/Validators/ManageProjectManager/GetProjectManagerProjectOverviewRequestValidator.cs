using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjectManager;

public class GetProjectManagerProjectOverviewRequestValidator : AbstractValidator<GetProjectManagerProjectOverviewRequest>
{
    public GetProjectManagerProjectOverviewRequestValidator()
    {
        RuleFor(request => request.PmUserId)
            .NotEmpty();

        RuleFor(request => request.ProjectId)
            .NotEmpty();
    }
}