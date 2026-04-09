using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjectManager;

public class GetProjectManagerProjectTeamRequestValidator : AbstractValidator<GetProjectManagerProjectTeamRequest>
{
    public GetProjectManagerProjectTeamRequestValidator()
    {
        RuleFor(request => request.PmUserId)
            .NotEmpty();

        RuleFor(request => request.ProjectId)
            .NotEmpty();
    }
}