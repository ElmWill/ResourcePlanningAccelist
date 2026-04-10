using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageAssignments;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageAssignments;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageAssignments;

public class GetAssignmentListRequestHandler : IRequestHandler<GetAssignmentListRequest, GetAssignmentListResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAssignmentListRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetAssignmentListResponse> Handle(GetAssignmentListRequest request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(request.PageNumber ?? PaginationDefaults.PageNumber, PaginationDefaults.PageNumber);
        var requestedPageSize = request.PageSize ?? PaginationDefaults.PageSize;
        var pageSize = Math.Clamp(requestedPageSize, 1, PaginationDefaults.MaxPageSize);

        var query = _dbContext.Assignments
            .AsNoTracking()
            .Include(item => item.Project)
            .Include(item => item.Employee)
                .ThenInclude(item => item.User)
            .Include(item => item.AssignedByUser)
            .AsQueryable();

        if (request.ProjectId.HasValue)
        {
            query = query.Where(item => item.ProjectId == request.ProjectId.Value);
        }

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(item => item.EmployeeId == request.EmployeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(item => item.Status.ToString() == request.Status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var assignments = await query
            .OrderByDescending(item => item.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new AssignmentListItemResponse
            {
                Id = item.Id,
                ProjectId = item.ProjectId,
                ProjectName = item.Project.Name,
                EmployeeId = item.EmployeeId,
                EmployeeName = item.Employee.User.FullName,
                RoleName = item.RoleName,
                Status = item.Status.ToString(),
                AllocationPercent = item.AllocationPercent,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                RequestedByName = item.AssignedByUser.FullName,
                ConflictWarning = item.ConflictWarning
            })
            .ToListAsync(cancellationToken);

        return new GetAssignmentListResponse
        {
            Assignments = assignments,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}