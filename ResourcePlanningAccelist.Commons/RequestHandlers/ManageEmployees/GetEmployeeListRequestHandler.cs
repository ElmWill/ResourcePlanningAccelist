using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageEmployees;

public class GetEmployeeListRequestHandler : IRequestHandler<GetEmployeeListRequest, GetEmployeeListResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetEmployeeListRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetEmployeeListResponse> Handle(GetEmployeeListRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Employees
            .AsNoTracking()
            .Include(employee => employee.User)
            .Include(employee => employee.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(employee => employee.Department != null && employee.Department.Name == request.Department);
        }

        var employees = await query
            .OrderBy(employee => employee.User.FullName)
            .Select(employee => new EmployeeListItemResponse
            {
                Id = employee.Id,
                FullName = employee.User.FullName,
                Email = employee.User.Email,
                JobTitle = employee.JobTitle,
                Department = employee.Department != null ? employee.Department.Name : null,
                AvailabilityPercent = employee.AvailabilityPercent,
                WorkloadPercent = employee.WorkloadPercent,
                AssignedHours = employee.AssignedHours,
                Status = employee.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new GetEmployeeListResponse
        {
            Employees = employees
        };
    }
}