using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageUsers;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileRequest, GetUserProfileResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetUserProfileHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetUserProfileResponse> Handle(GetUserProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(item => item.Id == request.UserId)
            .Select(item => new GetUserProfileResponse
            {
                Id = item.Id,
                FullName = item.FullName,
                Email = item.Email,
                Role = item.Role.ToString(),
                AvatarUrl = item.AvatarUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        return user ?? throw new KeyNotFoundException("User not found.");
    }
}
