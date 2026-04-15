namespace ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;

public static class AuthorizationPolicyNames
{
    public const string GmOnly = "GmOnly";

    public const string PmOnly = "PmOnly";

    public const string MarketingOnly = "MarketingOnly";
    public const string HrOnly = "HrOnly";

    public const string PmOrHr = "PmOrHr";

    public const string GmOrPm = "GmOrPm";

    public const string HrOrGm = "HrOrGm";

    public const string PmHrOrGm = "PmHrOrGm";
    public const string EmployeeOnly = "EmployeeOnly";
    public const string AnyRole = "AnyRole";

    public const string ProjectReadAccess = "ProjectReadAccess";
}