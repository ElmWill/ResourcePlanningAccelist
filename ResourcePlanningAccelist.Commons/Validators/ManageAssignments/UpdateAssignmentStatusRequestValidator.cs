using FluentValidation;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;

namespace ResourcePlanningAccelist.Commons.Validators.ManageAssignments;

public class UpdateAssignmentStatusRequestValidator : AbstractValidator<UpdateAssignmentStatusRequest>
{
    public UpdateAssignmentStatusRequestValidator()
    {
        RuleFor(request => request.AssignmentId)
            .NotEmpty();

        RuleFor(request => request.Status)
            .NotEmpty()
            .Must(status => Enum.TryParse<AssignmentStatus>(status, true, out _))
            .WithMessage("Status must be a valid AssignmentStatus value.");
    }
}