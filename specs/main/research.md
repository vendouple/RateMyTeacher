# Research Findings: RateMyTeacher

**Date**: 2025-10-23  
**Purpose**: Resolve technical unknowns and establish best practices for implementation

---

## 1. Discord-Style Permission System Implementation in EF Core

### Decision

Implement a **multi-role permission system** using three join tables:

- `RolePermission` (many-to-many: Role ↔ Permission)
- `UserRole` (many-to-many with payload: User ↔ Role + Scope + ScopeId)
- `ClassPermission` (direct permission grants at class level)

Use **hierarchical permission categories** with self-referential foreign key (`ParentCategoryId`).

### Rationale

**Why Multiple Roles per User:**

- Prevents role explosion (e.g., "ChemistryTeacher" + "PhysicsAdmin" + "JrAdminExtras" instead of creating "ChemistryTeacherPhysicsAdminJrAdminExtras" role)
- Mirrors Discord's proven model for complex permission scenarios
- Allows flexible context-specific permissions (Global, Department, Class scopes)

**Why Role Hierarchy with Rank:**

- Enforces protection: Junior roles cannot modify senior role permissions
- Prevents privilege escalation (JrAdmin cannot assign Admin role to themselves)
- Simple integer comparison for hierarchy checks (`RoleA.Rank < RoleB.Rank`)

**Why Hierarchical Permission Categories:**

- Supports both simple (toggle whole category) and granular (select sub-permissions) workflows
- Matches UI pattern of expandable/collapsible permission trees
- Example: "Grades Management" → [View Grades, Edit Grades, Delete Grades, Export Grades]

### Implementation Pattern

```csharp
// Permission evaluation pseudocode
var effectivePermissions = user.UserRoles
    .Where(ur => ur.Scope == Global ||
                 (ur.Scope == Department && ur.ScopeId == contextDeptId) ||
                 (ur.Scope == Class && ur.ScopeId == contextClassId))
    .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission))
    .Union(user.ClassPermissions.Where(cp => cp.ClassId == contextClassId)
           .Select(cp => cp.Permission))
    .Distinct()
    .ToList();

// Most specific scope wins: Class > Department > Global
```

**EF Core Configuration:**

- Many-to-many with payload: Configure `UserRole` as explicit join table with navigation properties
- Eager loading: `.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions)`
- Caching: Cache permission evaluation results per user per context (invalidate on role/permission changes)

### Alternatives Considered

1. **Single Role per User (Rejected)**

   - Reason: Would require creating combinatorial roles (e.g., "TeacherAndDeptAdminAndAIManager")
   - Problem: Role explosion, maintenance nightmare

2. **Flat Permission Structure (Rejected)**

   - Reason: No expandable categories, harder to manage 50+ permissions
   - Problem: Poor UX for admins, no quick "enable all grades permissions"

3. **Claims-Based System (Rejected)**
   - Reason: ASP.NET Core claims are user-session-scoped, not context-scoped (can't vary by class)
   - Problem: Doesn't support class-level overrides naturally

### References

- [EF Core Many-to-Many Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many)
- [Discord Permission System Overview](https://discord.com/developers/docs/topics/permissions)
- [Hierarchical Data in SQL](https://www.red-gate.com/simple-talk/databases/sql-server/t-sql-programming-sql-server/hierarchies-on-steroids-1-convert-an-adjacency-list-to-nested-sets/)

---

## 2. Google Gemini 2.5 Flash Integration

### Decision

Use the **Google.GenerativeAI** NuGet package with the following configuration:

- Model: `gemini-2.0-flash-exp`
- Safety settings: Block only high-probability harmful content
- Rate limiting: Exponential backoff with jitter (3 retries)
- Timeout: 30 seconds per request
- API key: Loaded from `.env` file via DotNetEnv library

### Rationale

**Why Google.GenerativeAI Package:**

- Official .NET SDK from Google
- Supports streaming and non-streaming responses
- Built-in retry logic and error handling
- Strong typing for safety settings and generation config

**Why gemini-2.0-flash-exp:**

- Latest version with improved educational content generation
- Fast response times (<3s typical for 300-word summaries)
- Cost-effective (free tier: 1500 requests/day, 1M tokens/month)
- Good balance of quality and speed for lesson summaries

**Why Exponential Backoff:**

- Handles transient network errors gracefully
- Respects rate limits (429 Too Many Requests)
- Exponential: 1s → 2s → 4s delays between retries
- Jitter: Random 0-500ms added to prevent thundering herd

**Why Safety Settings:**

- Block only HARM_CATEGORY_HIGH or HARM_CATEGORY_MEDIUM
- Allow educational content that might discuss sensitive topics (history, biology)
- Log blocked content for review (compliance audit trail)

### Implementation Pattern

```csharp
public class GeminiService
{
    private readonly GoogleGenerativeAI _client;
    private readonly ILogger<GeminiService> _logger;
    private const int MaxRetries = 3;
    private const int TimeoutSeconds = 30;

    public async Task<string> GenerateLessonSummaryAsync(string lessonNotes)
    {
        var prompt = $"Summarize the following lesson notes in under 300 words, " +
                     $"highlighting key concepts and takeaways:\n\n{lessonNotes}";

        var request = new GenerateContentRequest
        {
            Model = "gemini-2.0-flash-exp",
            Prompt = prompt,
            SafetySettings = new[]
            {
                new SafetySetting
                {
                    Category = HarmCategory.HarassmentOrHate,
                    Threshold = HarmBlockThreshold.BlockMediumAndAbove
                }
            },
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.7f,
                MaxOutputTokens = 500
            }
        };

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
                var response = await _client.GenerateContentAsync(request, cts.Token);
                return response.Text;
            }
            catch (RateLimitException ex)
            {
                if (attempt == MaxRetries - 1) throw;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt))
                            + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
                _logger.LogWarning("Rate limited, retrying in {Delay}ms", delay.TotalMilliseconds);
                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini API error on attempt {Attempt}", attempt + 1);
                if (attempt == MaxRetries - 1) throw;
            }
        }

        throw new InvalidOperationException("Failed after max retries");
    }
}
```

**Error Handling for Users:**

- 401 Unauthorized: "AI service configuration error. Please contact support."
- 429 Rate Limit: "AI service is busy. Please try again in a few moments."
- Timeout: "AI request timed out. Please try again."
- Generic error: "Unable to generate summary. Please try again later."
- Never expose API key errors or stack traces to users

### Alternatives Considered

1. **OpenAI GPT-4 (Rejected)**

   - Reason: More expensive ($0.01-0.03 per 1K tokens vs Gemini free tier)
   - Problem: Requires payment info upfront, no free tier for schools

2. **Azure OpenAI (Rejected)**

   - Reason: Requires Azure subscription and complex setup
   - Problem: Higher complexity, monthly billing

3. **Local LLaMA Model (Rejected)**
   - Reason: Requires GPU infrastructure and model management
   - Problem: Inconsistent quality, high maintenance, no cloud fallback

### References

- [Google Generative AI .NET SDK](https://github.com/google/generative-ai-dotnet)
- [Gemini API Documentation](https://ai.google.dev/docs)
- [Exponential Backoff Best Practices](https://cloud.google.com/iot/docs/how-tos/exponential-backoff)

---

## 3. Attendance Tracking Best Practices

### Decision

Implement **dual-track attendance system**:

**Student Attendance:**

- Default: Present for entire day (inserted automatically at midnight)
- Granularity: Per day (not per class)
- Mid-day updates: Teachers can mark student as Sick/OutOfClass/Absent after initial present
- Audit trail: All status changes logged in `AttendanceLog` table

**Teacher Attendance:**

- Absence workflow: Teacher submits absence request → Admin approves/rejects
- Visibility: Absence not shown to students until substitute assigned
- Reason required: Teachers must provide absence reason (sick, professional development, personal)
- Substitute tracking: Optional SubstituteTeacherId field

### Rationale

**Why Per-Day (Not Per-Class) for Students:**

- Simplifies UI: Single attendance record per student per day
- Matches school admin workflows: "Who was absent today?"
- Reduces database size: ~500 students × 180 days = 90K records (vs 450K if per-class)
- Still allows granularity: Teachers can update mid-day if student leaves early

**Why Separate Teacher Absence Model:**

- Different workflow: Requires approval (students don't approve their own attendance)
- Different privacy: Teacher absence reason visible to admin, not students
- Substitute assignment: Only relevant for teachers
- Historical tracking: "How many days was Teacher X absent this semester?"

**Why Default to Present:**

- Optimistic default reduces teacher workload (mark exceptions, not the norm)
- Matches real-world: Most students are present most days
- Can be overridden at any time during the day

**Why Audit Trail:**

- Accountability: Track who changed attendance and when
- Dispute resolution: "I was marked absent but I was here"
- Compliance: Some schools legally required to maintain attendance history
- Fraud detection: Identify patterns of retroactive changes

### Implementation Pattern

```csharp
// Auto-insert daily attendance at midnight (background job)
public async Task SeedDailyAttendanceAsync(DateTime date)
{
    var activeStudents = await _context.Students
        .Where(s => !s.IsDeleted)
        .ToListAsync();

    var attendanceRecords = activeStudents.Select(s => new Attendance
    {
        StudentId = s.Id,
        Date = date,
        Status = AttendanceStatus.Present,
        UpdatedBy = "System",
        UpdatedAt = DateTime.UtcNow
    });

    await _context.Attendances.AddRangeAsync(attendanceRecords);
    await _context.SaveChangesAsync();
}

// Mid-day update with audit trail
public async Task UpdateAttendanceAsync(int studentId, DateTime date,
    AttendanceStatus newStatus, string reason, int teacherId)
{
    var attendance = await _context.Attendances
        .FirstOrDefaultAsync(a => a.StudentId == studentId && a.Date == date);

    if (attendance == null)
        throw new NotFoundException("Attendance record not found");

    var oldStatus = attendance.Status;
    attendance.Status = newStatus;
    attendance.UpdatedBy = teacherId.ToString();
    attendance.UpdatedAt = DateTime.UtcNow;

    // Create audit log entry
    _context.AttendanceLogs.Add(new AttendanceLog
    {
        AttendanceId = attendance.Id,
        OldStatus = oldStatus,
        NewStatus = newStatus,
        ChangedBy = teacherId,
        ChangedAt = DateTime.UtcNow,
        Reason = reason
    });

    await _context.SaveChangesAsync();
}
```

**Attendance Status Enum:**

- Present: Default, student in school all day
- Absent: Student did not attend at all
- Sick: Student left early or absent due to illness
- OutOfClass: Student participating in competition, field trip, etc.
- Late: Student arrived after attendance taken (future enhancement)

### Alternatives Considered

1. **Per-Class Attendance (Rejected)**

   - Reason: Too granular for school needs, excessive database size
   - Problem: Teachers would need to mark attendance 5-8 times per day per student

2. **No Default Present (Rejected)**

   - Reason: Forces teachers to mark every student every day
   - Problem: High workload, teachers forget, incomplete data

3. **Self-Reported Student Attendance (Rejected)**
   - Reason: Inaccurate, students would mark themselves present when absent
   - Problem: Defeats purpose of attendance tracking

### References

- [School Attendance Best Practices](https://www.attendanceworks.org/)
- [FERPA Compliance for Attendance Records](https://www2.ed.gov/policy/gen/guid/fpco/ferpa/index.html)

---

## 4. Dynamic Bonus Configuration Management

### Decision

Store bonus configuration in **database tables** (`BonusConfig`, `BonusTier`) instead of environment variables or `appsettings.json`.

**Schema:**

```sql
BonusConfig (singleton or per-semester)
- Id (PK)
- MinimumRatingsThreshold (int, e.g., 10)
- CreatedAt (DateTime)
- ModifiedAt (DateTime)

BonusTier (multiple rows per config)
- Id (PK)
- ConfigId (FK to BonusConfig)
- Position (int?, nullable for ranges)
- RangeStart (int?, e.g., 5 for "5th-10th place")
- RangeEnd (int?, e.g., 10 for "5th-10th place")
- Amount (decimal, e.g., 10.00 for $10)
```

**Examples:**

- Single position: `Position=1, RangeStart=NULL, RangeEnd=NULL, Amount=10` → 1st place gets $10
- Range: `Position=NULL, RangeStart=5, RangeEnd=10, Amount=2` → 5th-10th place each get $2

### Rationale

**Why Database (Not Config Files):**

- Hot-reload: Changes apply immediately without restarting app
- Admin UI: Non-technical admins can modify bonuses via web interface
- Audit trail: Track who changed bonuses and when (ModifiedAt, ModifiedBy)
- Validation: Database constraints prevent invalid configs (Amount >= 0)

**Why Support Ranges:**

- Flexibility: Schools can reward top 10 teachers, not just top 3
- Tiered bonuses: 1st: $10, 2nd-3rd: $5, 4th-10th: $2
- Scalability: Large schools can distribute bonuses across more teachers

**Why Singleton Config per Semester:**

- Simplicity: One active configuration at a time
- Historical tracking: Keep old configs for audit ("What was the bonus structure in Fall 2024?")
- Versioning: Each semester can have different bonus structure if needed (future enhancement)

### Implementation Pattern

```csharp
public async Task<List<TeacherRanking>> CalculateBonusesAsync(int semesterId)
{
    var config = await _context.BonusConfigs
        .Include(c => c.BonusTiers)
        .OrderByDescending(c => c.CreatedAt)
        .FirstOrDefaultAsync();

    if (config == null)
        throw new InvalidOperationException("Bonus configuration not found");

    // Get teachers with sufficient ratings
    var rankings = await _context.Ratings
        .Where(r => r.SemesterId == semesterId)
        .GroupBy(r => r.TeacherId)
        .Select(g => new
        {
            TeacherId = g.Key,
            AverageRating = g.Average(r => r.Stars),
            TotalRatings = g.Count()
        })
        .Where(t => t.TotalRatings >= config.MinimumRatingsThreshold)
        .OrderByDescending(t => t.AverageRating)
        .ToListAsync();

    // Assign ranks and bonuses
    var results = new List<TeacherRanking>();
    for (int i = 0; i < rankings.Count; i++)
    {
        var rank = i + 1;
        var teacher = rankings[i];
        var bonus = CalculateBonusForRank(rank, config.BonusTiers);

        results.Add(new TeacherRanking
        {
            TeacherId = teacher.TeacherId,
            SemesterId = semesterId,
            Rank = rank,
            AverageRating = teacher.AverageRating,
            TotalRatings = teacher.TotalRatings,
            BonusAmount = bonus
        });
    }

    return results;
}

private decimal CalculateBonusForRank(int rank, List<BonusTier> tiers)
{
    // Check single position match first
    var exactMatch = tiers.FirstOrDefault(t => t.Position == rank);
    if (exactMatch != null) return exactMatch.Amount;

    // Check range match
    var rangeMatch = tiers.FirstOrDefault(t =>
        t.RangeStart.HasValue && t.RangeEnd.HasValue &&
        rank >= t.RangeStart && rank <= t.RangeEnd);
    if (rangeMatch != null) return rangeMatch.Amount;

    return 0; // No bonus for this rank
}
```

**Admin UI Workflow:**

1. Admin navigates to Settings → Bonus Configuration
2. Sets minimum ratings threshold (default: 10)
3. Adds bonus tiers:
   - "1st place: $10"
   - "2nd-3rd place: $5"
   - "4th-10th place: $2"
4. Saves configuration (writes to BonusConfig and BonusTier tables)
5. Rankings recalculated on next leaderboard view

### Alternatives Considered

1. **Environment Variables (Rejected)**

   - Reason: Requires app restart to apply changes
   - Problem: Non-technical admins can't modify, no audit trail

2. **appsettings.json (Rejected)**

   - Reason: Requires file system access and restart
   - Problem: Can't track who changed settings, no versioning

3. **Hardcoded in Code (Rejected)**
   - Reason: Requires redeployment for any change
   - Problem: Not flexible, developers become bottleneck

### References

- [Database-Driven Configuration Pattern](https://martinfowler.com/bliki/DatabaseConfiguration.html)
- [ASP.NET Core Options Pattern](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)

---

## 5. Soft Delete Pattern in EF Core

### Decision

Implement soft deletes using **global query filters** with an explicit `IsDeleted` boolean column on relevant entities.

**Entities with Soft Delete:**

- User (Students, Teachers, Admins)
- Class
- Department
- Role (except system roles: Admin, Teacher, Student)

**Entities with Hard Delete:**

- Ratings (permanent record for auditing)
- Attendance (legal requirement to retain)
- Logs (AttendanceLog, GradeLog, AIUsageLog)

### Rationale

**Why Soft Delete:**

- Data recovery: Accidental deletions can be undone
- Referential integrity: Avoids cascade delete issues (e.g., deleting teacher doesn't delete all their ratings)
- Audit trail: Historical data remains accessible for reporting
- Compliance: FERPA and educational regulations often require data retention

**Why Global Query Filters:**

- Automatic exclusion: Soft-deleted entities automatically excluded from queries
- Centralized logic: One filter in DbContext applies to all queries
- Easy override: `.IgnoreQueryFilters()` when you need to see deleted records

**Why Explicit Column (Not Shadow Property):**

- Clarity: Developers see `IsDeleted` in model classes, easier to understand
- Migrations: Explicit columns generate clearer migration files
- Debugging: Can inspect `IsDeleted` in debugger and database tools

**Why Hard Delete for Logs:**

- Immutability: Logs should never be deleted, only retained
- Compliance: Audit trails must be permanent
- Storage: Logs are append-only, soft delete adds unnecessary complexity

### Implementation Pattern

```csharp
// In ApplicationDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Global query filter for soft deletes
    modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
    modelBuilder.Entity<Class>().HasQueryFilter(c => !c.IsDeleted);
    modelBuilder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
    modelBuilder.Entity<Role>().HasQueryFilter(r => !r.IsDeleted);

    // Prevent cascade deletes (soft delete instead)
    modelBuilder.Entity<Rating>()
        .HasOne(r => r.Teacher)
        .WithMany(t => t.Ratings)
        .OnDelete(DeleteBehavior.Restrict);

    base.OnModelCreating(modelBuilder);
}

// Soft delete extension method
public static class SoftDeleteExtensions
{
    public static void SoftDelete(this IEntity entity)
    {
        if (entity is ISoftDeletable deletable)
        {
            deletable.IsDeleted = true;
            deletable.DeletedAt = DateTime.UtcNow;
        }
    }
}

// Usage in service
public async Task DeleteTeacherAsync(int teacherId, int deletedBy)
{
    var teacher = await _context.Teachers.FindAsync(teacherId);
    if (teacher == null)
        throw new NotFoundException("Teacher not found");

    teacher.SoftDelete();
    teacher.DeletedBy = deletedBy;
    await _context.SaveChangesAsync();

    _logger.LogInformation("Teacher {TeacherId} soft-deleted by {DeletedBy}",
        teacherId, deletedBy);
}

// View deleted records (admin only)
public async Task<List<User>> GetDeletedUsersAsync()
{
    return await _context.Users
        .IgnoreQueryFilters()
        .Where(u => u.IsDeleted)
        .ToListAsync();
}
```

**ISoftDeletable Interface:**

```csharp
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    int? DeletedBy { get; set; }
}

public class User : ISoftDeletable
{
    public int Id { get; set; }
    public string Email { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}
```

**Cascade Behavior:**

- Soft delete User → Keep all Ratings, Attendance, Logs (historical data)
- Soft delete Class → Keep all Attendance, Assignments (historical data)
- Soft delete Department → Soft delete child Departments (cascade IsDeleted)
- Hard delete system entities (migrations, logs) → Never allow via UI

### Alternatives Considered

1. **Shadow Properties (Rejected)**

   - Reason: Less discoverable, harder to debug
   - Problem: Developers don't see `IsDeleted` in model, easy to forget

2. **Separate Deleted Tables (Rejected)**

   - Reason: Doubles database schema, complex queries to join live + deleted data
   - Problem: Harder to restore, no referential integrity across tables

3. **Temporal Tables (SQL Server) (Rejected)**
   - Reason: SQLite doesn't support temporal tables
   - Problem: Not portable, overkill for soft delete needs

### References

- [EF Core Global Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
- [Soft Delete Pattern](https://www.thereformedprogrammer.net/ef-core-in-depth-soft-deleting-data-with-global-query-filters/)
- [Data Retention in Educational Systems](https://studentprivacy.ed.gov/faq/what-ferpa)

---

## Summary

All technical unknowns have been resolved:

1. ✅ **Permission System**: Multi-role with hierarchical categories, EF Core many-to-many
2. ✅ **Gemini Integration**: Google.GenerativeAI SDK, exponential backoff, safety settings
3. ✅ **Attendance Tracking**: Dual-track (per-day students, approval-based teachers), audit trail
4. ✅ **Bonus Configuration**: Database-driven, range-based tiers, hot-reload
5. ✅ **Soft Deletes**: Global query filters, explicit `IsDeleted` column, selective hard deletes

**Next Phase**: Proceed to Phase 1 (Design & Contracts) to generate `data-model.md`, `contracts/`, and `quickstart.md`.
