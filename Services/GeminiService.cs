using System.Threading;
using Microsoft.Extensions.Logging;
using RateMyTeacher.Services.Models;

namespace RateMyTeacher.Services;

/// <summary>
/// Placeholder Gemini service that currently focuses on AI usage auditing.
/// Implementation for actual Gemini calls will arrive in later phases.
/// </summary>
public class GeminiService
{
    private readonly IAIUsageService _aiUsageService;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(IAIUsageService aiUsageService, ILogger<GeminiService> logger)
    {
        _aiUsageService = aiUsageService;
        _logger = logger;
    }

    protected Task<int> RecordUsageAsync(AiUsageLogRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Recording AI usage entry for user {UserId} in mode {Mode}.", request.UserId, request.Mode);
        return _aiUsageService.LogAsync(request, cancellationToken);
    }

    protected Task MarkUsageViewedAsync(int logId, CancellationToken cancellationToken = default) =>
        _aiUsageService.MarkViewedAsync(logId, cancellationToken);
}
