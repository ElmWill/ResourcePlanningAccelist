using FluentValidation;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;

namespace ResourcePlanningAccelist.Commons.Validators.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectPmRecommendationRequestValidator : AbstractValidator<GetGeneralManagerProjectPmRecommendationRequest>
{
    public GetGeneralManagerProjectPmRecommendationRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.CandidateLimit)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(GeneralManagerPredictionConstants.MaxCandidateLimit)
            .When(request => request.CandidateLimit.HasValue);
    }
}
