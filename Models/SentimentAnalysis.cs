namespace RateMyTeacher.Models;

public class SentimentAnalysis
{
    public int Id { get; set; }
    public int RatingId { get; set; }
    public string Sentiment { get; set; } = string.Empty;
    public int Confidence { get; set; }
    public string? KeywordsJson { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public string Model { get; set; } = string.Empty;

    public Rating Rating { get; set; } = null!;
}
