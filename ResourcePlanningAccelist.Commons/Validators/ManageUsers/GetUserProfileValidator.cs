using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;

namespace ResourcePlanningAccelist.Commons.Validators.ManageUsers;

public class GetUserProfileValidator : AbstractValidator<GetUserProfileRequest>
{
    public GetUserProfileValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();
    }
}
