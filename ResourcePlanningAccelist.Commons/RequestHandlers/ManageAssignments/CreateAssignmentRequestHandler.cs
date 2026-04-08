using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;

public class CreateAssignmentRequestHandler : IRequestHandler<CreateAssignmentRequest, CreateAssignmentResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateAssignmentRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateAssignmentResponse> Handle(CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects.AnyAsync(project => project.Id == request.ProjectId, cancellationToken);
        var employeeExists = await _dbContext.Employees.AnyAsync(employee => employee.Id == request.EmployeeId, cancellationToken);

        if (!projectExists || !employeeExists)
        {
            throw new InvalidOperationException("Project or employee does not exist.");
        }

        var assignment = new Assignment
        {
            ProjectId = request.ProjectId,
            EmployeeId = request.EmployeeId,
            AssignedByUserId = request.AssignedByUserId,
            RoleName = request.RoleName,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AllocationPercent = request.AllocationPercent,
            Status = AssignmentStatus.Pending
        };

        _dbContext.Assignments.Add(assignment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateAssignmentResponse
        {
            AssignmentId = assignment.Id
        };
    }
}