using RateMyTeacher.Models.Enums;
using RateMyTeacher.Services.Models;

namespace RateMyTeacher.Services;

public interface IAIUsageService
{
    Task<int> LogAsync(AiUsageLogRequest request, CancellationToken cancellationToken = default);
    Task MarkViewedAsync(int logId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiUsageLogDto>> GetLogsAsync(AiUsageLogQuery query, CancellationToken cancellationToken = default);
}
