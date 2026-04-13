using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageHumanResources;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageHumanResources;
using ResourcePlanningAccelist.Entities;
using ResourcePlanningAccelist.Constants;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageHumanResources;

public class GetHiringListRequestHandler : IRequestHandler<GetHiringListRequest, GetHiringListResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetHiringListRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetHiringListResponse> Handle(GetHiringListRequest request, CancellationToken cancellationToken)
    {
        var hiringRequests = await _dbContext.HiringRequests
            .AsNoTracking()
            .Include(hr => hr.GmDecision)
                .ThenInclude(d => d.Project)
            .OrderByDescending(hr => hr.CreatedAt)
            .Select(hr => new HiringRequestItemResponse
            {
                Id = hr.Id,
                JobTitle = hr.JobTitle,
                ProjectName = hr.GmDecision.Project != null ? hr.GmDecision.Project.Name : "General",
                Details = hr.Details,
                Status = hr.Status.ToString(),
                StartedAt = hr.StartedAt ?? hr.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetHiringListResponse
        {
            HiringRequests = hiringRequests
        };
    }
}
