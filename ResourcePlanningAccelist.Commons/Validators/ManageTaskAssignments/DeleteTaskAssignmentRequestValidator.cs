using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;

namespace ResourcePlanningAccelist.Commons.Validators.ManageTaskAssignments;

public class DeleteTaskAssignmentRequestValidator : AbstractValidator<DeleteTaskAssignmentRequest>
{
    public DeleteTaskAssignmentRequestValidator()
    {
        RuleFor(request => request.TaskId)
            .NotEmpty()
            .WithMessage("Task ID is required");
    }
}
