using ResourcePlanningAccelist.Entities;

namespace ResourcePlanningAccelist.Commons.Security;

public interface IJwtProvider
{
    string Generate(AppUser user);
}
