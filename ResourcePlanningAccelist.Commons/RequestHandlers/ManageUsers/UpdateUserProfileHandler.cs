using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageUsers;

public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileRequest, UpdateUserProfileResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateUserProfileHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateUserProfileResponse> Handle(UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(item => item.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        user.FullName = request.FullName;
        user.Email = request.Email;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateUserProfileResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email
        };
    }
}
