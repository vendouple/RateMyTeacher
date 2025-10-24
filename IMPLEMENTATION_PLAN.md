# RateMyTeacher - Implementation Plan

## Project Configuration

### Technology Stack Overview

**Framework**: ASP.NET Core 9.0 MVC  
**Database**: SQLite (Embedded, No Installation Required)  
**AI Model**: Google Gemini 2.5 Flash  
**Target**: Demo/Prototype Application  
**Deployment**: Standalone executable with embedded database

---

## 1. Project Setup & Configuration

### 1.1 Create New ASP.NET Core MVC Project

```powershell
# Create new ASP.NET Core 9 MVC project
dotnet new mvc -n RateMyTeacher -f net9.0

# Navigate to project directory
cd RateMyTeacher

# Add required NuGet packages
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Google.Generative.AI --version 1.0.0
dotnet add package DotNetEnv --version 3.1.1
```

### 1.2 Environment Configuration (.env)

Create `.env` file in project root:

```env
# Google Gemini API Configuration
GEMINI_API_KEY=your_api_key_here
GEMINI_MODEL=gemini-2.5-flash

# Application Settings
ASPNETCORE_ENVIRONMENT=Development
DATABASE_PATH=Data/ratemyteacher.db

# AI Configuration
AI_MAX_TOKENS=8192
AI_TEMPERATURE=0.7
AI_TIMEOUT_SECONDS=30
```

**Note**: Bonus amounts and minimum rating thresholds are no longer configured here. They are managed through the admin settings interface and stored in the database for dynamic updates without restart.

### 1.3 .gitignore Updates

Add to `.gitignore`:

```
# Environment files
.env
.env.*
!.env.example

# SQLite Database
*.db
*.db-shm
*.db-wal
Data/
```

### 1.4 Create .env.example

```env
# Copy this file to .env and fill in your actual values
GEMINI_API_KEY=your_gemini_api_key_from_ai_google_dev
GEMINI_MODEL=gemini-2.5-flash
ASPNETCORE_ENVIRONMENT=Development
DATABASE_PATH=Data/ratemyteacher.db
AI_MAX_TOKENS=8192
AI_TEMPERATURE=0.7
AI_TIMEOUT_SECONDS=30
```

**Note**: Bonus configuration and minimum vote thresholds are now managed through the admin settings UI (stored in database), not environment variables. This allows dynamic configuration without application restart.

---

## 2. Database Architecture (SQLite)

### 2.1 Database Models

Create `Models/` folder structure:

```
Models/
├── User.cs
├── Teacher.cs
├── TeacherAbsence.cs      # NEW: Teacher attendance/absence tracking
├── Student.cs
├── Subject.cs
├── Class.cs
├── Department.cs          # NEW: Optional departmental organization
├── Schedule.cs
├── Lesson.cs
├── LessonLog.cs
├── Attendance.cs          # Student attendance (per day)
├── Grade.cs
├── Assignment.cs          # Homework/assignments
├── AssignmentSubmission.cs # NEW: Student file uploads
├── ReadingAssignment.cs   # NEW: Pre-class reading tracking
├── StudentNote.cs         # NEW: Student-shared notes
├── ExtraClass.cs          # NEW: After-school meetings/sessions
├── Rating.cs
├── Feedback.cs
├── Announcement.cs
├── Resource.cs
├── Badge.cs
├── Behavior.cs
├── SystemSetting.cs       # For app-wide settings
├── BonusTier.cs           # For configurable bonus rules
├── Permission.cs          # NEW: Permission definitions
├── PermissionCategory.cs  # NEW: Permission grouping
├── Role.cs                # NEW: Role templates
├── RolePermission.cs      # NEW: Role-Permission mapping
├── UserRole.cs            # NEW: User-Role assignments
└── ClassPermission.cs     # NEW: Class-level permission overrides
```

#### Example: Teacher.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models
{
    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(200)]
        public string Email { get; set; }

        [MaxLength(500)]
        public string Bio { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; }

        public int TotalRatings { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalBonusEarned { get; set; }

        public DateTime HireDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Class> Classes { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
        public ICollection<Badge> Badges { get; set; }
    }
}
```

#### Example: Rating.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        [Range(1, 5)]
        public int OverallRating { get; set; }

        [Range(1, 5)]
        public int ClarityRating { get; set; }

        [Range(1, 5)]
        public int EngagementRating { get; set; }

        [Range(1, 5)]
        public int FairnessRating { get; set; }

        [Range(1, 5)]
        public int HelpfulnessRating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }

        [Required]
        [MaxLength(50)]
        public string Semester { get; set; } // e.g., "2025-Fall"

        public bool IsVerified { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

#### NEW: SystemSetting.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace RateMyTeacher.Models
{
    public class SystemSetting
    {
        [Key]
        public int SettingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } // e.g., "MinimumRatingsThreshold"

        [Required]
        [MaxLength(500)]
        public string Value { get; set; } // Stored as string, parsed as needed

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string DataType { get; set; } // "int", "decimal", "string", "bool"

        public bool IsEditable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
```

#### NEW: BonusTier.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models
{
    public class BonusTier
    {
        [Key]
        public int BonusTierId { get; set; }

        [Required]
        public int RankStart { get; set; } // e.g., 1 for 1st place, 5 for 5th place

        [Required]
        public int RankEnd { get; set; } // e.g., 1 for single position, 10 for range (5th-10th)

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal BonusAmount { get; set; } // e.g., 10.00, 5.00, 2.50

        [MaxLength(200)]
        public string Description { get; set; } // e.g., "First Place", "5th-10th Place"

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } // For UI sorting

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
```

#### NEW: TeacherAbsence.cs (Phase 2)

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models
{
    public class TeacherAbsence
    {
        [Key]
        public int AbsenceId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }

        [Required]
        public DateTime AbsenceDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } // "Pending", "Approved", "Rejected"

        public int? ApprovedByUserId { get; set; } // Admin/Headmaster who approved

        [ForeignKey("ApprovedByUserId")]
        public User ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string AdminNotes { get; set; }

        public bool SubstituteAssigned { get; set; } = false;

        public int? SubstituteTeacherId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

#### NEW: AssignmentSubmission.cs (Phase 3)

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models
{
    public class AssignmentSubmission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [ForeignKey("AssignmentId")]
        public Assignment Assignment { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        [MaxLength(500)]
        public string FilePath { get; set; } // Uploaded file path

        [MaxLength(200)]
        public string FileName { get; set; }

        [MaxLength(1000)]
        public string Comments { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } // "Submitted", "Graded", "Late"

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Score { get; set; }

        [MaxLength(1000)]
        public string TeacherFeedback { get; set; }

        public DateTime? GradedAt { get; set; }
    }
}
```

#### NEW: ReadingAssignment.cs (Phase 3)

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models
{
    public class ReadingAssignment
    {
        [Key]
        public int ReadingAssignmentId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public Class Class { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } // e.g., "Module B pages 25-41"

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? NextClassDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<ReadingProgress> ReadingProgresses { get; set; }
    }

    public class ReadingProgress
    {
        [Key]
        public int ProgressId { get; set; }

        [Required]
        public int ReadingAssignmentId { get; set; }

        [ForeignKey("ReadingAssignmentId")]
        public ReadingAssignment ReadingAssignment { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        public bool IsCompleted { get; set; } = false;

        public bool IsUnderstood { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; }
    }
}
```

#### NEW: StudentNote.cs (Phase 3)

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models
{
    public class StudentNote
    {
        [Key]
        public int NoteId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; } // Markdown or rich text

        [MaxLength(500)]
        public string FilePath { get; set; } // Optional uploaded file

        public int Views { get; set; } = 0;

        public int Likes { get; set; } = 0;

        public int Points { get; set; } = 0; // Gamification points

        [MaxLength(500)]
        public string Tags { get; set; } // Comma-separated tags

        public bool IsPublic { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
```

#### NEW: ExtraClass.cs (Phase 3)

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models
{
    public class ExtraClass
    {
        [Key]
        public int ExtraClassId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public DateTime ScheduledDateTime { get; set; }

        public int DurationMinutes { get; set; } = 60;

        [Required]
        [MaxLength(50)]
        public string Platform { get; set; } // "Google Meet", "Zoom", "Microsoft Teams"

        [Required]
        [MaxLength(500)]
        public string MeetingLink { get; set; }

        [MaxLength(100)]
        public string MeetingId { get; set; }

        [MaxLength(100)]
        public string MeetingPassword { get; set; }

        public int MaxStudents { get; set; } = 30;

        public int EnrolledCount { get; set; } = 0;

        [MaxLength(50)]
        public string Status { get; set; } = "Scheduled"; // "Scheduled", "In Progress", "Completed", "Cancelled"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<ExtraClassEnrollment> Enrollments { get; set; }
    }

    public class ExtraClassEnrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        public int ExtraClassId { get; set; }

        [ForeignKey("ExtraClassId")]
        public ExtraClass ExtraClass { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public bool Attended { get; set; } = false;
    }
}
```

#### NEW: Permission System Models (Phase 2)

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models
{
    // Department model (optional feature)
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } // e.g., "Science Department", "Physics Department"

        [MaxLength(1000)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Class> Classes { get; set; }
    }

    // Permission Category (e.g., "Grades Management", "Attendance")
    public class PermissionCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // e.g., "Grades", "Attendance", "AI Companion"

        [MaxLength(500)]
        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Permission> Permissions { get; set; }
    }

    // Permission (granular actions like "View", "Edit", "Delete")
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public PermissionCategory Category { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // e.g., "View", "Edit", "Delete", "Create"

        [Required]
        [MaxLength(200)]
        public string Code { get; set; } // e.g., "grades.view", "grades.edit", "attendance.mark"

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Scope { get; set; } // "Global", "Department", "Class"

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }
    }

    // Role Template (e.g., "Admin", "Teacher", "Chemistry Viewer")
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Scope { get; set; } // "Global", "Department", "Class"

        public int? DepartmentId { get; set; } // Null for global roles

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        public bool IsDefault { get; set; } = false; // True for Admin, Teacher, Student

        public bool IsSystemRole { get; set; } = false; // Cannot be deleted

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<RolePermission> RolePermissions { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }

    // Role-Permission mapping (many-to-many)
    public class RolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        [Required]
        public int PermissionId { get; set; }

        [ForeignKey("PermissionId")]
        public Permission Permission { get; set; }

        public bool IsGranted { get; set; } = true; // Allow explicit deny

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // User-Role assignment (with optional class/department context)
    public class UserRole
    {
        [Key]
        public int UserRoleId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        // Optional: Scope to specific class or department
        public int? ClassId { get; set; }

        [ForeignKey("ClassId")]
        public Class Class { get; set; }

        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public int AssignedByUserId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // Class-level permission overrides
    public class ClassPermission
    {
        [Key]
        public int ClassPermissionId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public Class Class { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public int PermissionId { get; set; }

        [ForeignKey("PermissionId")]
        public Permission Permission { get; set; }

        public bool IsGranted { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }
    }
}
```

### 2.2 DbContext Configuration

Create `Data/ApplicationDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Models;

namespace RateMyTeacher.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonLog> LessonLogs { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<Behavior> Behaviors { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<BonusTier> BonusTiers { get; set; }

        // Phase 2/3 additions
        public DbSet<TeacherAbsence> TeacherAbsences { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<ReadingAssignment> ReadingAssignments { get; set; }
        public DbSet<ReadingProgress> ReadingProgresses { get; set; }
        public DbSet<StudentNote> StudentNotes { get; set; }
        public DbSet<ExtraClass> ExtraClasses { get; set; }
        public DbSet<ExtraClassEnrollment> ExtraClassEnrollments { get; set; }

        // Permission System (Phase 2)
        public DbSet<Department> Departments { get; set; }
        public DbSet<PermissionCategory> PermissionCategories { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ClassPermission> ClassPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User table
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // SystemSetting - unique key
            modelBuilder.Entity<SystemSetting>()
                .HasIndex(s => s.Key)
                .IsUnique();

            // BonusTier - validate rank ranges
            modelBuilder.Entity<BonusTier>()
                .HasIndex(b => new { b.RankStart, b.RankEnd });

            // Rating table - One rating per student per teacher per semester
            modelBuilder.Entity<Rating>()
                .HasIndex(r => new { r.StudentId, r.TeacherId, r.Semester })
                .IsUnique();

            // Attendance composite index
            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.StudentId, a.LessonId })
                .IsUnique();

            // Grade composite index
            modelBuilder.Entity<Grade>()
                .HasIndex(g => new { g.StudentId, g.SubjectId, g.Semester });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "admin@ratemyteacher.com",
                    Username = "admin",
                    PasswordHash = "hashed_password", // Use proper hashing
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed subjects
            modelBuilder.Entity<Subject>().HasData(
                new Subject { SubjectId = 1, Name = "Mathematics", Code = "MATH", CreatedAt = DateTime.UtcNow },
                new Subject { SubjectId = 2, Name = "English", Code = "ENG", CreatedAt = DateTime.UtcNow },
                new Subject { SubjectId = 3, Name = "Science", Code = "SCI", CreatedAt = DateTime.UtcNow },
                new Subject { SubjectId = 4, Name = "History", Code = "HIST", CreatedAt = DateTime.UtcNow },
                new Subject { SubjectId = 5, Name = "Physical Education", Code = "PE", CreatedAt = DateTime.UtcNow }
            );

            // Seed badge types
            modelBuilder.Entity<Badge>().HasData(
                new Badge { BadgeId = 1, Name = "Engagement Master", Description = "High student participation rates", IconUrl = "/images/badges/engagement.svg" },
                new Badge { BadgeId = 2, Name = "Punctuality Champion", Description = "Always on time", IconUrl = "/images/badges/punctuality.svg" },
                new Badge { BadgeId = 3, Name = "Innovation Award", Description = "Creative teaching methods", IconUrl = "/images/badges/innovation.svg" },
                new Badge { BadgeId = 4, Name = "Student Favorite", Description = "Consistently high ratings", IconUrl = "/images/badges/favorite.svg" }
            );

            // Seed system settings
            modelBuilder.Entity<SystemSetting>().HasData(
                new SystemSetting
                {
                    SettingId = 1,
                    Key = "MinimumRatingsThreshold",
                    Value = "10",
                    Description = "Minimum number of ratings required for a teacher to appear in rankings",
                    DataType = "int",
                    CreatedAt = DateTime.UtcNow
                },
                new SystemSetting
                {
                    SettingId = 2,
                    Key = "SemesterDurationMonths",
                    Value = "6",
                    Description = "Duration of each semester in months",
                    DataType = "int",
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed default bonus tiers
            modelBuilder.Entity<BonusTier>().HasData(
                new BonusTier
                {
                    BonusTierId = 1,
                    RankStart = 1,
                    RankEnd = 1,
                    BonusAmount = 10.00m,
                    Description = "First Place",
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new BonusTier
                {
                    BonusTierId = 2,
                    RankStart = 2,
                    RankEnd = 2,
                    BonusAmount = 5.00m,
                    Description = "Second Place",
                    DisplayOrder = 2,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
```

### 2.3 Program.cs Configuration

Update `Program.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;
using RateMyTeacher.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
Env.Load();

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure SQLite Database
var databasePath = Environment.GetEnvironmentVariable("DATABASE_PATH") ?? "Data/ratemyteacher.db";
var connectionString = $"Data Source={databasePath}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Register Gemini AI Service
builder.Services.AddSingleton<IGeminiService, GeminiService>();

// Register other services
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();        // NEW

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

## 3. Gemini AI Integration

### 3.1 Gemini Service Interface

Create `Services/IGeminiService.cs`:

```csharp
namespace RateMyTeacher.Services
{
    public interface IGeminiService
    {
        // Teacher tools
        Task<string> GenerateLessonSummary(string lessonContent, string topics);
        Task<string> GenerateQuizQuestions(string lessonContent, int numberOfQuestions);
        Task<string> SuggestLessonPlan(string subject, string previousTopics, string curriculum);
        Task<string> AnalyzeFeedbackSentiment(string feedback);
        Task<string> GenerateWeeklyReport(string teacherName, Dictionary<string, object> metrics);
        Task<List<string>> SuggestImprovementAreas(double rating, List<string> feedbackComments);

        // Student AI Companion (Phase 3)
        Task<string> ExplainConcept(string concept, string context, string difficultyLevel);
        Task<string> GuideStudentToSolution(string problem, string studentAttempt);
        Task<string> ShowAnswerOnly(string question, string context);
        Task<string> SummarizeReadingMaterial(string content, int maxWords);
        Task<bool> CheckReadingComprehension(string material, List<string> studentAnswers);
        Task<string> SuggestStudyPlan(string subject, DateTime dueDate, List<string> topics);
        Task<string> AnswerStudentQuestion(string question, string context);
    }
}
```

### 3.2 Gemini Service Implementation

Create `Services/GeminiService.cs`:

```csharp
using Google.Generative.AI;
using Google.Generative.AI.Models;

namespace RateMyTeacher.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly string _apiKey;
        private readonly string _modelName;
        private readonly int _maxTokens;
        private readonly float _temperature;
        private readonly int _timeoutSeconds;

        public GeminiService()
        {
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                ?? throw new InvalidOperationException("GEMINI_API_KEY not found in environment variables");
            _modelName = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.5-flash";
            _maxTokens = int.Parse(Environment.GetEnvironmentVariable("AI_MAX_TOKENS") ?? "8192");
            _temperature = float.Parse(Environment.GetEnvironmentVariable("AI_TEMPERATURE") ?? "0.7");
            _timeoutSeconds = int.Parse(Environment.GetEnvironmentVariable("AI_TIMEOUT_SECONDS") ?? "30");
        }

        private async Task<string> CallGeminiAPI(string prompt)
        {
            try
            {
                var client = new GenerativeAI(_apiKey);
                var model = client.GenerativeModel(_modelName);

                var generationConfig = new GenerationConfig
                {
                    Temperature = _temperature,
                    MaxOutputTokens = _maxTokens
                };

                var response = await model.GenerateContentAsync(prompt, generationConfig);
                return response.Text;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Gemini API Error: {ex.Message}");
                throw new Exception("Failed to generate AI response", ex);
            }
        }

        public async Task<string> GenerateLessonSummary(string lessonContent, string topics)
        {
            var prompt = $@"You are an educational assistant. Generate a concise lesson summary for students.

Lesson Content:
{lessonContent}

Topics Covered:
{topics}

Please create a clear, structured summary including:
1. Main topics covered
2. Key concepts
3. Important takeaways
4. Study tips

Keep it under 300 words.";

            return await CallGeminiAPI(prompt);
        }

        public async Task<string> GenerateQuizQuestions(string lessonContent, int numberOfQuestions)
        {
            var prompt = $@"You are a quiz generator for teachers. Create {numberOfQuestions} multiple-choice questions based on this lesson content:

{lessonContent}

For each question provide:
- The question
- 4 answer options (A, B, C, D)
- The correct answer
- A brief explanation

Format as JSON array with structure:
{{
  ""questions"": [
    {{
      ""question"": ""..."",
      ""options"": [""A. ..."", ""B. ..."", ""C. ..."", ""D. ...""],
      ""correctAnswer"": ""A"",
      ""explanation"": ""...""
    }}
  ]
}}";

            return await CallGeminiAPI(prompt);
        }

        public async Task<string> AnswerStudentQuestion(string question, string context)
        {
            var prompt = $@"You are a helpful teaching assistant. Answer this student's question based on the lesson material.

Student Question:
{question}

Lesson Material:
{context}

Provide a clear, educational answer that:
1. Directly answers the question
2. References the lesson material
3. Gives examples if helpful
4. Encourages further learning

Keep the answer concise (under 200 words).";

            return await CallGeminiAPI(prompt);
        }

        public async Task<string> SuggestLessonPlan(string subject, string previousTopics, string curriculum)
        {
            var prompt = $@"You are an educational curriculum planner. Suggest the next lesson plan.

Subject: {subject}
Previous Topics Covered: {previousTopics}
Curriculum Standards: {curriculum}

Create a lesson plan suggestion including:
1. Next topic to cover
2. Learning objectives
3. Suggested activities (2-3)
4. Required materials
5. Estimated duration
6. Assessment methods

Format as structured text.";

            return await CallGeminiAPI(prompt);
        }

        public async Task<string> AnalyzeFeedbackSentiment(string feedback)
        {
            var prompt = $@"Analyze the sentiment of this student feedback and categorize it.

Feedback:
{feedback}

Provide:
1. Overall sentiment (Positive/Neutral/Negative)
2. Key themes mentioned
3. Actionable insights for the teacher

Format as JSON:
{{
  ""sentiment"": ""Positive|Neutral|Negative"",
  ""themes"": [""theme1"", ""theme2""],
  ""insights"": ""...""
}}";

            return await CallGeminiAPI(prompt);
        }

        public async Task<string> GenerateWeeklyReport(string teacherName, Dictionary<string, object> metrics)
        {
            var metricsText = string.Join("\n", metrics.Select(m => $"{m.Key}: {m.Value}"));

            var prompt = $@"Generate a professional weekly teaching report for {teacherName}.

Metrics:
{metricsText}

Create a report including:
1. Performance Summary
2. Key Achievements
3. Areas for Improvement
4. Student Engagement Analysis
5. Recommendations for Next Week

Format as professional markdown.";

            return await CallGeminiAPI(prompt);
        }

        public async Task<List<string>> SuggestImprovementAreas(double rating, List<string> feedbackComments)
        {
            var feedbackText = string.Join("\n- ", feedbackComments);

            var prompt = $@"You are an educational consultant. Analyze this teacher's performance data.

Average Rating: {rating}/5
Student Feedback:
- {feedbackText}

Suggest 3-5 specific, actionable improvement areas for this teacher.
Format as JSON array:
{{
  ""suggestions"": [""suggestion1"", ""suggestion2"", ...]
}}";

            var response = await CallGeminiAPI(prompt);

            // Parse JSON response (simplified - add proper JSON parsing)
            try
            {
                // Use System.Text.Json or Newtonsoft.Json to parse
                var suggestions = new List<string>();
                // Parse and extract suggestions
                return suggestions;
            }
            catch
            {
                return new List<string> { response };
            }
        }
    }
}
```

---

## 4. Settings Management Service

### 4.1 Settings Service Interface

Create `Services/ISettingsService.cs`:

```csharp
namespace RateMyTeacher.Services
{
    public interface ISettingsService
    {
        // System Settings
        Task<string> GetSettingValueAsync(string key);
        Task<T> GetSettingValueAsync<T>(string key);
        Task<bool> UpdateSettingAsync(string key, string value);
        Task<List<SystemSetting>> GetAllSettingsAsync();

        // Bonus Tiers
        Task<List<BonusTier>> GetActiveBonusTiersAsync();
        Task<BonusTier> GetBonusTierByIdAsync(int bonusTierId);
        Task<bool> CreateBonusTierAsync(BonusTier bonusTier);
        Task<bool> UpdateBonusTierAsync(BonusTier bonusTier);
        Task<bool> DeleteBonusTierAsync(int bonusTierId);
        Task<decimal> CalculateBonusForRank(int rank);
        Task<Dictionary<int, decimal>> GetBonusDistributionAsync();

        // Convenience methods
        Task<int> GetMinimumRatingsThresholdAsync();
        Task<bool> UpdateMinimumRatingsThresholdAsync(int threshold);
    }
}
```

### 4.2 Settings Service Implementation

Create `Services/SettingsService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;
using RateMyTeacher.Models;

namespace RateMyTeacher.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SettingsService> _logger;

        public SettingsService(ApplicationDbContext context, ILogger<SettingsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // System Settings Methods
        public async Task<string> GetSettingValueAsync(string key)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            return setting?.Value ?? string.Empty;
        }

        public async Task<T> GetSettingValueAsync<T>(string key)
        {
            var value = await GetSettingValueAsync(key);

            if (string.IsNullOrEmpty(value))
                return default(T);

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error converting setting {key} to type {typeof(T).Name}");
                return default(T);
            }
        }

        public async Task<bool> UpdateSettingAsync(string key, string value)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == key);

                if (setting == null)
                {
                    _logger.LogWarning($"Setting {key} not found");
                    return false;
                }

                if (!setting.IsEditable)
                {
                    _logger.LogWarning($"Setting {key} is not editable");
                    return false;
                }

                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating setting {key}");
                return false;
            }
        }

        public async Task<List<SystemSetting>> GetAllSettingsAsync()
        {
            return await _context.SystemSettings
                .OrderBy(s => s.Key)
                .ToListAsync();
        }

        // Bonus Tier Methods
        public async Task<List<BonusTier>> GetActiveBonusTiersAsync()
        {
            return await _context.BonusTiers
                .Where(b => b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ThenBy(b => b.RankStart)
                .ToListAsync();
        }

        public async Task<BonusTier> GetBonusTierByIdAsync(int bonusTierId)
        {
            return await _context.BonusTiers
                .FirstOrDefaultAsync(b => b.BonusTierId == bonusTierId);
        }

        public async Task<bool> CreateBonusTierAsync(BonusTier bonusTier)
        {
            try
            {
                // Validate rank range
                if (bonusTier.RankStart > bonusTier.RankEnd)
                {
                    _logger.LogWarning("RankStart cannot be greater than RankEnd");
                    return false;
                }

                if (bonusTier.RankStart < 1)
                {
                    _logger.LogWarning("Rank must be at least 1");
                    return false;
                }

                bonusTier.CreatedAt = DateTime.UtcNow;
                _context.BonusTiers.Add(bonusTier);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bonus tier");
                return false;
            }
        }

        public async Task<bool> UpdateBonusTierAsync(BonusTier bonusTier)
        {
            try
            {
                var existing = await _context.BonusTiers
                    .FirstOrDefaultAsync(b => b.BonusTierId == bonusTier.BonusTierId);

                if (existing == null)
                    return false;

                // Validate rank range
                if (bonusTier.RankStart > bonusTier.RankEnd)
                {
                    _logger.LogWarning("RankStart cannot be greater than RankEnd");
                    return false;
                }

                existing.RankStart = bonusTier.RankStart;
                existing.RankEnd = bonusTier.RankEnd;
                existing.BonusAmount = bonusTier.BonusAmount;
                existing.Description = bonusTier.Description;
                existing.IsActive = bonusTier.IsActive;
                existing.DisplayOrder = bonusTier.DisplayOrder;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bonus tier");
                return false;
            }
        }

        public async Task<bool> DeleteBonusTierAsync(int bonusTierId)
        {
            try
            {
                var bonusTier = await _context.BonusTiers
                    .FirstOrDefaultAsync(b => b.BonusTierId == bonusTierId);

                if (bonusTier == null)
                    return false;

                _context.BonusTiers.Remove(bonusTier);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bonus tier");
                return false;
            }
        }

        public async Task<decimal> CalculateBonusForRank(int rank)
        {
            var bonusTiers = await GetActiveBonusTiersAsync();

            foreach (var tier in bonusTiers)
            {
                if (rank >= tier.RankStart && rank <= tier.RankEnd)
                {
                    return tier.BonusAmount;
                }
            }

            return 0m;
        }

        public async Task<Dictionary<int, decimal>> GetBonusDistributionAsync()
        {
            var distribution = new Dictionary<int, decimal>();
            var bonusTiers = await GetActiveBonusTiersAsync();

            foreach (var tier in bonusTiers)
            {
                for (int rank = tier.RankStart; rank <= tier.RankEnd; rank++)
                {
                    if (!distribution.ContainsKey(rank))
                    {
                        distribution[rank] = tier.BonusAmount;
                    }
                }
            }

            return distribution;
        }

        // Convenience Methods
        public async Task<int> GetMinimumRatingsThresholdAsync()
        {
            return await GetSettingValueAsync<int>("MinimumRatingsThreshold");
        }

        public async Task<bool> UpdateMinimumRatingsThresholdAsync(int threshold)
        {
            return await UpdateSettingAsync("MinimumRatingsThreshold", threshold.ToString());
        }
    }
}
```

---

## 5. Key Controllers

### 5.1 Admin Settings Controller

Create `Controllers/AdminController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using RateMyTeacher.Services;
using RateMyTeacher.Models;

namespace RateMyTeacher.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISettingsService _settingsService;
        private readonly IRatingService _ratingService;

        public AdminController(ISettingsService settingsService, IRatingService ratingService)
        {
            _settingsService = settingsService;
            _ratingService = ratingService;
        }

        // GET: /Admin/Settings
        public async Task<IActionResult> Settings()
        {
            var settings = await _settingsService.GetAllSettingsAsync();
            var bonusTiers = await _settingsService.GetActiveBonusTiersAsync();

            ViewBag.BonusTiers = bonusTiers;
            return View(settings);
        }

        // POST: /Admin/UpdateSetting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSetting(string key, string value)
        {
            var result = await _settingsService.UpdateSettingAsync(key, value);

            if (result)
            {
                TempData["Success"] = "Setting updated successfully";
            }
            else
            {
                TempData["Error"] = "Failed to update setting";
            }

            return RedirectToAction("Settings");
        }

        // GET: /Admin/BonusTiers
        public async Task<IActionResult> BonusTiers()
        {
            var bonusTiers = await _settingsService.GetActiveBonusTiersAsync();
            var distribution = await _settingsService.GetBonusDistributionAsync();

            ViewBag.Distribution = distribution;
            return View(bonusTiers);
        }

        // GET: /Admin/CreateBonusTier
        public IActionResult CreateBonusTier()
        {
            return View();
        }

        // POST: /Admin/CreateBonusTier
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBonusTier(BonusTier bonusTier)
        {
            if (!ModelState.IsValid)
                return View(bonusTier);

            var result = await _settingsService.CreateBonusTierAsync(bonusTier);

            if (result)
            {
                TempData["Success"] = "Bonus tier created successfully";
                return RedirectToAction("BonusTiers");
            }

            ModelState.AddModelError("", "Failed to create bonus tier");
            return View(bonusTier);
        }

        // GET: /Admin/EditBonusTier/5
        public async Task<IActionResult> EditBonusTier(int id)
        {
            var bonusTier = await _settingsService.GetBonusTierByIdAsync(id);

            if (bonusTier == null)
                return NotFound();

            return View(bonusTier);
        }

        // POST: /Admin/EditBonusTier/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBonusTier(BonusTier bonusTier)
        {
            if (!ModelState.IsValid)
                return View(bonusTier);

            var result = await _settingsService.UpdateBonusTierAsync(bonusTier);

            if (result)
            {
                TempData["Success"] = "Bonus tier updated successfully";
                return RedirectToAction("BonusTiers");
            }

            ModelState.AddModelError("", "Failed to update bonus tier");
            return View(bonusTier);
        }

        // POST: /Admin/DeleteBonusTier/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBonusTier(int id)
        {
            var result = await _settingsService.DeleteBonusTierAsync(id);

            if (result)
            {
                TempData["Success"] = "Bonus tier deleted successfully";
            }
            else
            {
                TempData["Error"] = "Failed to delete bonus tier";
            }

            return RedirectToAction("BonusTiers");
        }

        // GET: /Admin/Leaderboard
        public async Task<IActionResult> Leaderboard(string semester)
        {
            var minRatings = await _settingsService.GetMinimumRatingsThresholdAsync();
            var rankings = await _ratingService.GetTeacherRankingsAsync(semester, minRatings);
            var bonusDistribution = await _settingsService.GetBonusDistributionAsync();

            ViewBag.Semester = semester;
            ViewBag.MinimumRatings = minRatings;
            ViewBag.BonusDistribution = bonusDistribution;

            return View(rankings);
        }
    }
}
```

### 5.2 Teachers Controller

Create `Controllers/TeachersController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using RateMyTeacher.Services;
using RateMyTeacher.Models;

namespace RateMyTeacher.Controllers
{
    public class TeachersController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IRatingService _ratingService;
        private readonly IGeminiService _geminiService;

        public TeachersController(
            ITeacherService teacherService,
            IRatingService ratingService,
            IGeminiService geminiService)
        {
            _teacherService = teacherService;
            _ratingService = ratingService;
            _geminiService = geminiService;
        }

        // GET: /Teachers
        public async Task<IActionResult> Index()
        {
            var teachers = await _teacherService.GetAllTeachersAsync();
            return View(teachers);
        }

        // GET: /Teachers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            if (teacher == null)
                return NotFound();

            var ratings = await _ratingService.GetTeacherRatingsAsync(id);
            ViewBag.Ratings = ratings;

            return View(teacher);
        }

        // GET: /Teachers/Leaderboard
        public async Task<IActionResult> Leaderboard(string semester)
        {
            var rankings = await _ratingService.GetTeacherRankingsAsync(semester);
            ViewBag.Semester = semester;
            return View(rankings);
        }

        // POST: /Teachers/Rate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(Rating rating)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _ratingService.SubmitRatingAsync(rating);

            if (result.Success)
            {
                // Analyze sentiment using Gemini
                if (!string.IsNullOrEmpty(rating.Comment))
                {
                    var sentiment = await _geminiService.AnalyzeFeedbackSentiment(rating.Comment);
                    // Store sentiment analysis
                }

                return RedirectToAction("Details", new { id = rating.TeacherId });
            }

            return View("Error");
        }
    }
}
```

### 5.3 Lessons Controller

Create `Controllers/LessonsController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using RateMyTeacher.Services;
using RateMyTeacher.Models;

namespace RateMyTeacher.Controllers
{
    public class LessonsController : Controller
    {
        private readonly ILessonService _lessonService;
        private readonly IGeminiService _geminiService;

        public LessonsController(ILessonService lessonService, IGeminiService geminiService)
        {
            _lessonService = lessonService;
            _geminiService = geminiService;
        }

        // GET: /Lessons/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Lessons/GenerateSummary
        [HttpPost]
        public async Task<IActionResult> GenerateSummary([FromBody] LessonSummaryRequest request)
        {
            try
            {
                var summary = await _geminiService.GenerateLessonSummary(
                    request.LessonContent,
                    request.Topics);

                return Json(new { success = true, summary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // POST: /Lessons/GenerateQuiz
        [HttpPost]
        public async Task<IActionResult> GenerateQuiz([FromBody] QuizRequest request)
        {
            try
            {
                var quiz = await _geminiService.GenerateQuizQuestions(
                    request.LessonContent,
                    request.NumberOfQuestions);

                return Json(new { success = true, quiz });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // POST: /Lessons/AskAI
        [HttpPost]
        public async Task<IActionResult> AskAI([FromBody] AIQuestionRequest request)
        {
            try
            {
                var answer = await _geminiService.AnswerStudentQuestion(
                    request.Question,
                    request.Context);

                return Json(new { success = true, answer });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
```

---

## 6. Frontend Implementation

### 6.1 Neuromorphic CSS Setup

Create `wwwroot/css/neuromorphic.css`:

```css
:root {
  /* Light Mode Colors */
  --bg-primary: #e4e8ec;
  --bg-secondary: #d8dde3;
  --surface: #e4e8ec;
  --text-primary: #2c3e50;
  --text-secondary: #5a6c7d;
  --shadow-light: rgba(255, 255, 255, 0.8);
  --shadow-dark: rgba(0, 0, 0, 0.15);
  --accent: #3498db;
  --accent-hover: #2980b9;
}

[data-theme="dark"] {
  /* Dark Mode Colors */
  --bg-primary: #1a1d23;
  --bg-secondary: #24282f;
  --surface: #2a2f38;
  --text-primary: #e4e8ec;
  --text-secondary: #a0a8b4;
  --shadow-light: rgba(255, 255, 255, 0.05);
  --shadow-dark: rgba(0, 0, 0, 0.4);
  --accent: #5dade2;
  --accent-hover: #3498db;
}

body {
  background-color: var(--bg-primary);
  color: var(--text-primary);
  font-family: "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
  transition: background-color 0.3s ease, color 0.3s ease;
}

/* Neuromorphic Button */
.neuro-btn {
  background: var(--surface);
  border: none;
  border-radius: 12px;
  padding: 12px 24px;
  color: var(--text-primary);
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: -5px -5px 10px var(--shadow-light), 5px 5px 10px var(--shadow-dark);
}

.neuro-btn:hover {
  box-shadow: -7px -7px 14px var(--shadow-light), 7px 7px 14px var(--shadow-dark);
}

.neuro-btn:active {
  box-shadow: inset -3px -3px 8px var(--shadow-light), inset 3px 3px 8px var(--shadow-dark);
}

/* Neuromorphic Card */
.neuro-card {
  background: var(--surface);
  border-radius: 16px;
  padding: 24px;
  margin: 16px 0;
  box-shadow: -8px -8px 16px var(--shadow-light), 8px 8px 16px var(--shadow-dark);
  transition: all 0.3s ease;
}

.neuro-card:hover {
  box-shadow: -12px -12px 20px var(--shadow-light), 12px 12px 20px var(--shadow-dark);
}

/* Neuromorphic Input */
.neuro-input {
  background: var(--surface);
  border: none;
  border-radius: 10px;
  padding: 12px 16px;
  color: var(--text-primary);
  box-shadow: inset -3px -3px 8px var(--shadow-light), inset 3px 3px 8px var(--shadow-dark);
  transition: all 0.3s ease;
}

.neuro-input:focus {
  outline: none;
  box-shadow: inset -3px -3px 8px var(--shadow-light), inset 3px 3px 8px var(--shadow-dark),
    0 0 0 3px rgba(52, 152, 219, 0.3);
}

/* Rating Stars */
.rating-stars {
  display: flex;
  gap: 8px;
  font-size: 24px;
}

.star {
  color: #ddd;
  cursor: pointer;
  transition: color 0.2s ease;
}

.star.active,
.star:hover {
  color: #f39c12;
}

/* Theme Toggle */
.theme-toggle {
  position: fixed;
  top: 20px;
  right: 20px;
  background: var(--surface);
  border: none;
  border-radius: 50%;
  width: 50px;
  height: 50px;
  cursor: pointer;
  box-shadow: -5px -5px 10px var(--shadow-light), 5px 5px 10px var(--shadow-dark);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  transition: all 0.3s ease;
}

.theme-toggle:hover {
  transform: rotate(180deg);
}
```

### 6.2 Theme Toggle JavaScript

Create `wwwroot/js/theme.js`:

```javascript
// Theme Management
const themeToggle = document.getElementById("themeToggle");
const html = document.documentElement;

// Load saved theme or detect system preference
const savedTheme = localStorage.getItem("theme");
const systemPrefersDark = window.matchMedia(
  "(prefers-color-scheme: dark)"
).matches;

if (savedTheme) {
  html.setAttribute("data-theme", savedTheme);
} else if (systemPrefersDark) {
  html.setAttribute("data-theme", "dark");
}

// Toggle theme
themeToggle?.addEventListener("click", () => {
  const currentTheme = html.getAttribute("data-theme");
  const newTheme = currentTheme === "dark" ? "light" : "dark";

  html.setAttribute("data-theme", newTheme);
  localStorage.setItem("theme", newTheme);

  // Update icon
  themeToggle.textContent = newTheme === "dark" ? "☀️" : "🌙";
});

// Set initial icon
if (themeToggle) {
  const currentTheme = html.getAttribute("data-theme");
  themeToggle.textContent = currentTheme === "dark" ? "☀️" : "🌙";
}
```

### 6.3 AI Chat Interface

Create `wwwroot/js/ai-chat.js`:

```javascript
// AI Question Answering
async function askAI(question, context) {
  const response = await fetch("/Lessons/AskAI", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      question: question,
      context: context,
    }),
  });

  const data = await response.json();
  return data;
}

// Initialize AI Chat
document.addEventListener("DOMContentLoaded", () => {
  const chatForm = document.getElementById("aiChatForm");
  const chatInput = document.getElementById("chatInput");
  const chatMessages = document.getElementById("chatMessages");

  chatForm?.addEventListener("submit", async (e) => {
    e.preventDefault();

    const question = chatInput.value.trim();
    if (!question) return;

    // Add user message
    addMessage(question, "user");
    chatInput.value = "";

    // Show loading
    const loadingId = addMessage("Thinking...", "ai", true);

    try {
      // Get lesson context (from page data)
      const context =
        document.getElementById("lessonContext")?.textContent || "";

      const result = await askAI(question, context);

      // Remove loading, add response
      removeMessage(loadingId);
      addMessage(result.answer, "ai");
    } catch (error) {
      removeMessage(loadingId);
      addMessage("Sorry, I encountered an error. Please try again.", "ai");
    }
  });
});

function addMessage(text, sender, isLoading = false) {
  const chatMessages = document.getElementById("chatMessages");
  const messageId = `msg-${Date.now()}`;

  const messageDiv = document.createElement("div");
  messageDiv.id = messageId;
  messageDiv.className = `chat-message ${sender} ${isLoading ? "loading" : ""}`;
  messageDiv.textContent = text;

  chatMessages.appendChild(messageDiv);
  chatMessages.scrollTop = chatMessages.scrollHeight;

  return messageId;
}

function removeMessage(messageId) {
  const message = document.getElementById(messageId);
  message?.remove();
}
```

---

## 7. Localization Setup

### 7.1 Resource Files Structure

Create resource files:

```
Resources/
├── Controllers/
│   ├── HomeController.en.resx
│   ├── TeachersController.en.resx
│   └── LessonsController.en.resx
└── Views/
    ├── Shared.en.resx
    └── Home.en.resx
```

### 7.2 Update Program.cs for Localization

```csharp
// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "id", "zh" };
    options.SetDefaultCulture("en")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// After app is built, use localization middleware
app.UseRequestLocalization();
```

---

## 8. Development Workflow

### 8.1 Initial Setup Commands

```powershell
# Clone/create project
git clone https://github.com/vendouple/RateMyTeacher.git
cd RateMyTeacher

# Copy environment file
Copy-Item .env.example .env

# Edit .env and add your Gemini API key
# Get API key from: https://aistudio.google.com/apikey

# Restore packages
dotnet restore

# Create database and apply migrations
dotnet ef database update

# Run the application
dotnet run
```

### 8.2 Database Migration Commands

```powershell
# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Add new migration (after model changes)
dotnet ef migrations add AddNewFeature

# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

---

## 9. Testing Strategy

### 9.1 Unit Tests Setup

```powershell
# Create test project
dotnet new xunit -n RateMyTeacher.Tests

# Add reference to main project
cd RateMyTeacher.Tests
dotnet add reference ../RateMyTeacher/RateMyTeacher.csproj

# Add testing packages
dotnet add package Moq
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package FluentAssertions
```

### 9.2 Example Test

```csharp
using Xunit;
using Moq;
using RateMyTeacher.Services;
using FluentAssertions;

public class GeminiServiceTests
{
    [Fact]
    public async Task GenerateLessonSummary_ShouldReturnSummary()
    {
        // Arrange
        var service = new GeminiService();
        var lessonContent = "Photosynthesis is the process...";
        var topics = "Photosynthesis, Plant Biology";

        // Act
        var result = await service.GenerateLessonSummary(lessonContent, topics);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("photosynthesis");
    }
}
```

---

## 10. Deployment Configuration

### 10.1 Publish for Production

```powershell
# Publish as self-contained executable
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

# Output will be in: bin/Release/net9.0/win-x64/publish/
```

### 10.2 Docker Support (Optional)

Create `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RateMyTeacher/RateMyTeacher.csproj", "RateMyTeacher/"]
RUN dotnet restore "RateMyTeacher/RateMyTeacher.csproj"
COPY . .
WORKDIR "/src/RateMyTeacher"
RUN dotnet build "RateMyTeacher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RateMyTeacher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RateMyTeacher.dll"]
```

---

## 11. Performance Optimization

### 11.1 Caching Strategy

```csharp
// Add memory caching in Program.cs
builder.Services.AddMemoryCache();

// Use caching in services
public class TeacherService : ITeacherService
{
    private readonly IMemoryCache _cache;

    public async Task<List<Teacher>> GetTopTeachersAsync()
    {
        return await _cache.GetOrCreateAsync("top-teachers", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            return await _dbContext.Teachers
                .OrderByDescending(t => t.AverageRating)
                .Take(10)
                .ToListAsync();
        });
    }
}
```

### 11.2 Query Optimization

```csharp
// Use AsNoTracking for read-only queries
var teachers = await _dbContext.Teachers
    .AsNoTracking()
    .Include(t => t.Ratings)
    .ToListAsync();

// Use Select to fetch only needed fields
var teacherNames = await _dbContext.Teachers
    .Select(t => new { t.TeacherId, t.FirstName, t.LastName })
    .ToListAsync();
```

---

## 12. Security Checklist

- [ ] Add API key validation for Gemini service
- [ ] Implement user authentication (ASP.NET Identity)
- [ ] Add CSRF protection to all forms
- [ ] Sanitize all user inputs
- [ ] Implement rate limiting for AI endpoints
- [ ] Add SQL injection protection (using EF Core)
- [ ] Secure .env file (never commit to git)
- [ ] Add HTTPS enforcement
- [ ] Implement proper password hashing
- [ ] Add logging for security events

---

## 13. Phase 1 Implementation Checklist (MVP)

### Week 1-2: Foundation

- [ ] Create ASP.NET Core 9 MVC project
- [ ] Configure SQLite database
- [ ] Set up Entity Framework Core
- [ ] Create core models (User, Teacher, Student, Rating)
- [ ] Configure .env file with Gemini API key
- [ ] Implement basic authentication

### Week 3-4: Core Features

- [ ] Implement teacher listing page
- [ ] Create teacher rating system
- [ ] Add rating validation (one per student per semester)
- [ ] Calculate average ratings
- [ ] Implement ranking system
- [ ] Create bonus calculation logic

### Week 5-6: AI Integration

- [ ] Integrate Gemini 2.5 Flash API
- [ ] Implement lesson summary generation
- [ ] Add AI question answering feature
- [ ] Create quiz generation endpoint
- [ ] Add sentiment analysis for feedback

### Week 7-8: UI/UX

- [ ] Implement neuromorphic design system
- [ ] Add dark/light mode toggle
- [ ] Create responsive layouts
- [ ] Design rating interface with stars
- [ ] Add loading states and animations
- [ ] Implement form validations

### Week 9-10: Testing & Deployment

- [ ] Write unit tests for services
- [ ] Test AI integration thoroughly
- [ ] Perform security audit
- [ ] Optimize database queries
- [ ] Create deployment package
- [ ] Write user documentation

### Phase 2: Enhanced Features (Teacher Attendance & Settings)

- [ ] Implement teacher absence tracking system
- [ ] Create admin approval workflow for teacher absences
- [ ] Add substitute teacher assignment
- [ ] Update student attendance to per-day tracking
- [ ] Add mid-day status updates (sick, out-of-class, etc.)
- [ ] Implement bonus tier management UI
- [ ] Create system settings admin panel
- [ ] Add attendance history views for teachers and students

### Phase 3: Student Dashboard & Learning Management System

#### Student Dashboard Core

- [ ] Create student dashboard layout and navigation
- [ ] Implement homework/assignment viewing with due dates
- [ ] Build file upload system for assignment submissions
- [ ] Display grades with personal stats and class averages
- [ ] Create calendar view with all due dates and events
- [ ] Implement reading assignment tracking system
- [ ] Add teacher notes display for next class
- [ ] Build completion status tracking for readings

#### AI Study Companion

- [ ] Integrate AI companion interface in student dashboard
- [ ] Implement "Explain Concept" mode (detailed explanations)
- [ ] Implement "Guide to Solution" mode (step-by-step guidance, no direct answers)
- [ ] Implement "Show Answer Only" mode (answer without explanation)
- [ ] Add comprehension check system for required readings
- [ ] Build AI study plan generator based on due dates
- [ ] Implement context-aware AI responses (understands current coursework)
- [ ] Add AI usage limits and safety guardrails

#### Gamification & Collaboration

- [ ] Design and implement student note sharing system
- [ ] Add points/badges for shared notes (views, likes)
- [ ] Create leaderboard for student contributions
- [ ] Implement note search and filtering by subject/tags
- [ ] Add note quality rating system

#### Extra Classes & Meetings

- [ ] Build extra class scheduling interface for teachers
- [ ] Integrate with external meeting platforms (Google Meet, Zoom, Teams)
- [ ] Create student enrollment system for extra classes
- [ ] Add calendar integration for scheduled meetings
- [ ] Implement attendance tracking for extra classes
- [ ] Add email/notification system for meeting reminders

#### Analytics & Insights

- [ ] Create personalized study recommendations
- [ ] Build "What's Next" and "Urgent" priority system
- [ ] Add module/reading recommendations before class
- [ ] Implement performance tracking over time
- [ ] Create class average comparisons
- [ ] Add subject-wise performance breakdown

---

## 14. API Costs & Limits (Gemini 2.5 Flash)

### Free Tier Limits

- **Rate Limit**: 15 requests per minute (RPM)
- **Daily Limit**: 1,500 requests per day
- **Input Tokens**: Free (up to limit)
- **Output Tokens**: Free (up to limit)
- **Context Window**: 1 million tokens
- **Cost**: $0

### Paid Tier (if scaling)

- **Input**: $0.075 per 1M tokens
- **Output**: $0.30 per 1M tokens
- **Rate Limit**: Much higher (60+ RPM)

### Recommendations for Demo

- Use **free tier** for development and demo
- Implement caching to reduce API calls
- Add rate limiting on client side
- Queue AI requests during high load
- Consider upgrading if > 1,500 requests/day

---

## 15. File Structure Summary

```
RateMyTeacher/
├── .env                          # Environment variables (API keys)
├── .env.example                  # Template for .env
├── .gitignore                    # Git ignore file
├── appsettings.json              # App configuration
├── Program.cs                    # Application entry point
├── Controllers/
│   ├── HomeController.cs
│   ├── TeachersController.cs
│   ├── LessonsController.cs
│   ├── GradesController.cs
│   └── StudentsController.cs
├── Models/
│   ├── User.cs
│   ├── Teacher.cs
│   ├── Student.cs
│   ├── Rating.cs
│   ├── Lesson.cs
│   ├── Grade.cs
│   └── ... (other models)
├── Services/
│   ├── IGeminiService.cs
│   ├── GeminiService.cs
│   ├── ITeacherService.cs
│   ├── TeacherService.cs
│   └── ... (other services)
├── Data/
│   ├── ApplicationDbContext.cs
│   └── ratemyteacher.db         # SQLite database file
├── Views/
│   ├── Home/
│   ├── Teachers/
│   ├── Lessons/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   └── _ViewImports.cshtml
├── wwwroot/
│   ├── css/
│   │   ├── site.css
│   │   └── neuromorphic.css
│   ├── js/
│   │   ├── site.js
│   │   ├── theme.js
│   │   └── ai-chat.js
│   ├── images/
│   └── lib/
├── Resources/                    # Localization files
│   ├── Controllers/
│   └── Views/
└── Migrations/                   # EF Core migrations
```

---

## 16. Getting Started Guide

### Prerequisites

1. .NET 9 SDK installed
2. Visual Studio Code or Visual Studio 2022
3. Gemini API key from [Google AI Studio](https://aistudio.google.com/apikey)

### Quick Start

1. **Clone the repository** (or create new project)
2. **Copy `.env.example` to `.env`**
3. **Add your Gemini API key** to `.env`
4. **Run**: `dotnet restore`
5. **Run**: `dotnet ef database update`
6. **Run**: `dotnet run`
7. **Open**: https://localhost:5001

### First Time Setup

1. Create admin account
2. Add sample teachers and students
3. Test rating system
4. Try AI features (summary, Q&A, quiz generation)
5. Toggle dark/light mode
6. Test on mobile devices

---

## 17. Troubleshooting

### Common Issues

**Issue**: Gemini API key not found

- **Solution**: Ensure `.env` file exists and contains `GEMINI_API_KEY=your_key`

**Issue**: Database not created

- **Solution**: Run `dotnet ef database update`

**Issue**: AI requests timing out

- **Solution**: Check internet connection, verify API key, check rate limits

**Issue**: Dark mode not working

- **Solution**: Clear browser cache, check `theme.js` is loaded

**Issue**: Rating not saving

- **Solution**: Check ModelState validation, verify student is enrolled

---

## Next Steps

After completing Phase 1 MVP, proceed with:

1. **Phase 2**: Advanced features (voice-to-text, advanced analytics)
2. **Phase 3**: Student-Side Dashboard.
3. **Phase 4**: Multi-school support
4. **Production**: Deploy to Azure/AWS

---

_Document Version: 1.0_  
_Last Updated: October 22, 2025_  
_Technology: ASP.NET Core 9 MVC + SQLite + Gemini 2.5 Flash_
