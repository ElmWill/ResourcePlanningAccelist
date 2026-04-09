using FluentValidation;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;

namespace ResourcePlanningAccelist.Commons.Validators.ManageGeneralManagerPredictions;

public class GetGeneralManagerWorkforceSummaryRequestValidator : AbstractValidator<GetGeneralManagerWorkforceSummaryRequest>
{
    public GetGeneralManagerWorkforceSummaryRequestValidator()
    {
        RuleFor(request => request.DepartmentId)
            .NotEmpty()
            .When(request => request.DepartmentId.HasValue);

        RuleFor(request => request.TopSkillLimit)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(20)
            .When(request => request.TopSkillLimit.HasValue);
    }
}
