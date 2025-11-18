using System.ComponentModel.DataAnnotations;
using RateMyTeacher.Services;

namespace RateMyTeacher.Models.Teachers;

public class TeacherListViewModel
{
    public IReadOnlyList<TeacherSummaryDto> Teachers { get; init; } = Array.Empty<TeacherSummaryDto>();
}

public class TeacherDetailViewModel
{
    public required TeacherDetailDto Teacher { get; init; }
    public StudentRatingDto? ExistingRating { get; init; }
    public RatingFormModel RatingForm { get; init; } = new();
    public bool CanSubmitRating { get; init; }
    public string? StatusMessage { get; set; }
}

public class RatingFormModel
{
    [Required]
    public int TeacherId { get; set; }

    [Range(1, 5)]
    public int Stars { get; set; } = 5;

    [MaxLength(500)]
    public string? Comment { get; set; }
}
