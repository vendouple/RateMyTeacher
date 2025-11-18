using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RateMyTeacher.Models;
using RateMyTeacher.Models.Enums;

namespace RateMyTeacher.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<TeacherRanking> TeacherRankings => Set<TeacherRanking>();
    public DbSet<BonusConfig> BonusConfigs => Set<BonusConfig>();
    public DbSet<BonusTier> BonusTiers => Set<BonusTier>();
    public DbSet<Bonus> Bonuses => Set<Bonus>();
    public DbSet<AISummary> AISummaries => Set<AISummary>();
    public DbSet<AIUsageLog> AIUsageLogs => Set<AIUsageLog>();
    public DbSet<SentimentAnalysis> SentimentAnalyses => Set<SentimentAnalysis>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<AIControlSetting> AIControlSettings => Set<AIControlSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            v => v.ToDateTime(TimeOnly.MinValue),
            v => DateOnly.FromDateTime(v));
        var dateOnlyComparer = new ValueComparer<DateOnly>(
            (l, r) => l == r,
            v => v.GetHashCode(),
            v => v);

        var aiModeConverter = new EnumToStringConverter<AiInteractionMode>();
        var aiScopeConverter = new EnumToStringConverter<AiControlScope>();

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.IsDeleted);
            entity.Property(u => u.Role).HasMaxLength(32);
            entity.Property(u => u.Username).HasMaxLength(64);
            entity.Property(u => u.TimeZone).HasMaxLength(64);
            entity.Property(u => u.Locale).HasMaxLength(8);
            entity.HasQueryFilter(u => !u.IsDeleted);
            entity.HasOne(u => u.DeletedBy)
                .WithMany()
                .HasForeignKey(u => u.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasOne(t => t.User)
                .WithOne(u => u.TeacherProfile)
                .HasForeignKey<Teacher>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(t => t.Department).HasMaxLength(100);
            entity.Property(t => t.Bio).HasMaxLength(1000);
            entity.Property(t => t.ProfileImageUrl).HasMaxLength(500);
            entity.HasQueryFilter(t => !t.User.IsDeleted);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasOne(s => s.User)
                .WithOne(u => u.StudentProfile)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(s => s.StudentNumber).HasMaxLength(50);
            entity.Property(s => s.ParentContact).HasMaxLength(15);
            entity.HasQueryFilter(s => !s.User.IsDeleted);
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(64);
            entity.HasIndex(s => s.Name).IsUnique();
            entity.Property(s => s.StartDate)
                .HasConversion(dateOnlyConverter, dateOnlyComparer);
            entity.Property(s => s.EndDate)
                .HasConversion(dateOnlyConverter, dateOnlyComparer);
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasIndex(r => new { r.StudentId, r.TeacherId, r.SemesterId })
                .IsUnique();
            entity.ToTable(t => t.HasCheckConstraint("CK_Ratings_Stars", "Stars BETWEEN 1 AND 5"));
            entity.Property(r => r.Comment).HasMaxLength(500);
            entity.HasOne(r => r.Student)
                .WithMany(s => s.Ratings)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Teacher)
                .WithMany(t => t.Ratings)
                .HasForeignKey(r => r.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Semester)
                .WithMany(s => s.Ratings)
                .HasForeignKey(r => r.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(r => !r.Student.User.IsDeleted && !r.Teacher.User.IsDeleted);
        });

        modelBuilder.Entity<SentimentAnalysis>(entity =>
        {
            entity.HasIndex(sa => sa.RatingId).IsUnique();
            entity.Property(sa => sa.Sentiment).HasMaxLength(32);
            entity.HasOne(sa => sa.Rating)
                .WithOne(r => r.SentimentAnalysis)
                .HasForeignKey<SentimentAnalysis>(sa => sa.RatingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(sa =>
                !sa.Rating.Student.User.IsDeleted &&
                !sa.Rating.Teacher.User.IsDeleted);
        });

        modelBuilder.Entity<TeacherRanking>(entity =>
        {
            entity.HasIndex(tr => new { tr.TeacherId, tr.SemesterId }).IsUnique();
            entity.Property(tr => tr.AverageRating).HasPrecision(4, 2);
            entity.Property(tr => tr.BonusAmount).HasPrecision(10, 2);
            entity.HasOne(tr => tr.Teacher)
                .WithMany(t => t.Rankings)
                .HasForeignKey(tr => tr.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(tr => !tr.Teacher.User.IsDeleted);
        });

        modelBuilder.Entity<BonusConfig>(entity =>
        {
            entity.Property(b => b.MinimumRatingsThreshold).HasDefaultValue(20);
            entity.HasMany(b => b.Tiers)
                .WithOne(t => t.Config)
                .HasForeignKey(t => t.ConfigId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BonusTier>(entity =>
        {
            entity.Property(t => t.Amount).HasPrecision(10, 2);
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_BonusTier_PositionOrRange",
                "(Position IS NOT NULL) OR (RangeStart IS NOT NULL AND RangeEnd IS NOT NULL)"));
        });

        modelBuilder.Entity<Bonus>(entity =>
        {
            entity.Property(b => b.AwardedAmount).HasPrecision(10, 2);
            entity.Property(b => b.BaseAmount).HasPrecision(10, 2);
            entity.Property(b => b.TierLabel).HasMaxLength(128);
            entity.HasIndex(b => b.BatchId);
            entity.HasIndex(b => new { b.SemesterId, b.Rank });
            entity.HasOne(b => b.Teacher)
                .WithMany()
                .HasForeignKey(b => b.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(b => b.Semester)
                .WithMany()
                .HasForeignKey(b => b.SemesterId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(b => b.AwardedBy)
                .WithMany()
                .HasForeignKey(b => b.AwardedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AISummary>(entity =>
        {
            entity.Property(a => a.Model).HasMaxLength(64);
            entity.HasOne(a => a.Teacher)
                .WithMany(t => t.Summaries)
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(a => !a.Teacher.User.IsDeleted);
        });

        modelBuilder.Entity<AIUsageLog>(entity =>
        {
            entity.Property(l => l.Mode)
                .HasConversion(aiModeConverter)
                .HasMaxLength(32);
            entity.HasIndex(l => l.Timestamp);
            entity.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(l => !l.User.IsDeleted);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasIndex(s => s.Key).IsUnique();
            entity.Property(s => s.Key).HasMaxLength(128);
        });

        modelBuilder.Entity<AIControlSetting>(entity =>
        {
            entity.HasIndex(s => new { s.Scope, s.ScopeId }).IsUnique();
            entity.Property(s => s.Scope)
                .HasConversion(aiScopeConverter)
                .HasMaxLength(32);
            entity.Property(s => s.Mode)
                .HasConversion(aiModeConverter)
                .HasMaxLength(32);
            entity.Property(s => s.Notes).HasMaxLength(250);
            entity.HasOne(s => s.ModifiedBy)
                .WithMany()
                .HasForeignKey(s => s.ModifiedById)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
