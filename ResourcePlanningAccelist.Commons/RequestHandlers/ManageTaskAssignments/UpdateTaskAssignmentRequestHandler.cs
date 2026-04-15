using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageTaskAssignments;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageTaskAssignments;

public class UpdateTaskAssignmentRequestHandler : IRequestHandler<UpdateTaskAssignmentRequest, UpdateTaskAssignmentResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateTaskAssignmentRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateTaskAssignmentResponse> Handle(
        UpdateTaskAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        // Fetch the task assignment
        var taskAssignment = await _dbContext.TaskAssignments
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);
        if (taskAssignment == null)
        {
            throw new KeyNotFoundException($"Task assignment with ID {request.TaskId} not found");
        }

        // Update status
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<ResourcePlanningAccelist.Entities.TaskStatus>(request.Status, ignoreCase: true, out var status))
            {
                taskAssignment.Status = status;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.TaskName))
        {
            taskAssignment.TaskName = request.TaskName.Trim();
        }

        if (request.Description is not null)
        {
            taskAssignment.Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();
        }

        // Update priority if provided
        if (!string.IsNullOrEmpty(request.Priority))
        {
            if (Enum.TryParse<PriorityLevel>(request.Priority, ignoreCase: true, out var priority))
            {
                taskAssignment.Priority = priority;
                // Recalculate workload hours based on new priority
                taskAssignment.WorkloadHours = priority switch
                {
                    PriorityLevel.Low => 20,
                    PriorityLevel.Medium => 30,
                    PriorityLevel.High => 50,
                    _ => 30
                };
            }
        }

        if (request.WorkloadHours.HasValue)
        {
            taskAssignment.WorkloadHours = request.WorkloadHours.Value;
        }

        // Update due date if provided
        if (request.DueDate.HasValue)
        {
            taskAssignment.DueDate = request.DueDate.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await AssignmentWorkloadUpdater.RecalculateEmployeeWorkloadAsync(_dbContext, taskAssignment.EmployeeId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateTaskAssignmentResponse
        {
            TaskId = taskAssignment.Id,
            Status = taskAssignment.Status.ToString(),
        };
    }
}
