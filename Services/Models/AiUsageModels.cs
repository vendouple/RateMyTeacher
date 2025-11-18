using RateMyTeacher.Models.Enums;

namespace RateMyTeacher.Services.Models;

public sealed record AiUsageLogRequest(
    int UserId,
    string Query,
    string Response,
    AiInteractionMode Mode,
    int? ClassId = null,
    DateTime? Timestamp = null);

public sealed record AiUsageLogQuery(
    int? UserId = null,
    int? ClassId = null,
    AiInteractionMode? Mode = null,
    DateTime? From = null,
    DateTime? To = null,
    int Take = 50);

public sealed record AiUsageLogDto(
    int Id,
    string UserName,
    string UserRole,
    int? ClassId,
    AiInteractionMode Mode,
    DateTime Timestamp,
    DateTime? ViewedAt,
    string Query,
    string Response);
