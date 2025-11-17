# Implementation Plan: RateMyTeacher

**Branch**: `main` | **Date**: 2025-10-23 | **Spec**: [spec.md](./spec.md)

## Summary

RateMyTeacher is a comprehensive educational platform for teacher evaluation, attendance tracking, and learning management. The system supports:

- **Phase 1 (P1)**: Teacher ratings, leaderboards with configurable bonuses, authentication, AI lesson summaries
- **Phase 2 (P2)**: Discord-style multi-role permission system, attendance tracking, AI companion controls
- **Phase 3 (P3)**: Student LMS features (assignments, notes, gamification, reading tracking)

**Technical Approach**: ASP.NET Core 10 MVC with SQLite embedded database, Entity Framework Core for ORM, Google Gemini 2.5 Flash for AI features, and Razor Views with vanilla JavaScript for the UI.

## Technical Context

**Language/Version**: C# / .NET 10  
**Framework**: ASP.NET Core 10 MVC  
**Primary Dependencies**:

- Entity Framework Core 10.0 (ORM & Migrations)
- Google.GenerativeAI (Gemini 2.5 Flash integration)
- DotNetEnv (environment variable management)
- Chart.js (client-side data visualization)

**Storage**: SQLite (embedded, no external database server required)  
**Testing**: xUnit, Moq (for mocking), EF Core InMemory (integration tests)  
**Target Platform**: Cross-platform (Linux, Windows, macOS)  
**Project Type**: Web application (single ASP.NET Core MVC project)  
**Performance Goals**:

- <200ms response time for page loads (p95)
- Support 100+ concurrent users
- <100ms for permission evaluation queries

**Constraints**:

- Must work with embedded SQLite (no external DB infrastructure)
- API key stored in `.env` file only (never in version control)
- HTTPS-only in production
- Must support phased rollout (P1/P2/P3 features)

**Scale/Scope**:

- ~500-1000 students per school
- ~50-100 teachers per school
- ~10-20 admins per school
- Multiple semesters per year (historical data retention)

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### I. Security-First Development (NON-NEGOTIABLE)

- ✅ Input validation on client and server sides
- ✅ SQL injection prevention via EF Core parameterized queries
- ✅ XSS protection via Razor output encoding and CSP headers
- ✅ HTTPS-only enforced in production (launchSettings.json + middleware)
- ✅ Authentication required for all features (no anonymous access)
- ✅ Role-based access control with granular permissions (US7)
- ✅ Secrets in `.env` file loaded via DotNetEnv library

**Status**: ✅ PASS

### II. Discord-Style Multi-Role Permission System

- ✅ Multiple role assignments per user supported (UserRole join table)
- ✅ Cumulative permissions (UNION of all role permissions)
- ✅ Role hierarchy protection (rank system prevents junior roles from modifying senior roles)
- ✅ Permission categories hierarchical and expandable (PermissionCategory table)
- ✅ Three scopes: Global → Department (optional) → Class
- ✅ Class-level permissions override department/global
- ✅ Global Admins have irrevocable full access

**Status**: ✅ PASS

### III. Test-Driven Development for Core Features (NON-NEGOTIABLE)

- ✅ P1 features: TDD with Red-Green-Refactor
- ✅ Target: 80%+ coverage on business logic (Services, Permission evaluation)
- ✅ Integration tests required for: Auth flows, Permission checks, DB operations, Gemini API
- ✅ Unit tests required for: Service layer, Permission logic, Bonus calculations

**Status**: ✅ PASS (will implement tests before features)

### IV. AI Transparency & Control

- ✅ Three-level AI control: Global Admin → Class Admin → Teacher
- ✅ AI modes visible to students: "Explain", "Guide", "Show Answer"
- ✅ AI failures show user-friendly messages with retry
- ✅ AI usage logged for auditing (AIUsageLog table)

**Status**: ✅ PASS

### V. Data Integrity & Audit Trail

- ✅ One rating per student per teacher per semester (unique constraint)
- ✅ Attendance modifications logged (AttendanceLog table)
- ✅ Grade changes auditable (GradeLog table)
- ✅ Teacher absence workflow tracked (TeacherAbsence table with approval status)
- ✅ Bonus tier changes versioned (BonusConfig table with modified timestamp)
- ✅ Soft deletes for Users, Classes, Roles, Departments (IsDeleted flag)

**Status**: ✅ PASS

### VI. Phased Development with Feature Flags

- ✅ Phase 1: Core ratings, auth, leaderboards, AI summaries
- ✅ Phase 2: Permissions, attendance, settings, AI controls
- ✅ Phase 3: Student LMS, gamification, notes, extra classes
- ✅ Department feature toggleable via SystemSetting (EnableDepartments flag)
- ✅ Database schema supports all phases from P1 (nullable columns or separate tables)

**Status**: ✅ PASS

### VII. Accessibility & Localization-Ready

- ✅ ASP.NET Core i18n framework configured (base: English)
- ✅ Resource files structured by controller/view
- ✅ ARIA labels for interactive elements
- ✅ Keyboard navigation support
- ✅ Dark/light mode (neuromorphic design, CSS variables)
- ✅ WCAG 2.1 AA color contrast ratios

**Status**: ✅ PASS

---

**OVERALL CONSTITUTION CHECK**: ✅ **PASS** - No violations, all principles satisfied.

## Project Structure

### Documentation (this feature)

```text
specs/main/
├── plan.md              # This file
├── spec.md              # Feature specification (user stories)
├── tasks.md             # Task breakdown (created via /speckit.tasks)
├── research.md          # Phase 0 research findings
├── data-model.md        # Phase 1 entity design
├── quickstart.md        # Phase 1 setup guide
└── contracts/           # Phase 1 API contracts (if applicable)
```

### Source Code (repository root)

```text
RateMyTeacher/
├── Controllers/
│   ├── HomeController.cs           # Landing, dashboard
│   ├── TeachersController.cs       # Teacher listing, profiles
│   ├── RatingsController.cs        # Rating submission, history
│   ├── LeaderboardController.cs    # Rankings, bonus display
│   ├── AuthController.cs           # Login, logout, register
│   ├── AttendanceController.cs     # Teacher/student attendance
│   ├── PermissionsController.cs    # Role/permission management
│   ├── SettingsController.cs       # System settings, bonus config
│   ├── AISummaryController.cs      # AI lesson summaries
│   ├── AICompanionController.cs    # AI study companion (P3)
│   ├── AssignmentsController.cs    # Assignments, submissions (P3)
│   └── NotesController.cs          # Student notes, sharing (P3)
│
├── Models/
│   ├── User.cs                     # Base user (Student/Teacher/Admin)
│   ├── Teacher.cs                  # Teacher-specific data
│   ├── Student.cs                  # Student-specific data
│   ├── Rating.cs                   # Student ratings of teachers
│   ├── Comment.cs                  # Rating comments
│   ├── Semester.cs                 # Academic semester
│   ├── BonusConfig.cs              # Dynamic bonus configuration
│   ├── BonusTier.cs                # Bonus tiers (1st: $10, 2nd: $5, etc.)
│   ├── TeacherRanking.cs           # Computed rankings per semester
│   │
│   ├── Department.cs               # Optional departments (P2)
│   ├── Permission.cs               # Granular permissions (P2)
│   ├── PermissionCategory.cs       # Permission categories (P2)
│   ├── Role.cs                     # Role templates (P2)
│   ├── RolePermission.cs           # Many-to-many Role-Permission (P2)
│   ├── UserRole.cs                 # Many-to-many User-Role (P2)
│   ├── ClassPermission.cs          # Class-level permission overrides (P2)
│   │
│   ├── Attendance.cs               # Student daily attendance (P2)
│   ├── AttendanceLog.cs            # Audit trail for attendance changes (P2)
│   ├── TeacherAbsence.cs           # Teacher absence requests (P2)
│   ├── Schedule.cs                 # Class schedules (P2)
│   │
│   ├── Assignment.cs               # Homework/assignments (P3)
│   ├── AssignmentSubmission.cs     # Student submissions (P3)
│   ├── Grade.cs                    # Grades with audit trail (P3)
│   ├── GradeLog.cs                 # Grade change history (P3)
│   ├── ReadingAssignment.cs        # Required readings (P3)
│   ├── StudentNote.cs              # Shareable notes (P3)
│   ├── ExtraClass.cs               # Extra meetings (Zoom, Meet) (P3)
│   │
│   ├── AISummary.cs                # AI-generated lesson summaries
│   ├── AIUsageLog.cs               # AI usage audit trail
│   ├── SentimentAnalysis.cs        # Sentiment of comments (P3)
│   ├── SystemSetting.cs            # Global settings (e.g., EnableDepartments)
│   └── ErrorViewModel.cs           # Error page model
│
├── Services/
│   ├── RatingService.cs            # Rating submission, validation
│   ├── RankingService.cs           # Leaderboard calculation
│   ├── BonusService.cs             # Bonus calculation from config
│   ├── PermissionService.cs        # Permission evaluation (P2)
│   ├── AttendanceService.cs        # Attendance tracking (P2)
│   ├── GeminiService.cs            # Gemini API integration
│   ├── AICompanionService.cs       # AI study companion (P3)
│   ├── SentimentService.cs         # Sentiment analysis (P3)
│   └── NotificationService.cs      # Alerts (future)
│
├── Data/
│   ├── ApplicationDbContext.cs     # EF Core DbContext
│   └── Migrations/                 # EF Core migrations
│
├── Views/
│   ├── Home/
│   ├── Teachers/
│   ├── Ratings/
│   ├── Leaderboard/
│   ├── Auth/
│   ├── Attendance/
│   ├── Permissions/
│   ├── Settings/
│   ├── Assignments/ (P3)
│   ├── Notes/ (P3)
│   └── Shared/
│       ├── _Layout.cshtml
│       └── _ValidationScriptsPartial.cshtml
│
├── wwwroot/
│   ├── css/
│   │   └── site.css               # Dark/light mode, neuromorphic
│   ├── js/
│   │   └── site.js                # Client-side interactions
│   └── lib/                       # Bootstrap, jQuery, Chart.js
│
├── Resources/                     # Localization (i18n)
│   ├── Controllers/
│   └── Views/
│
├── appsettings.json               # App configuration
├── appsettings.Development.json   # Dev overrides
├── .env                           # Gemini API key (not in git)
├── Program.cs                     # App entry point
└── RateMyTeacher.csproj           # Project file
```

**Structure Decision**: Single ASP.NET Core MVC project with clear separation of concerns (Controllers, Models, Services, Data). This matches the "web application" pattern but uses server-side rendering instead of a separate frontend/backend split, which is appropriate for the MVC architecture.

## Complexity Tracking

> No constitutional violations - this section is not applicable.

## Phase 0: Research

### Research Tasks

1. **Discord-Style Permission System Implementation in EF Core**

   - Research: Many-to-many relationships with payloads (UserRole with scope)
   - Research: Hierarchical data structures (permission categories)
   - Research: Performance optimization for permission queries (caching, eager loading)
   - Output: `research.md` section on permission data model best practices

2. **Google Gemini 2.5 Flash Integration**

   - Research: Google.GenerativeAI NuGet package usage
   - Research: Rate limiting strategies (exponential backoff)
   - Research: Error handling patterns (timeout, invalid key, quota exceeded)
   - Research: Safety settings for educational content
   - Output: `research.md` section on Gemini integration patterns

3. **Attendance Tracking Best Practices**

   - Research: Daily vs per-class attendance models
   - Research: Mid-day status updates (sick, competition, out-of-class)
   - Research: Approval workflows for teacher absences
   - Output: `research.md` section on attendance system design

4. **Bonus Configuration System**

   - Research: Database-driven configuration vs environment variables
   - Research: Supporting range-based bonuses (5th-10th: $2) and single positions (1st: $10)
   - Research: Hot-reload strategies for configuration changes
   - Output: `research.md` section on dynamic configuration patterns

5. **Soft Delete Pattern in EF Core**
   - Research: Global query filters for IsDeleted
   - Research: Shadow properties vs explicit columns
   - Research: Cascade delete behavior with soft deletes
   - Output: `research.md` section on soft delete implementation

### Research Deliverables

- **File**: `specs/main/research.md`
- **Sections**:
  - Permission System Architecture
  - Gemini API Integration Patterns
  - Attendance Workflow Design
  - Dynamic Configuration Management
  - Soft Delete Best Practices
- **Format**: Decision, Rationale, Alternatives Considered for each topic

## Phase 1: Design & Contracts

### Data Model

**File**: `specs/main/data-model.md`

**Entities to Define** (27 total):

#### Core Entities (P1)

- User (base class: Id, Email, PasswordHash, Role, CreatedAt, IsDeleted)
- Teacher (extends User: Bio, ProfileImage, HireDate)
- Student (extends User: GradeLevel, ParentContact)
- Semester (Id, Name, StartDate, EndDate, AcademicYear)
- Rating (StudentId, TeacherId, SemesterId, Stars, Comment, CreatedAt) [Unique(StudentId, TeacherId, SemesterId)]
- BonusConfig (MinimumRatingsThreshold, CreatedAt, ModifiedAt)
- BonusTier (ConfigId, Position, RangeStart, RangeEnd, Amount) [e.g., 1st: $10, 5th-10th: $2]
- TeacherRanking (TeacherId, SemesterId, Rank, AverageRating, TotalRatings, BonusAmount)

#### Permission System Entities (P2)

- Department (Id, Name, IsEnabled, ParentDepartmentId, IsDeleted)
- Permission (Id, Name, Code, CategoryId, Description)
- PermissionCategory (Id, Name, ParentCategoryId, IsExpandable)
- Role (Id, Name, Rank, IsSystemRole, IsDeleted)
- RolePermission (RoleId, PermissionId)
- UserRole (UserId, RoleId, Scope [Global/Department/Class], ScopeId, AssignedBy, AssignedAt)
- ClassPermission (ClassId, UserId, PermissionId, GrantedBy, GrantedAt)

#### Attendance Entities (P2)

- Schedule (Id, ClassId, TeacherId, DayOfWeek, StartTime, EndTime)
- Attendance (StudentId, ClassId, Date, Status [Present/Sick/OutOfClass/Absent], UpdatedBy, UpdatedAt)
- AttendanceLog (AttendanceId, OldStatus, NewStatus, ChangedBy, ChangedAt, Reason)
- TeacherAbsence (TeacherId, Date, Reason, Status [Pending/Approved/Rejected], RequestedAt, ApprovedBy, ApprovedAt, SubstituteTeacherId)

#### LMS Entities (P3)

- Assignment (ClassId, Title, Description, DueDate, CreatedBy, CreatedAt)
- AssignmentSubmission (AssignmentId, StudentId, FileUrl, SubmittedAt, Grade, GradedBy)
- Grade (StudentId, ClassId, AssignmentId, Value, MaxValue, GradedBy, GradedAt)
- GradeLog (GradeId, OldValue, NewValue, ChangedBy, ChangedAt, Reason)
- ReadingAssignment (ClassId, Title, Content, DueDate, IsCompleted, ComprehensionCheckPassed)
- StudentNote (StudentId, ClassId, Title, Content, IsShared, CreatedAt, Likes)
- ExtraClass (ClassId, Title, MeetingUrl, ScheduledAt, CreatedBy)

#### AI & Settings Entities

- AISummary (TeacherId, ClassId, LessonNotes, Summary, GeneratedAt)
- AIUsageLog (UserId, ClassId, Query, Response, Mode [Explain/Guide/ShowAnswer], Timestamp)
- SentimentAnalysis (RatingId, Sentiment [Positive/Neutral/Negative], Confidence, AnalyzedAt)
- SystemSetting (Key, Value, Description, ModifiedAt)

### API Contracts (if applicable)

For this MVC application, most interactions are server-rendered, but we may expose some JSON endpoints for AJAX:

**Directory**: `specs/main/contracts/`

**Potential JSON Endpoints**:

- `POST /api/ratings` - Submit rating (AJAX)
- `GET /api/leaderboard/{semesterId}` - Get rankings JSON
- `POST /api/ai/summary` - Generate AI summary (AJAX)
- `POST /api/ai/companion` - AI study companion chat (AJAX)
- `GET /api/permissions/check` - Permission evaluation (AJAX)

### Quickstart Guide

**File**: `specs/main/quickstart.md`

**Contents**:

1. Prerequisites (.NET 10 SDK, SQLite)
2. Clone repository
3. Create `.env` file with Gemini API key
4. Run migrations: `dotnet ef database update`
5. Seed default data (Admin user, default roles)
6. Run application: `dotnet run`
7. Access at `https://localhost:5001`
8. Default credentials (Admin/admin@school.com)

### Agent Context Update

After completing Phase 1 design:

- Run `.specify/scripts/powershell/update-agent-context.ps1 -AgentType copilot` (or equivalent)
- Add: ASP.NET Core 10 MVC, Entity Framework Core 10, Google.GenerativeAI
- Preserve manual additions between markers

## Phase 2: Implementation Planning

This phase is handled by the `/speckit.tasks` command, which will generate `specs/main/tasks.md` with:

- Task breakdown by user story (US1-US7)
- Test tasks (unit, integration) for TDD compliance
- Database migration tasks
- Parallel/sequential task ordering
- File paths for each task

**Not included in this plan.md** - see tasks.md after running `/speckit.tasks`.

## Next Steps

1. ✅ **Constitution Check**: PASSED
2. ⏳ **Phase 0**: Generate `research.md` (5 research topics)
3. ⏳ **Phase 1**: Generate `data-model.md` (27 entities)
4. ⏳ **Phase 1**: Generate `contracts/` (optional JSON endpoints)
5. ⏳ **Phase 1**: Generate `quickstart.md` (setup instructions)
6. ⏳ **Phase 1**: Update agent context (add new technologies)
7. ⏳ **Phase 1**: Re-check constitution compliance
8. ⏳ **Phase 2**: Run `/speckit.tasks` to generate implementation tasks

**Current Status**: Plan complete, ready for Phase 0 research.

---

**Plan Version**: 1.0.0 | **Last Updated**: 2025-10-23
