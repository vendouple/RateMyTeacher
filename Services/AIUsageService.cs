using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RateMyTeacher.Data;
using RateMyTeacher.Models;
using RateMyTeacher.Services.Models;

namespace RateMyTeacher.Services;

public class AIUsageService : IAIUsageService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AIUsageService> _logger;

    public AIUsageService(ApplicationDbContext dbContext, ILogger<AIUsageService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> LogAsync(AiUsageLogRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var sanitizedQuery = request.Query?.Trim() ?? string.Empty;
        var sanitizedResponse = request.Response?.Trim() ?? string.Empty;

        var entity = new AIUsageLog
        {
            UserId = request.UserId,
            ClassId = request.ClassId,
            Query = sanitizedQuery,
            Response = sanitizedResponse,
            Mode = request.Mode,
            Timestamp = request.Timestamp ?? DateTime.UtcNow
        };

        _dbContext.AIUsageLogs.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task MarkViewedAsync(int logId, CancellationToken cancellationToken = default)
    {
        var entry = await _dbContext.AIUsageLogs
            .FirstOrDefaultAsync(l => l.Id == logId, cancellationToken);

        if (entry is null)
        {
            _logger.LogWarning("AI usage log {LogId} not found when marking viewed.", logId);
            return;
        }

        if (entry.ViewedAt is not null)
        {
            return;
        }

        entry.ViewedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiUsageLogDto>> GetLogsAsync(
        AiUsageLogQuery query,
        CancellationToken cancellationToken = default)
    {
        query ??= new AiUsageLogQuery();
        var take = Math.Clamp(query.Take, 1, 200);

        var logsQuery = _dbContext.AIUsageLogs
            .AsNoTracking()
            .Include(l => l.User)
            .OrderByDescending(l => l.Timestamp)
            .AsQueryable();

        if (query.UserId is int userId)
        {
            logsQuery = logsQuery.Where(l => l.UserId == userId);
        }

        if (query.ClassId is int classId)
        {
            logsQuery = logsQuery.Where(l => l.ClassId == classId);
        }

        if (query.Mode is not null)
        {
            logsQuery = logsQuery.Where(l => l.Mode == query.Mode);
        }

        if (query.From is DateTime from)
        {
            logsQuery = logsQuery.Where(l => l.Timestamp >= from);
        }

        if (query.To is DateTime to)
        {
            logsQuery = logsQuery.Where(l => l.Timestamp <= to);
        }

        return await logsQuery
            .Take(take)
            .Select(l => new AiUsageLogDto(
                l.Id,
                l.User.FullName,
                l.User.Role,
                l.ClassId,
                l.Mode,
                l.Timestamp,
                l.ViewedAt,
                l.Query,
                l.Response))
            .ToListAsync(cancellationToken);
    }
}
