using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;

namespace ResourcePlanningAccelist.Commons.Validators.ManageProjects;

public class GetProjectRevisionRequestValidator : AbstractValidator<GetProjectRevisionRequest>
{
    public GetProjectRevisionRequestValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}
