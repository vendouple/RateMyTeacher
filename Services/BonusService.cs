using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RateMyTeacher.Data;
using RateMyTeacher.Models;

namespace RateMyTeacher.Services;

public class BonusService : IBonusService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BonusService> _logger;
    private readonly string _tieStrategy;

    public BonusService(
        ApplicationDbContext dbContext,
        IConfiguration configuration,
        ILogger<BonusService> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
        _tieStrategy = configuration["BONUS_TIE_STRATEGY"]?.Trim().ToLowerInvariant() ?? "split";
    }

    public async Task<BonusSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var config = await _dbContext.BonusConfigs
            .Include(c => c.Tiers)
            .OrderByDescending(c => c.ModifiedAt)
            .ThenByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (config is null || !config.Tiers.Any())
        {
            config = await EnsureDefaultConfigAsync(cancellationToken);
        }

        var tiers = config.Tiers
            .OrderBy(t => t.Position ?? int.MaxValue)
            .ThenBy(t => t.RangeStart ?? int.MaxValue)
            .Select(t => new BonusTierSetting(t.Position, t.RangeStart, t.RangeEnd, t.Amount))
            .ToList();

        var settings = new BonusSettings(
            config.MinimumRatingsThreshold,
            tiers,
            GetCurrencyCode());

        return settings;
    }

    public IReadOnlyList<BonusPayout> CalculatePayouts(
        IReadOnlyList<TeacherLeaderboardEntry> leaderboard,
        BonusSettings settings)
    {
        if (leaderboard.Count == 0 || settings.Tiers.Count == 0)
        {
            return Array.Empty<BonusPayout>();
        }

        var payouts = new List<BonusPayout>();
        var awardedTeacherIds = new HashSet<int>();

        var positionTiers = settings.Tiers
            .Where(t => t.Position.HasValue)
            .OrderBy(t => t.Position!.Value)
            .ToList();

        foreach (var tier in positionTiers)
        {
            var matches = leaderboard
                .Where(entry => entry.Rank == tier.Position!.Value)
                .ToList();

            AddPayoutsForTier(matches, tier, payouts, awardedTeacherIds);
        }

        var rangeTiers = settings.Tiers
            .Where(t => !t.Position.HasValue && t.RangeStart.HasValue && t.RangeEnd.HasValue)
            .OrderBy(t => t.RangeStart!.Value)
            .ToList();

        foreach (var tier in rangeTiers)
        {
            var matches = leaderboard
                .Where(entry =>
                    !awardedTeacherIds.Contains(entry.TeacherId) &&
                    entry.Rank >= tier.RangeStart &&
                    entry.Rank <= tier.RangeEnd)
                .ToList();

            AddPayoutsForTier(matches, tier, payouts, awardedTeacherIds);
        }

        return payouts;
    }

    public async Task<BonusAwardResult> AwardAsync(
        LeaderboardResult leaderboard,
        IReadOnlyList<BonusPayout> payouts,
        int? awardedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (payouts.Count == 0)
        {
            return new BonusAwardResult(leaderboard.Semester.Id, Guid.Empty, 0);
        }

        var batchId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var rankings = await _dbContext.TeacherRankings
            .Where(tr => tr.SemesterId == leaderboard.Semester.Id)
            .ToDictionaryAsync(tr => tr.TeacherId, cancellationToken);

        foreach (var payout in payouts)
        {
            if (rankings.TryGetValue(payout.Entry.TeacherId, out var ranking))
            {
                ranking.Rank = payout.Entry.Rank;
                ranking.AverageRating = decimal.Round((decimal)payout.Entry.AverageRating, 2);
                ranking.TotalRatings = payout.Entry.TotalRatings;
                ranking.BonusAmount = payout.AwardedAmount;
                ranking.CalculatedAt = now;
            }
            else
            {
                _dbContext.TeacherRankings.Add(new TeacherRanking
                {
                    TeacherId = payout.Entry.TeacherId,
                    SemesterId = leaderboard.Semester.Id,
                    Rank = payout.Entry.Rank,
                    AverageRating = decimal.Round((decimal)payout.Entry.AverageRating, 2),
                    TotalRatings = payout.Entry.TotalRatings,
                    BonusAmount = payout.AwardedAmount,
                    CalculatedAt = now
                });
            }

            _dbContext.Bonuses.Add(new Bonus
            {
                BatchId = batchId,
                TeacherId = payout.Entry.TeacherId,
                SemesterId = leaderboard.Semester.Id,
                Rank = payout.Entry.Rank,
                AwardedAmount = payout.AwardedAmount,
                BaseAmount = payout.BaseAmount,
                TierLabel = payout.TierLabel,
                SplitAcrossTies = payout.SplitAcrossTies,
                AwardedAt = now,
                AwardedById = awardedByUserId
            });
        }

        var saved = await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Awarded {AwardCount} bonuses for semester {SemesterId} with batch {BatchId}.",
            payouts.Count,
            leaderboard.Semester.Id,
            batchId);

        return new BonusAwardResult(leaderboard.Semester.Id, batchId, payouts.Count);
    }

    private void AddPayoutsForTier(
        IReadOnlyList<TeacherLeaderboardEntry> matches,
        BonusTierSetting tier,
        ICollection<BonusPayout> payouts,
        ISet<int> awardedTeacherIds)
    {
        if (matches.Count == 0)
        {
            return;
        }

        var baseAmount = tier.Amount;
        decimal amountPerTeacher = baseAmount;
        var splitAcrossTies = matches.Count > 1 && tier.Position.HasValue && _tieStrategy == "split";

        if (splitAcrossTies)
        {
            amountPerTeacher = baseAmount / matches.Count;
        }

        foreach (var entry in matches)
        {
            if (!awardedTeacherIds.Add(entry.TeacherId))
            {
                continue;
            }

            payouts.Add(new BonusPayout(
                entry,
                decimal.Round(amountPerTeacher, 2),
                baseAmount,
                BuildTierLabel(tier),
                splitAcrossTies));
        }
    }

    private static string BuildTierLabel(BonusTierSetting tier)
    {
        if (tier.Position.HasValue)
        {
            return $"Rank {tier.Position.Value}";
        }

        if (tier.RangeStart.HasValue && tier.RangeEnd.HasValue)
        {
            return tier.RangeStart.Value == tier.RangeEnd.Value
                ? $"Rank {tier.RangeStart.Value}"
                : $"Ranks {tier.RangeStart.Value}-{tier.RangeEnd.Value}";
        }

        return "Bonus Tier";
    }

    private async Task<BonusConfig> EnsureDefaultConfigAsync(CancellationToken cancellationToken)
    {
        var firstPlace = ParseDecimal(_configuration["FIRST_PLACE_BONUS"], 10m);
        var secondPlace = ParseDecimal(_configuration["SECOND_PLACE_BONUS"], 5m);
        var minimumVotes = ParseInt(_configuration["MINIMUM_VOTES_THRESHOLD"], 10);

        var config = new BonusConfig
        {
            MinimumRatingsThreshold = minimumVotes,
            Tiers = new List<BonusTier>
            {
                new()
                {
                    Position = 1,
                    Amount = firstPlace
                },
                new()
                {
                    Position = 2,
                    Amount = secondPlace
                }
            }
        };

        _dbContext.BonusConfigs.Add(config);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created default bonus configuration using environment fallbacks.");
        return config;
    }

    private static decimal ParseDecimal(string? raw, decimal fallback)
    {
        if (decimal.TryParse(raw, out var parsed) && parsed >= 0)
        {
            return parsed;
        }

        return fallback;
    }

    private static int ParseInt(string? raw, int fallback)
    {
        if (int.TryParse(raw, out var parsed) && parsed > 0)
        {
            return parsed;
        }

        return fallback;
    }

    private string GetCurrencyCode()
        => _configuration["BONUS_CURRENCY"]?.Trim().ToUpperInvariant() ?? "USD";
}
