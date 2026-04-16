using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageTaskAssignments;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageTaskAssignments;

public class DeleteTaskAssignmentRequestHandler : IRequestHandler<DeleteTaskAssignmentRequest, DeleteTaskAssignmentResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public DeleteTaskAssignmentRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DeleteTaskAssignmentResponse> Handle(DeleteTaskAssignmentRequest request, CancellationToken cancellationToken)
    {
        var taskAssignment = await _dbContext.TaskAssignments
            .FirstOrDefaultAsync(item => item.Id == request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task assignment with ID {request.TaskId} not found");

        var employeeId = taskAssignment.EmployeeId;

        _dbContext.TaskAssignments.Remove(taskAssignment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AssignmentWorkloadUpdater.RecalculateEmployeeWorkloadAsync(_dbContext, employeeId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteTaskAssignmentResponse
        {
            TaskId = request.TaskId,
        };
    }
}
