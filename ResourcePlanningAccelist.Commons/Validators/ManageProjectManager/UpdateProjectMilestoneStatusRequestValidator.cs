using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjectManager;

public class UpdateProjectMilestoneStatusRequestValidator : AbstractValidator<UpdateProjectMilestoneStatusRequest>
{
    public UpdateProjectMilestoneStatusRequestValidator()
    {
        RuleFor(request => request.PmUserId)
            .NotEmpty();

        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.MilestoneId)
            .NotEmpty();
    }
}