using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;

public class UpdateAssignmentProgressRequestHandler : IRequestHandler<UpdateAssignmentProgressRequest, UpdateAssignmentProgressResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateAssignmentProgressRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateAssignmentProgressResponse> Handle(UpdateAssignmentProgressRequest request, CancellationToken cancellationToken)
    {
        var assignment = await _dbContext.Assignments.FirstOrDefaultAsync(item => item.Id == request.AssignmentId, cancellationToken)
            ?? throw new KeyNotFoundException("Assignment not found.");

        assignment.ProgressPercent = request.ProgressPercent;

        if (request.ProgressPercent == 100)
        {
            assignment.Status = AssignmentStatus.Completed;
        }
        else if (request.ProgressPercent > 0 && assignment.Status is AssignmentStatus.Accepted or AssignmentStatus.Approved)
        {
            assignment.Status = AssignmentStatus.InProgress;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await AssignmentWorkloadUpdater.RecalculateEmployeeWorkloadAsync(_dbContext, assignment.EmployeeId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateAssignmentProgressResponse
        {
            AssignmentId = assignment.Id,
            ProgressPercent = assignment.ProgressPercent,
            Status = assignment.Status.ToString()
        };
    }
}