namespace RateMyTeacher.Models;

public class BonusTier
{
    public int Id { get; set; }
    public int ConfigId { get; set; }
    public int? Position { get; set; }
    public int? RangeStart { get; set; }
    public int? RangeEnd { get; set; }
    public decimal Amount { get; set; }

    public BonusConfig Config { get; set; } = null!;
}
