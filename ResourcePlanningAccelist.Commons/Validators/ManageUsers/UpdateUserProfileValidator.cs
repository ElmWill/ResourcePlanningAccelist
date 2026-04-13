using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;

namespace ResourcePlanningAccelist.Commons.Validators.ManageUsers;

public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();

        RuleFor(request => request.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Email)
            .NotEmpty()
            .MaximumLength(320)
            .EmailAddress();
    }
}
