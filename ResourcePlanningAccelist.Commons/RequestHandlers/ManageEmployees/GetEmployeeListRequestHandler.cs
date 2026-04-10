using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageEmployees;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageEmployees;
using ResourcePlanningAccelist.Entities;
using ResourcePlanningAccelist.Constants;

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
            .Include(employee => employee.Assignments)
                .ThenInclude(a => a.Project)
            .Include(employee => employee.EmployeeSkills)
                .ThenInclude(es => es.Skill)
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
                Phone = employee.Phone,
                Status = employee.Status.ToString(),
                HireDate = employee.HireDate,
                Skills = employee.EmployeeSkills.Select(es => es.Skill.Name).ToList(),
                Assignments = employee.Assignments.Where(a => a.Status == AssignmentStatus.Approved || a.Status == AssignmentStatus.Accepted).Select(a => new AssignmentListItemResponse
                {
                    Id = a.Id,
                    ProjectId = a.ProjectId,
                    ProjectName = a.Project.Name,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee.User.FullName,
                    RoleName = a.RoleName,
                    Status = a.Status.ToString(),
                    AllocationPercent = a.AllocationPercent,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new GetEmployeeListResponse
        {
            Employees = employees
        };
    }
}