using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;
using RateMyTeacher.Models;

namespace RateMyTeacher.Services;

public class TeacherService : ITeacherService
{
    private readonly ApplicationDbContext _dbContext;

    public TeacherService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TeacherSummaryDto>> GetSummariesAsync(CancellationToken cancellationToken = default)
    {
        var semester = await GetActiveSemesterAsync(cancellationToken);

        var teachers = await _dbContext.Teachers
            .AsNoTracking()
            .Select(t => new
            {
                t.Id,
                FullName = t.User.FirstName + " " + t.User.LastName,
                t.Department,
                t.ProfileImageUrl,
                Average = t.Ratings
                    .Where(r => r.SemesterId == semester.Id)
                    .Select(r => (double?)r.Stars)
                    .Average(),
                Count = t.Ratings.Count(r => r.SemesterId == semester.Id)
            })
            .ToListAsync(cancellationToken);

        return teachers
            .Select(t => new TeacherSummaryDto(
                t.Id,
                t.FullName,
                t.Department,
                t.ProfileImageUrl,
                Math.Round(t.Average ?? 0, 2),
                t.Count))
            .OrderByDescending(t => t.AverageRating)
            .ThenBy(t => t.FullName)
            .ToList();
    }

    public async Task<TeacherDetailDto?> GetDetailAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        var semester = await GetActiveSemesterAsync(cancellationToken);

        var teacher = await _dbContext.Teachers
            .AsNoTracking()
            .Where(t => t.Id == teacherId)
            .Select(t => new
            {
                t.Id,
                FullName = t.User.FirstName + " " + t.User.LastName,
                t.Department,
                t.ProfileImageUrl,
                t.Bio,
                t.AllowAiTools,
                Average = t.Ratings
                    .Where(r => r.SemesterId == semester.Id)
                    .Select(r => (double?)r.Stars)
                    .Average(),
                Count = t.Ratings.Count(r => r.SemesterId == semester.Id)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (teacher is null)
        {
            return null;
        }

        var recentRatings = await _dbContext.Ratings
            .AsNoTracking()
            .Where(r => r.TeacherId == teacherId && r.SemesterId == semester.Id)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .ThenByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => new RatingPreviewDto(
                r.Id,
                r.Student.User.FirstName + " " + r.Student.User.LastName,
                r.Stars,
                r.Comment,
                r.UpdatedAt ?? r.CreatedAt))
            .ToListAsync(cancellationToken);

        var summary = new TeacherSummaryDto(
            teacher.Id,
            teacher.FullName,
            teacher.Department,
            teacher.ProfileImageUrl,
            Math.Round(teacher.Average ?? 0, 2),
            teacher.Count);

        return new TeacherDetailDto(summary, teacher.Bio, teacher.AllowAiTools, recentRatings);
    }

    public Task<bool> ExistsAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Teachers.AnyAsync(t => t.Id == teacherId, cancellationToken);
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
            throw new InvalidOperationException("No semesters configured. Please seed at least one semester before using teacher listings.");
        }

        return semester;
    }
}
