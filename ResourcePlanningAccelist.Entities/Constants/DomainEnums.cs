namespace ResourcePlanningAccelist.Constants;

public enum UserRole
{
    Marketing,
    Pm,
    Gm,
    Hr,
    Employee
}

public enum SkillCategory
{
    Technical,
    Soft,
    Business
}

public enum ProjectStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected,
    Assigned,
    InProgress,
    Completed,
    Cancelled
}

public enum ProjectRiskLevel
{
    Low,
    Medium,
    High
}

public enum ExperienceLevel
{
    Junior,
    Mid,
    Senior
}

public enum PriorityLevel
{
    Low,
    Medium,
    High
}

public enum AssignmentStatus
{
    Pending,
    Approved,
    Rejected,
    Accepted,
    InProgress,
    Completed,
    Cancelled
}

public enum AssignmentReviewStatus
{
    Pending,
    Approved,
    Rejected
}

public enum ContractStatus
{
    Active,
    Extended,
    Terminated,
    Expired
}

public enum DecisionType
{
    ExtendContract,
    TerminateContract,
    HireResource,
    ProjectAssignment
}

public enum DecisionStatus
{
    Pending,
    Executed,
    ClarificationRequested
}

public enum NotificationType
{
    Assignment,
    Change,
    Alert,
    Feedback
}

public enum ReviewDecision
{
    Approved,
    Rejected,
    RevisionRequested
}

public enum WorkloadStatus
{
    Available,
    Moderate,
    Busy,
    Overloaded
}

public enum EmploymentStatus
{
    Active,
    Inactive,
    Resigned,
    Terminated
}

public enum TimelineTaskStatus
{
    Pending,
    InProgress,
    Completed
}