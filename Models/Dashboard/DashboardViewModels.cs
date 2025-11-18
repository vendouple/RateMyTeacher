namespace RateMyTeacher.Models.Dashboard;

public sealed record DashboardCardViewModel(
    string Title,
    string Value,
    string Description,
    string Icon,
    string? ActionText = null,
    string? ActionController = null,
    string? ActionName = null);

public sealed record DashboardQuickLinkViewModel(
    string Label,
    string Controller,
    string Action,
    string Icon,
    object? RouteValues = null
);

public sealed record DashboardActivityViewModel(
    string Title,
    string Description,
    DateTime Timestamp
);

public sealed record TeacherDashboardRatingViewModel(
    string StudentName,
    string SemesterName,
    int Stars,
    string? Comment,
    DateTime CreatedAt
);

public sealed record StudentSuggestedTeacherViewModel(
    int TeacherId,
    string TeacherName,
    double AverageRating,
    int RatingCount,
    string? Department
);

public class AdminDashboardViewModel
{
    public IReadOnlyList<DashboardCardViewModel> Cards { get; init; } = Array.Empty<DashboardCardViewModel>();
    public IReadOnlyList<DashboardActivityViewModel> Activities { get; init; } = Array.Empty<DashboardActivityViewModel>();
    public IReadOnlyList<DashboardQuickLinkViewModel> QuickLinks { get; init; } = Array.Empty<DashboardQuickLinkViewModel>();
}

public class TeacherDashboardViewModel
{
    public bool HasProfile { get; init; }
    public string TeacherName { get; init; } = string.Empty;
    public double AverageRating { get; init; }
    public int RatingsCount { get; init; }
    public int SummaryCount { get; init; }
    public int AiSessions { get; init; }
    public IReadOnlyList<TeacherDashboardRatingViewModel> RecentRatings { get; init; } = Array.Empty<TeacherDashboardRatingViewModel>();
    public IReadOnlyList<DashboardQuickLinkViewModel> QuickLinks { get; init; } = Array.Empty<DashboardQuickLinkViewModel>();
}

public class StudentDashboardViewModel
{
    public bool HasProfile { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public int RatingsSubmitted { get; init; }
    public int AvailableTeachers { get; init; }
    public int AiSessions { get; init; }
    public IReadOnlyList<StudentSuggestedTeacherViewModel> Suggestions { get; init; } = Array.Empty<StudentSuggestedTeacherViewModel>();
    public IReadOnlyList<DashboardQuickLinkViewModel> QuickLinks { get; init; } = Array.Empty<DashboardQuickLinkViewModel>();
}
