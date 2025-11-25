# Implementation Plan: RateMyTeacher

**Branch**: `main` | **Date**: 2025-10-23 | **Spec**: [spec.md](./spec.md)

## Summary

RateMyTeacher is a comprehensive educational platform for modern teaching workflows. The SPEC defines four capability pillars that this plan now mirrors one-to-one:

1. **Lesson & Schedule Management** – timetable automation, substitution workflows, AI lesson planning assistant, structured lesson summaries, and attendance/absence tracking.
2. **Teaching Log & Feedback** – daily “What I taught” logs, voice-to-text reflections, student post-class feedback, anonymous channels, and AI-generated weekly/monthly reports.
3. **Performance & Bonus System** – RateMyTeacher ratings, streamlined Admin-led user management, leaderboard-driven bonuses, recognition badges, and teacher improvement recommendations.
4. **Grades & Data Unification** – unified gradebook, cross-term tracking, subject-wide comparisons, and AI alerts for anomalies.

### Direction Update – 2025-11-18

- Roles are restricted to **Admin, Teacher, Student**. All department head, principal, parent/guardian, and sysadmin personas roll into Admin responsibilities with no scoped overrides.
- AI governance is intentionally simple: one global enable toggle plus Teacher AI Mode (Unrestricted/Guided/Off) and Student AI Mode (Learning/Unrestricted/Off). Scoped overrides and per-class switches were removed.
- Neuromorphic styling is deferred; keep Bootstrap defaults crisp and accessible.
- Admin-driven password resets must force a `MustChangePassword` flow before non-admins can continue.

**Release Cadence & Phases** (detailed roadmap below): Phase 1 establishes infrastructure, Phase 2A locks accessibility/localization (complete), Phase 2B introduces AI governance + logging, Phase 3–4 deliver RateMyTeacher MVP, Phase 5A focuses on AI lesson experiences, Phase 5B brings attendance & schedule operations, Phase 6 extends sentiment + anonymous feedback, and Phase 7 unifies grades/insights.

**Technical Approach**: ASP.NET Core 10 MVC with SQLite embedded database, Entity Framework Core for ORM, Google Gemini 2.5 Flash for AI features, Razor Views with vanilla JavaScript, Chart.js for analytics, SignalR for timetable updates, and background services for reminders/reporting.

## Phase & Story Alignment

| Phase | Priority | Scope | SPEC References |
|-------|----------|-------|-----------------|
| Phase 1 | P1 | Project setup, EF Core, auth shell | §3.1 RateMyTeacher (infrastructure) |
| Phase 2 | P1 | Foundational data models + services | §3.1 |
| Phase 2A ✅ | P1 | Accessibility + localization baseline | Principles, §Lesson intro |
| Phase 2B | P1 | Global AI toggle + teacher/student modes, AI usage logging, safety toggles | §1.3, §3 AI Transparency |
| Phase 3 (US1) | P1 | Student ratings + enrollment enforcement | §3.1 RateMyTeacher |
| Phase 4 (US2) | P1 | Leaderboard, payouts, bonus audit trail | §3.1 RateMyTeacher |
| Phase 5A (US3) | P2 | AI lesson summary generator + structured outputs | §1.3 Lesson Summary Generator |
| Phase 5B (US4) | P2 | Lesson planning assistant, teaching resource hub | §1.2 Lesson Planning Assistant |
| Phase 6A (US5) | P2 | Attendance & absence tracker with substitution + timetable automation | §1.1 Timetable, §1.4 Attendance |
| Phase 6B (US6) | P2 | Teaching log, voice-to-text, AI weekly/monthly reports | §2.1–§2.4 |
| Phase 7A (US7) | P3 | Anonymous feedback, recognition badges, improvement programme | §3.2–§3.4 |
| Phase 7B (US8) | P3 | Gradebook, cross-term tracking, subject comparisons, AI alerts | §4.1–§4.4 |

Each user story bundles domain models, services, UI, telemetry, and acceptance tests so QA can validate increments independently.

## Technical Context

**Language/Version**: C# / .NET 10  
**Framework**: ASP.NET Core 10 MVC  
**Primary Dependencies**:

- Entity Framework Core 10.0 (ORM & Migrations)
- Google.GenerativeAI (Gemini 2.5 Flash integration)
- DotNetEnv (environment variable management)
- Chart.js + SignalR (client-side data visualization & realtime updates)
- xUnit, Moq, EFCore.InMemory (test harness per TDD mandate)

**Storage**: SQLite (embedded, no external database server required)  
**Testing**: xUnit, Moq (for mocking), EF Core InMemory (integration tests)  
**Target Platform**: Cross-platform (Linux, Windows, macOS)  
**Project Type**: Web application (single ASP.NET Core MVC project)  
**Performance Goals**:

- <200 ms response time for page loads (p95)
- Support 100+ concurrent users
- <100 ms for permission evaluation queries
- Gemini calls (including retries) finish within 30 seconds
- Reminder jobs send notifications within ±1 minute of the 15-minute mark

**Constraints**:

- Must work with embedded SQLite (no external DB infrastructure)
- API key stored in `.env` file only (never in version control)
- Bonus configurations must live in the database with admin CRUD (per spec)
- HTTPS-only in production
- Neuromorphic styling is deferred; focus on Bootstrap defaults and accessibility basics. Localization commitments remain.
- Must support phased rollout (P1/P2/P3 features) with feature flags hiding incomplete modules

**Scale/Scope**:

- ~500-1000 students per school
- ~50-100 teachers per school
- ~10-20 admins per school
- Multiple semesters per year (historical data retention)
- AI usage logs retained 90 days per safety rules

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### I. Security-First Development (NON-NEGOTIABLE)

- ✅ Input validation on client and server sides (controllers, view models, JS)
- ✅ SQL injection prevention via EF Core parameterized queries
- ✅ XSS protection via Razor output encoding and CSP headers
- ✅ HTTPS-only enforced in production (launchSettings.json + middleware)
- ✅ Authentication required for all features (no anonymous access)
- ✅ Role-based access control with granular permissions (US7)
- ✅ Secrets in `.env` file loaded via DotNetEnv library

**Status**: ✅ PASS

### II. Linear Role Model (Admin > Teacher > Student)

- ✅ Exactly three roles exist; Admin is the superset that owns every management responsibility.
- ✅ Users carry a single role assignment kept in the `Users` table (no join table required).
- ✅ Authorization checks rely on simple `Role` comparisons—no scoped overrides or mixed assignments.
- ✅ Admin UI for user management must only surface these three roles and provide password reset tooling.

**Status**: ✅ PASS

### III. Test-Driven Development for Core Features (NON-NEGOTIABLE)

- ✅ P1 features: TDD with Red-Green-Refactor (failing tests before implementation)
- ✅ Regression tests for leaderboard checksum endpoint and AI summary formatting
- ✅ Target: 80%+ coverage on business logic (Services, Permission evaluation)
- ✅ Integration tests required for: Auth flows, Permission checks, DB operations, Gemini API
- ✅ Unit tests required for: Service layer, Permission logic, Bonus calculations

**Status**: ✅ PASS (will implement tests before features)

### IV. AI Transparency & Control

- ✅ Three-level AI control: Global Admin → Class Admin → Teacher
- ✅ AI modes visible to students: "Explain", "Guide", "Show Answer"
- ✅ AI failures show user-friendly messages with retry
- ✅ AI usage logged for auditing (AIUsageLog table) with "Viewed" indicator and 90-day retention

**Status**: ✅ PASS

### V. Data Integrity & Audit Trail

- ✅ One rating per student per teacher per semester (unique constraint)
- ✅ Attendance modifications logged (AttendanceLog table)
- ✅ Grade changes auditable (GradeLog table)
- ✅ Teacher absence workflow tracked (TeacherAbsence table with approval status)
- ✅ Bonus tier changes versioned (BonusConfig + BonusTier tables plus audit trail)
- ✅ Soft deletes for Users, Classes, Roles, Departments (IsDeleted flag)

**Status**: ✅ PASS

### VI. Phased Development with Feature Flags

- ✅ Phase 1: Core ratings, auth, leaderboards, AI summaries
- ✅ Phase 2: Permissions, attendance, settings, AI controls
- ✅ Phase 3: Student LMS, gamification, notes, extra classes
- ✅ Department feature toggleable via SystemSetting (EnableDepartments flag)
- ✅ Feature flags guard incomplete LMS/AI features in production
- ✅ Database schema supports all phases from P1 (nullable columns or separate tables)

**Status**: ✅ PASS

### VII. Accessibility & Localization-Ready

- ✅ ASP.NET Core i18n framework configured (base: English plus Indonesian, Mandarin roadmap)
- ✅ Prefers-reduced-motion + theme preload to avoid flashes
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
│   ├── HomeController.cs           # Landing, dashboard, badges
│   ├── TeachersController.cs       # Teacher listing, profiles
│   ├── RatingsController.cs        # Rating submission, history, enrollment checks
│   ├── LeaderboardController.cs    # Rankings, public display
│   ├── AdminController.cs          # Leaderboard admin, bonus config, reports
│   ├── AuthController.cs           # Login, logout, register
│   ├── AttendanceController.cs     # Teacher/student attendance
│   ├── ScheduleController.cs       # Timetable, reminders, substitutions
│   ├── PermissionsController.cs    # Role/permission management
│   ├── SettingsController.cs       # System settings, AI controls
│   ├── AISummaryController.cs      # AI lesson summaries
│   ├── AICompanionController.cs    # AI study companion (P3)
│   ├── AssignmentsController.cs    # Assignments, submissions (P3)
│   ├── NotesController.cs          # Student notes, sharing (P3)
│   ├── LogsController.cs           # Teaching log entries, voice notes
│   ├── ReportsController.cs        # AI weekly/monthly reports
│   └── SentimentController.cs      # Feedback analytics
│
├── Models/
│   ├── User.cs                     # Base user (Student/Teacher/Admin)
│   ├── Teacher.cs                  # Teacher-specific data
│   ├── Student.cs                  # Student-specific data
│   ├── Rating.cs                   # Student ratings of teachers
│   ├── Comment.cs                  # Rating comments
│   ├── Semester.cs                 # Academic semester
│   ├── BonusConfig.cs              # Dynamic bonus configuration
│   ├── BonusTier.cs                # Bonus tiers (position & range)
│   ├── Bonus.cs                    # Awarded payouts (audit)
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
│   ├── SubstitutionAssignment.cs   # Substitute management (P2)
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
│   ├── AIUsageLog.cs               # AI usage audit trail (viewed flag)
│   ├── TeachingLog.cs              # Daily topics, attachments, transcripts
│   ├── ReportSnapshot.cs           # AI weekly/monthly reports
│   ├── SentimentAnalysis.cs        # Sentiment of comments (P3)
│   ├── SystemSetting.cs            # Global settings (EnableDepartments, AI toggles)
│   └── ErrorViewModel.cs           # Error page model
│
├── Services/
│   ├── RatingService.cs            # Rating submission, validation, enrollment enforcement
│   ├── LeaderboardService.cs       # Leaderboard calculation, checksum snapshots
│   ├── BonusService.cs             # Bonus calculation from DB config + audit
│   ├── PermissionService.cs        # Permission evaluation (P2)
│   ├── AttendanceService.cs        # Attendance tracking (P2)
│   ├── ScheduleService.cs          # Timetable automation (SignalR, reminders)
│   ├── GeminiService.cs            # Gemini API integration
│   ├── AICompanionService.cs       # AI study companion (P3)
│   ├── SentimentService.cs         # Sentiment analysis (P3)
│   ├── LessonLogService.cs         # Teaching logs, voice-to-text
│   ├── ReportService.cs            # AI weekly reports
│   ├── NotificationService.cs      # Alerts (email/push)
│   └── LocalizationService.cs      # Resource helpers
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
│   │   ├── site.css               # Base styling
│   │   └── neuromorphic.css       # Design tokens/components
│   ├── js/
│   │   ├── site.js                # Client-side interactions
│   │   ├── theme.js               # Theme toggle
│   │   ├── schedule.js            # Timetable interactions
│   │   ├── ai.js                  # AI summary helpers
│   │   └── analytics.js           # Chart/dashboard helpers
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

## User Stories (Derived from SPECIFICATION.md)

| ID | Priority | Title | Independent Test Criteria |
|----|----------|-------|---------------------------|
| US1 | P1 | Student ratings w/ enrollment enforcement | Student submits rating once per teacher/semester; UI hides button when no enrollment; DB proves unique constraint |
| US2 | P1 | Leaderboard & bonus payouts | Admin recalculates rankings, sees threshold filtering, and payout audit entries persist |
| US3 | P2 | AI lesson summary generator | Teacher captures lesson notes, receives <300-word structured summary (Main Topics → Study Tips) with retries/logging |
| US4 | P2 | Lesson planning assistant | Teacher receives curriculum-aligned suggestions + resources per class, can bookmark and export |
| US5 | P2 | Attendance & timetable automation | Teacher sees “now/next” schedule, marks attendance (QR optional), admin assigns substitutes, notifications fire 15 minutes prior |
| US6 | P2 | Teaching log, voice-to-text, AI weekly/monthly reports | Teacher logs daily topics (attachments + transcripts) and admin downloads aggregated AI report |
| US7 | P3 | Anonymous feedback, recognition badges, improvement programme | Student posts anonymous feedback, admin moderates, automatic badges appear on teacher profile, improvement tips generated |
| US8 | P3 | Unified gradebook & analytics | Counselor views cross-term grades, radar chart, AI alerts for anomalies, CSV export |

These stories supplement Phase 2B AI governance tasks and ensure every SPEC section has an associated implementation slice.

**Structure Decision**: Single ASP.NET Core MVC project with clear separation of concerns (Controllers, Models, Services, Data). This matches the "web application" pattern but uses server-side rendering instead of a separate frontend/backend split, which is appropriate for the MVC architecture.

## Complexity Tracking

> No constitutional violations - this section is not applicable.

## Phase 0: Research

### Research Tasks

1. **Linear Role & Password Reset Enforcement**

   - Research: Best practices for hard-coding three roles with enum + lookup tables
   - Research: Admin UX for issuing temporary passwords + `MustChangePassword` gates
   - Research: Auditing role changes without the overhead of permission trees
   - Output: `research.md` section describing the simplified RBAC + reset workflow

2. **Google Gemini 2.5 Flash Integration**

   - Research: Google.GenerativeAI NuGet package usage
   - Research: Rate limiting strategies (exponential backoff)
   - Research: Error handling patterns (timeout, invalid key, quota exceeded)
   - Research: Safety settings for educational content
   - Output: `research.md` section on Gemini integration patterns

3. **Attendance & Schedule Automation**

   - Research: Daily vs per-class attendance models
   - Research: Mid-day status updates (sick, competition, out-of-class)
   - Research: Approval + substitution workflows for teacher absences
   - Research: Reminder scheduling (SignalR, background jobs, calendar sync)
   - Output: `research.md` section on attendance & timetable automation

4. **Bonus Configuration System & Audit**

   - Research: Database-driven configuration vs environment variables
   - Research: Supporting range-based bonuses (5th-10th: $2) and single positions (1st: $10)
   - Research: Hot-reload strategies for configuration changes
   - Research: Audit trails and checksum snapshot endpoints
   - Output: `research.md` section on dynamic configuration + audit patterns

5. **Soft Delete & Audit Trails in EF Core**
   - Research: Global query filters for IsDeleted
   - Research: Shadow properties vs explicit columns
   - Research: Cascade delete behavior with soft deletes
   - Research: Audit log modeling for permissions/AI usage
   - Output: `research.md` section on soft delete + audit implementation

6. **Lesson Planning Assistant & Content Recommendations**
   - Research: Curriculum tagging schemas and metadata storage
   - Research: External content APIs (OER Commons, Khan Academy) for enrichment
   - Research: Prompt engineering for “suggest three activities” instructions per grade level
   - Output: `research.md` section on AI-assisted planning best practices

7. **Voice-to-Text & Multi-Lingual Support**
   - Research: Azure Speech vs Google Cloud Speech trade-offs for en/id/zh
   - Research: Browser-based recording constraints and fallbacks for mobile
   - Research: Accessibility guidelines for transcript editing
   - Output: `research.md` section documenting chosen provider + UX considerations

8. **Grade Analytics & AI Alerts**
   - Research: Statistical methods for anomaly detection (z-score, EWMA)
   - Research: Data visualization libraries for radar charts + trend lines
   - Research: GDPR/FERPA implications for automated alerts
   - Output: `research.md` section listing detection thresholds and notification hooks

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

**Entities to Define** (~40 total to cover LMS + automation):

#### Core Entities (P1)

- User (base class: Id, Email, PasswordHash, Role, CreatedAt, IsDeleted)
- Teacher (extends User: Bio, ProfileImage, HireDate)
- Student (extends User: GradeLevel, ParentContact)
- Semester (Id, Name, StartDate, EndDate, AcademicYear)
- Rating (StudentId, TeacherId, SemesterId, Stars, Comment, CreatedAt) [Unique(StudentId, TeacherId, SemesterId)]
- BonusConfig (MinimumRatingsThreshold, Currency, CreatedAt, ModifiedAt)
- BonusTier (ConfigId, Position, RangeStart, RangeEnd, Amount, SplitMode, RequiresApproval)
- Bonus (TeacherId, SemesterId, TierId, Amount, AwardedBy, AwardedAt, BatchId)
- TeacherRanking (TeacherId, SemesterId, Rank, AverageRating, TotalRatings, BonusAmount, SnapshotChecksum)

#### Permission System Entities (P2)

- Department (Id, Name, IsEnabled, ParentDepartmentId, IsDeleted)
- Permission (Id, Name, Code, CategoryId, Description)
- PermissionCategory (Id, Name, ParentCategoryId, IsExpandable)
- Role (Id, Name, Rank, IsSystemRole, IsDeleted)
- RolePermission (RoleId, PermissionId)
- UserRole (UserId, RoleId, Scope [Global/Department/Class], ScopeId, AssignedBy, AssignedAt)
- ClassPermission (ClassId, UserId, PermissionId, GrantedBy, GrantedAt)

#### Attendance & Schedule Entities (P2)

- Schedule (Id, ClassId, TeacherId, DayOfWeek, StartTime, EndTime)
- Attendance (StudentId, ClassId, Date, Status [Present/Sick/OutOfClass/Absent], UpdatedBy, UpdatedAt)

- AttendanceLog (AttendanceId, OldStatus, NewStatus, ChangedBy, ChangedAt, Reason)
- TeacherAbsence (TeacherId, Date, Reason, Status [Pending/Approved/Rejected], RequestedAt, ApprovedBy, ApprovedAt, SubstituteTeacherId)
- SubstitutionAssignment (Id, AbsenceId, SubstituteTeacherId, ClassId, AssignedAt)
- Reminder (Id, ScheduleId, TriggerAt, SentAt, Channel)

#### LMS Entities (P3)

- Assignment (ClassId, Title, Description, DueDate, CreatedBy, CreatedAt)
- AssignmentSubmission (AssignmentId, StudentId, FileUrl, SubmittedAt, Grade, GradedBy)
- Grade (StudentId, ClassId, AssignmentId, Value, MaxValue, GradedBy, GradedAt)
- GradeLog (GradeId, OldValue, NewValue, ChangedBy, ChangedAt, Reason)
- ReadingAssignment (ClassId, Title, Content, DueDate, IsCompleted, ComprehensionCheckPassed)
- StudentNote (StudentId, ClassId, Title, Content, IsShared, CreatedAt, Likes)
- ExtraClass (ClassId, Title, MeetingUrl, ScheduledAt, CreatedBy)

#### AI & Settings Entities

- AISummary (TeacherId, ClassId, LessonNotes, Summary, GeneratedAt, ExportUrl)
- AIUsageLog (UserId, ClassId, Query, Response, Mode [Explain/Guide/ShowAnswer], Timestamp, ViewedAt)
- AIControlSetting (ScopeType, ScopeId, IsEnabled)
- SentimentAnalysis (RatingId, Sentiment [Positive/Neutral/Negative], Confidence, AnalyzedAt)
- SystemSetting (Key, Value, Description, ModifiedAt)
- ReportSnapshot (RangeStart, RangeEnd, Payload, GeneratedAt)

### API Contracts (if applicable)

For this MVC application, most interactions are server-rendered, but we may expose some JSON endpoints for AJAX:

**Directory**: `specs/main/contracts/`

**Potential JSON Endpoints**:

- `POST /api/ratings` - Submit rating (AJAX, enrollment enforcement)
- `GET /api/leaderboard/{semesterId}` - Get rankings JSON + checksum metadata
- `POST /api/leaderboard/{semesterId}/award` - Trigger bonus payouts (admin only)
- `POST /api/ai/summary` - Generate AI summary (AJAX)
- `POST /api/ai/companion` - AI study companion chat (AJAX)
- `POST /api/ai/control` - Toggle AI availability per scope (global/department/class)
- `GET /api/ai/logs` - Fetch AI usage logs filtered by scope with permissions
- `GET /api/permissions/check` - Permission evaluation (AJAX)
- `POST /api/permissions/templates` - CRUD role templates
- `POST /api/attendance/substitution` - Assign substitutes + push notifications

### Quickstart Guide

**File**: `specs/main/quickstart.md`

**Contents**:

1. Prerequisites (.NET 10 SDK, SQLite)
2. Clone repository
3. Create `.env` file with Gemini API key
4. Run migrations: `dotnet ef database update`
5. Seed default data (Admin user, default roles)
6. Run migrations: `dotnet ef database update`
7. Run tests: `dotnet test`
8. Start development server: `dotnet watch run`
9. Access at `https://localhost:5001`
10. Default credentials (Admin/admin@school.com)

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
