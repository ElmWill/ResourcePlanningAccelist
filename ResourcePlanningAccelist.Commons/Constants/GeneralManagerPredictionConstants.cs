namespace ResourcePlanningAccelist.Commons.Constants;

public static class GeneralManagerPredictionConstants
{
    public const int DefaultCandidateLimit = 5;

    public const int MaxCandidateLimit = 10;

    public const decimal SkillWeight = 0.55m;

    public const decimal CapacityWeight = 0.25m;

    public const decimal HistoryWeight = 0.10m;

    public const decimal RoleExperienceWeight = 0.10m;

    public const int ExplicitSkillMaxProficiency = 5;

    public const decimal PrimarySkillBonus = 0.08m;

    public const decimal HistoryNormalizationFactor = 3m;

    public const decimal ExperienceNormalizationAssignments = 5m;

    public const decimal CompletionNormalizationAssignments = 10m;

    public const int HistoricalDecayWindowDays = 180;
}