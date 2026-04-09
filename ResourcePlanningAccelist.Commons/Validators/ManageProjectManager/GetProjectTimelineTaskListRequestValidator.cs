using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjectManager;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjectManager;

public class GetProjectTimelineTaskListRequestValidator : AbstractValidator<GetProjectTimelineTaskListRequest>
{
    public GetProjectTimelineTaskListRequestValidator()
    {
        RuleFor(request => request.PmUserId)
            .NotEmpty();

        RuleFor(request => request.ProjectId)
            .NotEmpty();
    }
}