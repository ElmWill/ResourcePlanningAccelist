using ResourcePlanningAccelist.Commons.Security;
using static BCrypt.Net.BCrypt;

namespace ResourcePlanningAccelist.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return EnhancedHashPassword(password);
    }

    public bool Verify(string password, string hash)
    {
        return EnhancedVerify(password, hash);
    }
}
