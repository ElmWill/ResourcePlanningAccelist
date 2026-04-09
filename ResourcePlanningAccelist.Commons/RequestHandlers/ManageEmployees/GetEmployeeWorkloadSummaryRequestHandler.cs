using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class GetEmployeeWorkloadSummaryRequestHandler : IRequestHandler<GetEmployeeWorkloadSummaryRequest, GetEmployeeWorkloadSummaryResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetEmployeeWorkloadSummaryRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetEmployeeWorkloadSummaryResponse> Handle(GetEmployeeWorkloadSummaryRequest request, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees
            .AsNoTracking()
            .Where(item => item.Id == request.EmployeeId)
            .Select(item => new
            {
                item.Id,
                item.AvailabilityPercent,
                item.WorkloadPercent,
                item.AssignedHours,
                item.WorkloadState
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Employee not found.");

        var activeStatuses = new[]
        {
            AssignmentStatus.Pending,
            AssignmentStatus.Approved,
            AssignmentStatus.Accepted,
            AssignmentStatus.InProgress
        };

        var activeAssignmentCount = await _dbContext.Assignments
            .AsNoTracking()
            .CountAsync(
                item => item.EmployeeId == request.EmployeeId && activeStatuses.Contains(item.Status),
                cancellationToken);

        return new GetEmployeeWorkloadSummaryResponse
        {
            EmployeeId = employee.Id,
            AvailabilityPercent = employee.AvailabilityPercent,
            WorkloadPercent = employee.WorkloadPercent,
            AssignedHours = employee.AssignedHours,
            ActiveAssignmentCount = activeAssignmentCount,
            WorkloadState = employee.WorkloadState.ToString()
        };
    }
}