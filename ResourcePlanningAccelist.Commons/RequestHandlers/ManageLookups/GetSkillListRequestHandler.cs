using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Constants;
using ResourcePlanningAccelist.Constants;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageLookups;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageLookups;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageLookups;

public class GetSkillListRequestHandler : IRequestHandler<GetSkillListRequest, GetSkillListResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetSkillListRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetSkillListResponse> Handle(GetSkillListRequest request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(request.PageNumber ?? PaginationDefaults.PageNumber, PaginationDefaults.PageNumber);
        var requestedPageSize = request.PageSize ?? PaginationDefaults.PageSize;
        var pageSize = Math.Clamp(requestedPageSize, 1, PaginationDefaults.MaxPageSize);

        var query = _dbContext.Skills.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            query = query.Where(item => item.Name.Contains(request.Query));
        }

        if (!string.IsNullOrWhiteSpace(request.Category) && Enum.TryParse<SkillCategory>(request.Category, true, out var category))
        {
            query = query.Where(item => item.Category == category);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var skills = await query
            .OrderBy(item => item.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new SkillListItemResponse
            {
                Id = item.Id,
                Name = item.Name,
                Category = item.Category.ToString()
            })
            .ToListAsync(cancellationToken);

        return new GetSkillListResponse
        {
            Skills = skills,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}