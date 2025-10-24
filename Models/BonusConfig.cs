namespace RateMyTeacher.Models;

public class BonusConfig
{
    public int Id { get; set; }
    public int MinimumRatingsThreshold { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public int? ModifiedById { get; set; }
    public User? ModifiedBy { get; set; }

    public ICollection<BonusTier> Tiers { get; set; } = new List<BonusTier>();
}
