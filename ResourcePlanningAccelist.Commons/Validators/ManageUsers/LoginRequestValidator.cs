using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;

namespace ResourcePlanningAccelist.Commons.Validators.ManageUsers;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
