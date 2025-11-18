namespace RateMyTeacher.Services;

public interface ITeacherService
{
    Task<IReadOnlyList<TeacherSummaryDto>> GetSummariesAsync(CancellationToken cancellationToken = default);
    Task<TeacherDetailDto?> GetDetailAsync(int teacherId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int teacherId, CancellationToken cancellationToken = default);
}

public sealed record TeacherSummaryDto(
    int Id,
    string FullName,
    string? Department,
    string? ProfileImageUrl,
    double AverageRating,
    int TotalRatings);

public sealed record TeacherDetailDto(
    TeacherSummaryDto Summary,
    string? Bio,
    bool AllowAiTools,
    IReadOnlyList<RatingPreviewDto> RecentRatings);

public sealed record RatingPreviewDto(
    int RatingId,
    string StudentName,
    int Stars,
    string? Comment,
    DateTime CreatedAt);
