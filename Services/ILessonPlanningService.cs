using RateMyTeacher.Models.Lessons;

namespace RateMyTeacher.Services;

public interface ILessonPlanningService
{
    Task<LessonPlanDisplayModel> GenerateAsync(LessonPlanRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LessonPlanDisplayModel>> GetHistoryAsync(int teacherId, int take, CancellationToken cancellationToken = default);
}
