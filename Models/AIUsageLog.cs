using RateMyTeacher.Models.Enums;

namespace RateMyTeacher.Models;

public class AIUsageLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ClassId { get; set; }
    public string Query { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public AiInteractionMode Mode { get; set; } = AiInteractionMode.Explain;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DateTime? ViewedAt { get; set; }

    public User User { get; set; } = null!;
}
