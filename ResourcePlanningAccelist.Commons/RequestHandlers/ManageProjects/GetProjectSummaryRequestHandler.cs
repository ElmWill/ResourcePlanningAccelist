using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageProjects;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageProjects;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageProjects;

public class GetProjectSummaryRequestHandler : IRequestHandler<GetProjectSummaryRequest, GetProjectSummaryResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectSummaryRequestHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProjectSummaryResponse> Handle(GetProjectSummaryRequest request, CancellationToken cancellationToken)
    {
        // Query the database to group by status and retrieve counts efficiently
        var statusCounts = await _dbContext.Projects
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(
                k => k.Status.ToString(),
                v => v.Count,
                cancellationToken
            );

        int totalCount = statusCounts.Values.Sum();

        return new GetProjectSummaryResponse
        {
            TotalProjects = totalCount,
            ProjectsByStatus = statusCounts
        };
    }
}
