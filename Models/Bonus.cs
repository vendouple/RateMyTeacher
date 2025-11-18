namespace RateMyTeacher.Models;

public class Bonus
{
    public int Id { get; set; }
    public Guid BatchId { get; set; } = Guid.NewGuid();
    public int TeacherId { get; set; }
    public int SemesterId { get; set; }
    public int Rank { get; set; }
    public decimal AwardedAmount { get; set; }
    public decimal BaseAmount { get; set; }
    public string TierLabel { get; set; } = string.Empty;
    public bool SplitAcrossTies { get; set; }
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    public int? AwardedById { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public Semester Semester { get; set; } = null!;
    public User? AwardedBy { get; set; }
}
