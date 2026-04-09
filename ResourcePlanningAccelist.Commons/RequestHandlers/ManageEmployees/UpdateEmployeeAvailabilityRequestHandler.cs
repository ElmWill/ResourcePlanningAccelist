using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class UpdateEmployeeAvailabilityRequestHandler : IRequestHandler<UpdateEmployeeAvailabilityRequest, UpdateEmployeeAvailabilityResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateEmployeeAvailabilityRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateEmployeeAvailabilityResponse> Handle(UpdateEmployeeAvailabilityRequest request, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees.FirstOrDefaultAsync(item => item.Id == request.EmployeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Employee not found.");

        employee.AvailabilityPercent = request.AvailabilityPercent;

        employee.WorkloadState = employee.WorkloadPercent switch
        {
            <= 30 => WorkloadStatus.Available,
            <= 70 => WorkloadStatus.Moderate,
            <= 100 => WorkloadStatus.Busy,
            _ => WorkloadStatus.Overloaded
        };

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateEmployeeAvailabilityResponse
        {
            EmployeeId = employee.Id,
            AvailabilityPercent = employee.AvailabilityPercent,
            WorkloadPercent = employee.WorkloadPercent
        };
    }
}