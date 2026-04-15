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

        var activeStatuses = new[] { 
            AssignmentStatus.Pending, 
            AssignmentStatus.GmApproved, 
            AssignmentStatus.Approved, 
            AssignmentStatus.Accepted, 
            AssignmentStatus.InProgress 
        };

        var employees = await query
            .OrderBy(employee => employee.User.FullName)
            .Select(employee => new 
            {
                Employee = employee,
                // On-the-fly calculation for absolute freshness
                CalculatedWorkload = employee.Assignments
                    .Where(a => activeStatuses.Contains(a.Status))
                    .GroupBy(a => a.ProjectId)
                    .Select(g => g.Sum(x => x.AllocationPercent) > 100m ? 100m : g.Sum(x => x.AllocationPercent))
                    .Sum()
            })
            .Select(item => new EmployeeListItemResponse
            {
                Id = item.Employee.Id,
                FullName = item.Employee.User.FullName,
                Email = item.Employee.User.Email,
                JobTitle = item.Employee.JobTitle,
                Department = item.Employee.Department != null ? item.Employee.Department.Name : null,
                WorkloadPercent = item.CalculatedWorkload,
                AvailabilityPercent = Math.Max(0m, 100m - item.CalculatedWorkload),
                Skills = item.Employee.EmployeeSkills
                    .OrderBy(es => es.IsPrimary ? 0 : 1)
                    .ThenBy(es => es.Skill.Name)
                    .Select(es => es.Skill.Name)
                    .ToList(),
                AssignedHours = Math.Round((item.CalculatedWorkload / 100m) * 40m, 2),
                Phone = item.Employee.Phone,
                Status = item.Employee.Status.ToString(),
                WorkloadStatus = (item.CalculatedWorkload <= 30m ? "Available" :
                                  item.CalculatedWorkload <= 70m ? "Moderate" :
                                  item.CalculatedWorkload <= 100m ? "Busy" : "Overloaded"),
                HireDate = item.Employee.HireDate,
                ContractEndDate = item.Employee.Contracts
                    .Where(c => c.Status == ContractStatus.Active)
                    .OrderByDescending(c => c.EndDate)
                    .Select(c => c.EndDate)
                    .FirstOrDefault(),
                Assignments = item.Employee.Assignments
                    .Where(a => a.Status == AssignmentStatus.Pending || 
                                a.Status == AssignmentStatus.GmApproved || 
                                a.Status == AssignmentStatus.Approved || 
                                a.Status == AssignmentStatus.Accepted || 
                                a.Status == AssignmentStatus.InProgress)
                    .Select(a => new AssignmentListItemResponse
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