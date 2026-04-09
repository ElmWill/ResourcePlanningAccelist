using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageLookups;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageLookups;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageLookups;

public class GetDepartmentListRequestHandler : IRequestHandler<GetDepartmentListRequest, GetDepartmentListResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetDepartmentListRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetDepartmentListResponse> Handle(GetDepartmentListRequest request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(request.PageNumber ?? PaginationDefaults.PageNumber, PaginationDefaults.PageNumber);
        var requestedPageSize = request.PageSize ?? PaginationDefaults.PageSize;
        var pageSize = Math.Clamp(requestedPageSize, 1, PaginationDefaults.MaxPageSize);

        var query = _dbContext.Departments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            query = query.Where(item => item.Name.Contains(request.Query));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var departments = await query
            .OrderBy(item => item.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new DepartmentListItemResponse
            {
                Id = item.Id,
                Name = item.Name
            })
            .ToListAsync(cancellationToken);

        return new GetDepartmentListResponse
        {
            Departments = departments,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}