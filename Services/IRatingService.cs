using RateMyTeacher.Models;

namespace RateMyTeacher.Services;

public interface IRatingService
{
    Task<StudentRatingDto?> GetStudentRatingAsync(
        int studentUserId,
        int teacherId,
        CancellationToken cancellationToken = default);

    Task<RatingOperationResult> SubmitAsync(
        int studentUserId,
        RatingSubmission submission,
        CancellationToken cancellationToken = default);

    Task<LeaderboardResult> GetLeaderboardAsync(
        int? semesterId,
        int minimumRatingsThreshold,
        CancellationToken cancellationToken = default);
}

public sealed record RatingSubmission(int TeacherId, int Stars, string? Comment)
{
    public string? SanitizedComment => Comment?.Trim();
}

public sealed record StudentRatingDto(int RatingId, int SemesterId, int Stars, string? Comment, DateTime CreatedAt, DateTime? UpdatedAt);

public sealed record RatingOperationResult(bool Succeeded, bool CreatedNew, string? ErrorMessage, StudentRatingDto? Rating)
{
    public static RatingOperationResult Failure(string message)
        => new(false, false, message, null);

    public static RatingOperationResult Success(StudentRatingDto rating, bool createdNew)
        => new(true, createdNew, null, rating);
}

public sealed record LeaderboardResult(SemesterSummaryDto Semester, IReadOnlyList<TeacherLeaderboardEntry> Entries);

public sealed record SemesterSummaryDto(int Id, string Name, string AcademicYear, bool IsCurrent);

public sealed record TeacherLeaderboardEntry(
    int TeacherId,
    string TeacherName,
    string? Department,
    double AverageRating,
    int TotalRatings,
    int Rank,
    bool IsTie);
