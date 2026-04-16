using System;
using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;

namespace ResourcePlanningAccelist.Commons.Validators.ManageUsers
{
    public class UpdateUserPasswordRequestValidator : AbstractValidator<UpdateUserPasswordRequest>
    {
        public UpdateUserPasswordRequestValidator()
        {
            RuleFor(request => request.UserId).NotEmpty();

            RuleFor(request => request.CurrentPassword).NotEmpty();

            RuleFor(request => request.NewPassword).NotEmpty().MinimumLength(6);

            RuleFor(request => request.ConfirmNewPassword)
                .NotEmpty()
                .Equal(request => request.NewPassword)
                .WithMessage("Passwords do not match.");
        }
    }
}
