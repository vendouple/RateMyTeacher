using RateMyTeacher.Models;

namespace RateMyTeacher.Services;

public interface IBonusService
{
    Task<BonusSettings> GetSettingsAsync(CancellationToken cancellationToken = default);

    IReadOnlyList<BonusPayout> CalculatePayouts(
        IReadOnlyList<TeacherLeaderboardEntry> leaderboard,
        BonusSettings settings);

    Task<BonusAwardResult> AwardAsync(
        LeaderboardResult leaderboard,
        IReadOnlyList<BonusPayout> payouts,
        int? awardedByUserId,
        CancellationToken cancellationToken = default);
}

public sealed record BonusSettings(int MinimumRatingsThreshold, IReadOnlyList<BonusTierSetting> Tiers, string CurrencyCode);

public sealed record BonusTierSetting(int? Position, int? RangeStart, int? RangeEnd, decimal Amount);

public sealed record BonusPayout(
    TeacherLeaderboardEntry Entry,
    decimal AwardedAmount,
    decimal BaseAmount,
    string TierLabel,
    bool SplitAcrossTies);

public sealed record BonusAwardResult(int SemesterId, Guid BatchId, int AwardedCount);
