using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RateMyTeacher.Data;
using RateMyTeacher.Models;

namespace RateMyTeacher.Services;

public class RatingService : IRatingService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<RatingService> _logger;

    public RatingService(ApplicationDbContext dbContext, ILogger<RatingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<StudentRatingDto?> GetStudentRatingAsync(
        int studentUserId,
        int teacherId,
        CancellationToken cancellationToken = default)
    {
        var student = await _dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == studentUserId, cancellationToken);

        if (student is null)
        {
            return null;
        }

        var semester = await GetActiveSemesterAsync(cancellationToken);

        var rating = await _dbContext.Ratings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.StudentId == student.Id &&
                     r.TeacherId == teacherId &&
                     r.SemesterId == semester.Id,
                cancellationToken);

        return rating is null ? null : ToDto(rating);
    }

    public async Task<RatingOperationResult> SubmitAsync(
        int studentUserId,
        RatingSubmission submission,
        CancellationToken cancellationToken = default)
    {
        if (submission.Stars is < 1 or > 5)
        {
            return RatingOperationResult.Failure("Rating must be between 1 and 5 stars.");
        }

        var sanitizedComment = submission.SanitizedComment;
        if (sanitizedComment is { Length: > 500 })
        {
            return RatingOperationResult.Failure("Comments must be 500 characters or fewer.");
        }

        var student = await _dbContext.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == studentUserId, cancellationToken);

        if (student is null)
        {
            return RatingOperationResult.Failure("Student profile not found.");
        }

        if (!student.IsActive)
        {
            return RatingOperationResult.Failure("Inactive students cannot submit ratings.");
        }

        var teacher = await _dbContext.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == submission.TeacherId, cancellationToken);

        if (teacher is null)
        {
            return RatingOperationResult.Failure("Teacher not found.");
        }

        if (!teacher.IsActive)
        {
            return RatingOperationResult.Failure("Teacher is not accepting ratings.");
        }

        if (teacher.UserId == student.UserId)
        {
            return RatingOperationResult.Failure("You cannot rate yourself.");
        }

        var semester = await GetActiveSemesterAsync(cancellationToken);

        var existingRating = await _dbContext.Ratings.FirstOrDefaultAsync(
            r => r.StudentId == student.Id &&
                 r.TeacherId == submission.TeacherId &&
                 r.SemesterId == semester.Id,
            cancellationToken);

        if (existingRating is null)
        {
            var newRating = new Rating
            {
                StudentId = student.Id,
                TeacherId = submission.TeacherId,
                SemesterId = semester.Id,
                Stars = submission.Stars,
                Comment = sanitizedComment,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Ratings.Add(newRating);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Student {StudentId} created rating {RatingId} for teacher {TeacherId} in semester {SemesterId}.",
                student.Id,
                newRating.Id,
                submission.TeacherId,
                semester.Id);

            return RatingOperationResult.Success(ToDto(newRating), true);
        }

        existingRating.Stars = submission.Stars;
        existingRating.Comment = sanitizedComment;
        existingRating.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Student {StudentId} updated rating {RatingId} for teacher {TeacherId} in semester {SemesterId}.",
            student.Id,
            existingRating.Id,
            submission.TeacherId,
            semester.Id);

        return RatingOperationResult.Success(ToDto(existingRating), false);
    }

    public async Task<LeaderboardResult> GetLeaderboardAsync(
        int? semesterId,
        int minimumRatingsThreshold,
        CancellationToken cancellationToken = default)
    {
        if (minimumRatingsThreshold < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumRatingsThreshold), "Minimum ratings threshold must be at least 1.");
        }

        var semester = await ResolveSemesterAsync(semesterId, cancellationToken);

        var aggregates = await _dbContext.Ratings
            .AsNoTracking()
            .Where(r => r.SemesterId == semester.Id)
            .GroupBy(r => new
            {
                r.TeacherId,
                r.Teacher.User.FirstName,
                r.Teacher.User.LastName,
                r.Teacher.Department
            })
            .Select(group => new
            {
                group.Key.TeacherId,
                FullName = (group.Key.FirstName ?? string.Empty) + " " + (group.Key.LastName ?? string.Empty),
                group.Key.Department,
                AverageRating = group.Average(r => (double)r.Stars),
                TotalRatings = group.Count()
            })
            .Where(x => x.TotalRatings >= minimumRatingsThreshold)
            .ToListAsync(cancellationToken);

        if (!aggregates.Any())
        {
            return new LeaderboardResult(ToSemesterSummary(semester), Array.Empty<TeacherLeaderboardEntry>());
        }

        var ordered = aggregates
            .OrderByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.TotalRatings)
            .ThenBy(x => x.FullName)
            .ToList();

        var entries = new List<TeacherLeaderboardEntry>(ordered.Count);
        double? previousAverage = null;
        int previousRank = 0;
        int position = 0;

        foreach (var item in ordered)
        {
            position++;
            var isTie = previousAverage.HasValue && Math.Abs(item.AverageRating - previousAverage.Value) < 0.0001;
            var rank = isTie ? previousRank : position;

            entries.Add(new TeacherLeaderboardEntry(
                item.TeacherId,
                item.FullName.Trim(),
                item.Department,
                Math.Round(item.AverageRating, 3),
                item.TotalRatings,
                rank,
                isTie));

            previousAverage = item.AverageRating;
            previousRank = rank;
        }

        return new LeaderboardResult(ToSemesterSummary(semester), entries);
    }

    private async Task<Semester> GetActiveSemesterAsync(CancellationToken cancellationToken)
    {
        var semester = await _dbContext.Semesters
            .AsNoTracking()
                .OrderByDescending(s => s.IsCurrent)
                .ThenByDescending(s => s.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

        if (semester is null)
        {
            throw new InvalidOperationException("No semesters configured. Please seed at least one semester before using the rating system.");
        }

        return semester;
    }

    private async Task<Semester> ResolveSemesterAsync(int? semesterId, CancellationToken cancellationToken)
    {
        if (semesterId is null)
        {
            return await GetActiveSemesterAsync(cancellationToken);
        }

        var semester = await _dbContext.Semesters
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == semesterId.Value, cancellationToken);

        if (semester is null)
        {
            throw new InvalidOperationException($"Semester with id {semesterId.Value} was not found.");
        }

        return semester;
    }

    private static SemesterSummaryDto ToSemesterSummary(Semester semester) => new(
        semester.Id,
        semester.Name,
        semester.AcademicYear,
        semester.IsCurrent);

    private static StudentRatingDto ToDto(Rating rating) => new(
        rating.Id,
        rating.SemesterId,
        rating.Stars,
        rating.Comment,
        rating.CreatedAt,
        rating.UpdatedAt);
}
