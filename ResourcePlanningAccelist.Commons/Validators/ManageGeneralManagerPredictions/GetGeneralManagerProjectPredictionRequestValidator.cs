using FluentValidation;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageGeneralManagerPredictions;

namespace ResourcePlanningAccelist.Commons.Validators.ManageGeneralManagerPredictions;

public class GetGeneralManagerProjectPredictionRequestValidator : AbstractValidator<GetGeneralManagerProjectPredictionRequest>
{
    public GetGeneralManagerProjectPredictionRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .NotEmpty();

        RuleFor(request => request.CandidateLimit)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(GeneralManagerPredictionConstants.MaxCandidateLimit)
            .When(request => request.CandidateLimit.HasValue);
    }
}