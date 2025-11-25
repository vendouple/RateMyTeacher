using RateMyTeacher.Models.Lessons;

namespace RateMyTeacher.Services;

public interface ILessonSummaryService
{
    Task<LessonSummaryDisplayModel> GenerateAsync(int teacherId, int requestedByUserId, string lessonNotes, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LessonSummaryDisplayModel>> GetHistoryAsync(int teacherId, int take, CancellationToken cancellationToken = default);
}
