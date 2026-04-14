using ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers;
using ResourcePlanningAccelist.Entities;
using ResourcePlanningAccelist.Commons.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ResourcePlanningAccelist.Commons.RequestHandlers.ManageUsers;

public class LoginRequestHandler : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginRequestHandler(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(u => u.EmployeeProfile)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return new LoginResponse { Success = false, Message = "Invalid email or password." };
        }

        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return new LoginResponse { Success = false, Message = "Invalid email or password." };
        }

        if (!user.IsActive)
        {
            return new LoginResponse { Success = false, Message = "Account is inactive." };
        }

        var token = _jwtProvider.Generate(user);

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful.",
            Token = token,
            User = new UserProfileResponse
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Avatar = user.AvatarUrl,
                EmployeeId = user.EmployeeProfile?.Id
            }
        };
    }
}
