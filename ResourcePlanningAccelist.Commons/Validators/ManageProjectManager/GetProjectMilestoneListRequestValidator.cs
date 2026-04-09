using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjectManager;

public class GetProjectMilestoneListRequestValidator : AbstractValidator<GetProjectMilestoneListRequest>
{
    public GetProjectMilestoneListRequestValidator()
    {
        RuleFor(request => request.PmUserId)
            .NotEmpty();

        RuleFor(request => request.ProjectId)
            .NotEmpty();
    }
}