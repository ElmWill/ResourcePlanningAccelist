using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class GetEmployeeDetailRequestHandler : IRequestHandler<GetEmployeeDetailRequest, GetEmployeeDetailResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetEmployeeDetailRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetEmployeeDetailResponse> Handle(GetEmployeeDetailRequest request, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees
            .AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.Department)
            .Include(item => item.EmployeeSkills)
                .ThenInclude(item => item.Skill)
            .Where(item => item.Id == request.EmployeeId)
            .Select(item => new GetEmployeeDetailResponse
            {
                Id = item.Id,
                UserId = item.UserId,
                FullName = item.User.FullName,
                Email = item.User.Email,
                JobTitle = item.JobTitle,
                Department = item.Department != null ? item.Department.Name : null,
                AvailabilityPercent = item.AvailabilityPercent,
                WorkloadPercent = item.WorkloadPercent,
                Phone = item.Phone,
                Status = item.Status.ToString()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return employee ?? throw new KeyNotFoundException("Employee not found.");
    }
}