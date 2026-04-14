using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageTaskAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageTaskAssignments;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageTaskAssignments;

public class CreateTaskAssignmentRequestHandler : IRequestHandler<CreateTaskAssignmentRequest, CreateTaskAssignmentResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateTaskAssignmentRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateTaskAssignmentResponse> Handle(
        CreateTaskAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        // Validate project exists
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found");
        }

        // Validate employee exists
        var employee = await _dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {request.EmployeeId} not found");
        }

        var priority = Enum.TryParse<PriorityLevel>(request.Priority, ignoreCase: true, out var parsedPriority)
            ? parsedPriority
            : PriorityLevel.Medium;

        // Workload hours are the source of truth. Priority only provides fallback defaults.
        var workloadHours = request.WorkloadHours ?? (priority switch
        {
            PriorityLevel.Low => 20,
            PriorityLevel.Medium => 30,
            PriorityLevel.High => 50,
            _ => 30
        });

        // Create task assignment
        var taskAssignment = new TaskAssignment
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            EmployeeId = request.EmployeeId,
            AssignedByUserId = request.AssignedByUserId,
            TaskName = request.TaskName,
            Description = request.Description,
            Priority = priority,
            WorkloadHours = workloadHours,
            DueDate = request.DueDate,
            Status = ResourcePlanningAccelist.Entities.TaskStatus.Pending,
        };

        _dbContext.TaskAssignments.Add(taskAssignment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AssignmentWorkloadUpdater.RecalculateEmployeeWorkloadAsync(_dbContext, taskAssignment.EmployeeId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateTaskAssignmentResponse
        {
            TaskId = taskAssignment.Id,
        };
    }
}
