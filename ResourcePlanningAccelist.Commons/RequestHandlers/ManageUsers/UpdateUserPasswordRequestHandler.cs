using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Security;
using ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers;
using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageUsers
{
    public class UpdateUserPasswordRequestHandler : IRequestHandler<UpdateUserPasswordRequest, UpdateUserPasswordResponse>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;

        public UpdateUserPasswordRequestHandler(ApplicationDbContext dbContext, IPasswordHasher passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<UpdateUserPasswordResponse> Handle(UpdateUserPasswordRequest request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return new UpdateUserPasswordResponse { Success = false, Message = "User not found." };
            }

            if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return new UpdateUserPasswordResponse { Success = false, Message = "Invalid current password." };
            }

            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new UpdateUserPasswordResponse { Success = true, Message = "Password updated successfully." };
        }
    }
}
