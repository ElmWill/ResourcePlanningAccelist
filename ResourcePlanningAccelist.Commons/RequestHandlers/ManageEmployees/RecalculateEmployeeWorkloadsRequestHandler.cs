using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class RecalculateEmployeeWorkloadsRequestHandler : IRequestHandler<RecalculateEmployeeWorkloadsRequest, RecalculateEmployeeWorkloadsResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public RecalculateEmployeeWorkloadsRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RecalculateEmployeeWorkloadsResponse> Handle(RecalculateEmployeeWorkloadsRequest request, CancellationToken cancellationToken)
    {
        var employeeIds = await _dbContext.Employees
            .AsNoTracking()
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

        foreach (var employeeId in employeeIds)
        {
            await AssignmentWorkloadUpdater.RecalculateEmployeeWorkloadAsync(_dbContext, employeeId, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var overloadedCount = await _dbContext.Employees
            .AsNoTracking()
            .CountAsync(item => item.WorkloadPercent > 100m || item.WorkloadState == WorkloadStatus.Overloaded, cancellationToken);

        return new RecalculateEmployeeWorkloadsResponse
        {
            TotalEmployeesRecalculated = employeeIds.Count,
            OverloadedEmployeeCount = overloadedCount,
        };
    }
}
