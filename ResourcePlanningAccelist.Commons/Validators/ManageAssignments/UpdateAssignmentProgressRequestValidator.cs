using FluentValidation;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;

namespace ResourcePlanningAccelist.Commons.Validators.ManageAssignments;

public class UpdateAssignmentProgressRequestValidator : AbstractValidator<UpdateAssignmentProgressRequest>
{
    public UpdateAssignmentProgressRequestValidator()
    {
        RuleFor(request => request.AssignmentId)
            .NotEmpty();

        RuleFor(request => request.ProgressPercent)
            .InclusiveBetween(0, 100);
    }
}