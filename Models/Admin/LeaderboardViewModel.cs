using RateMyTeacher.Services;

namespace RateMyTeacher.Models.Admin;

public class LeaderboardPageViewModel
{
    public required BonusSettings Settings { get; init; }
    public required LeaderboardResult Leaderboard { get; init; }
    public IReadOnlyList<BonusPayout> Payouts { get; init; } = Array.Empty<BonusPayout>();
    public IReadOnlyList<SemesterOptionViewModel> Semesters { get; init; } = Array.Empty<SemesterOptionViewModel>();
    public int SelectedSemesterId { get; init; }
    public string? StatusMessage { get; set; }

    public bool HasQualifiedTeachers => Leaderboard.Entries.Any();
}

public sealed record SemesterOptionViewModel(int Id, string Name, bool IsCurrent);

public class LeaderboardAwardRequest
{
    public int SemesterId { get; set; }
}
