using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Entities;
using ResourcePlanningAccelist.Commons.Helpers;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;

public class UpdateAssignmentStatusRequestHandler : IRequestHandler<UpdateAssignmentStatusRequest, UpdateAssignmentStatusResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateAssignmentStatusRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateAssignmentStatusResponse> Handle(UpdateAssignmentStatusRequest request, CancellationToken cancellationToken)
    {
        var assignment = await _dbContext.Assignments.FirstOrDefaultAsync(item => item.Id == request.AssignmentId, cancellationToken)
            ?? throw new KeyNotFoundException("Assignment not found.");

        if (!Enum.TryParse<AssignmentStatus>(request.Status, true, out var parsedStatus))
        {
            throw new InvalidOperationException("Invalid assignment status.");
        }

        assignment.Status = parsedStatus;

        if (parsedStatus == AssignmentStatus.Accepted)
        {
            assignment.AcceptedAt = DateTimeOffset.UtcNow;
        }

        if (parsedStatus == AssignmentStatus.Rejected)
        {
            assignment.RejectedAt = DateTimeOffset.UtcNow;
        }

        // SAVE FIRST so the recalculation query can see the new status
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Trigger Recalculation
        await WorkloadHelper.RecalculateEmployeeWorkloadAsync(assignment.EmployeeId, _dbContext, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateAssignmentStatusResponse
        {
            AssignmentId = assignment.Id,
            Status = assignment.Status.ToString()
        };
    }
}