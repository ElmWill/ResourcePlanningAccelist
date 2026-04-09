using FluentValidation;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;

namespace ResourcePlanningAccelist.Commons.Validators.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectRiskRequestValidator : AbstractValidator<GetGeneralManagerProjectRiskRequest>
{
    public GetGeneralManagerProjectRiskRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.CandidateLimit)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(GeneralManagerPredictionConstants.MaxCandidateLimit)
            .When(request => request.CandidateLimit.HasValue);
    }
}
