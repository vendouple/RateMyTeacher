# Tasks: Application for Teacher (SPEC-Aligned)

**Input**: `/specs/main/plan.md`, `/specs/main/spec.md`, `/specs/main/data-model.md`, `/specs/main/research.md`, `/specs/main/contracts/`

Tests are optional but encouraged for P1 items due to constitution Principle III. Each task references the concrete file(s) it touches so work can be independently assigned.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish project scaffolding, configuration, and developer ergonomics.

- [X] T001 Add required EF Core, SQLite, DotNetEnv, and Mscc.GenerativeAI package references in `RateMyTeacher/RateMyTeacher.csproj`.
- [X] T002 Create `.env.example` with Gemini, database, and bonus configuration keys in `RateMyTeacher/.env.example`.
- [X] T003 Update ignore rules for environment files and SQLite artifacts in `RateMyTeacher/.gitignore`.
- [X] T004 Document project overview, environment setup, and spec references in `RateMyTeacher/README.md`.
- [X] T005 Add placeholder documentation for data directory responsibilities in `RateMyTeacher/Data/README.md`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Provide core infrastructure all user stories depend on.

- [X] T006 Create EF Core context with DbSets and configuration stubs in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [X] T007 Configure dependency injection, DotNetEnv loading, and SQLite connection in `RateMyTeacher/Program.cs`.
- [X] T008 [P] Implement base user aggregate with roles and metadata in `RateMyTeacher/Models/User.cs`.
- [X] T009 [P] Implement teacher profile model with navigation properties in `RateMyTeacher/Models/Teacher.cs`.
- [X] T010 [P] Implement student model including enrollment metadata in `RateMyTeacher/Models/Student.cs`.
- [X] T011 Add design-time context factory to support migrations in `RateMyTeacher/Data/DesignTimeDbContextFactory.cs`.
- [X] T012 Scaffold cookie-based authentication endpoints in `RateMyTeacher/Controllers/AccountController.cs` and associated views under `RateMyTeacher/Views/Account/`.
- [X] T013 Refresh shared layout with navigation placeholders and theme toggle slot in `RateMyTeacher/Views/Shared/_Layout.cshtml`.

---

## Phase 2A: Accessibility & Localization Baseline (Principle VII)

**Purpose**: Ship neuromorphic styling, dark/light parity, and localization scaffolding before user stories.

- [X] T014 Configure ASP.NET Core localization services, add base resource folders, and wire default cultures in `Program.cs`.
- [X] T015 Introduce neuromorphic styling in `RateMyTeacher/wwwroot/css/neuromorphic.css`, connect tokens into `_Layout.cshtml`, and ensure both themes render.
- [X] T016 Implement theme toggle behavior plus initial icons in `RateMyTeacher/wwwroot/js/theme.js` and verify the toggle loads before first paint.

---

## Phase 2B: AI Governance & Usage Logging (Priority: P1 compliance)

**Purpose**: Enforce AI transparency, scoped toggles, and auditing before layering additional AI features.

- [ ] T017 Add AI control scope configuration (Global/Department/Class) in `RateMyTeacher/Models/SystemSetting.cs` and register supporting tables in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [ ] T018 Create `RateMyTeacher/Models/AIControlSetting.cs` plus EF migration to persist per-scope enablement, defaulting to "Explain" mode.
- [ ] T019 Implement AI usage logging pipeline (service + repository) in `RateMyTeacher/Services/AIUsageService.cs` and hook into `GeminiService` to capture prompts, outputs, and viewed flags.
- [ ] T020 Build AI governance admin UI in `RateMyTeacher/Controllers/SettingsController.cs` with Razor view `RateMyTeacher/Views/Settings/AiControls.cshtml` so admins can toggle modes and review recent logs.

---

## Phase 3: User Story 1 – Teacher Listing & Ratings (Priority: P1) 🎯

**Goal**: Students can view teachers, submit a single rating per term, and see averages tied to enrollments.

- [X] T021 [P] [US1] Define rating entity with validation attributes in `RateMyTeacher/Models/Rating.cs`.
- [X] T022 [US1] Configure rating relationships, unique index `(StudentId, TeacherId, Semester)` and enrollment checks in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [X] T023 [P] [US1] Declare rating contract in `RateMyTeacher/Services/IRatingService.cs`.
- [X] T024 [US1] Implement rating logic (submit, list, averages) in `RateMyTeacher/Services/RatingService.cs`.
- [X] T025 [P] [US1] Declare teacher retrieval contract in `RateMyTeacher/Services/ITeacherService.cs`.
- [X] T026 [US1] Implement teacher listing with eager loading + semester filters in `RateMyTeacher/Services/TeacherService.cs`.
- [X] T027 [US1] Build MVC endpoints (Index, Details, Rate) in `RateMyTeacher/Controllers/TeachersController.cs`.
- [X] T028 [P] [US1] Create teacher list UI with neuromorphic cards in `RateMyTeacher/Views/Teachers/Index.cshtml`.
- [X] T029 [P] [US1] Create teacher detail view with rating summary and eligibility messaging in `RateMyTeacher/Views/Teachers/Details.cshtml`.
- [X] T030 [P] [US1] Implement interactive rating widget and submission logic in `RateMyTeacher/wwwroot/js/rating.js`.
- [X] T031 [US1] Seed sample teachers, students, enrollments in `RateMyTeacher/Data/Seed/SeedData.cs` and wire execution at startup.
- [X] T032 [US1] Register teacher and rating services plus MVC routes in `RateMyTeacher/Program.cs`.

---

## Phase 4: User Story 2 – Leaderboard & Bonus Calculation (Priority: P1)

**Goal**: Admins view semester rankings meeting vote thresholds and approve payouts per business rules.

- [X] T033 [US2] Extend `RateMyTeacher/Services/RatingService.cs` with ranking aggregation filtered by semester and vote threshold.
- [X] T034 [P] [US2] Create bonus payout entity in `RateMyTeacher/Models/Bonus.cs` with teacher linkage.
- [X] T035 [US2] Register `Bonuses` DbSet and bonus constraints in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [X] T036 [US2] Implement environment-driven payout logic in `RateMyTeacher/Services/BonusService.cs`.
- [X] T037 [US2] Add admin leaderboard and bonus distribution actions in `RateMyTeacher/Controllers/AdminController.cs`.
- [X] T038 [P] [US2] Build leaderboard view with rankings and payouts in `RateMyTeacher/Views/Admin/Leaderboard.cshtml`.
- [X] T039 [US2] Capture bonus audit trail via EF migration artifacts under `RateMyTeacher/Migrations/`.
- [X] T040 [US2] Add checksum snapshot endpoint/tests in `RateMyTeacher/Controllers/AdminController.cs` to support regression replays.

---

## Phase 5A: User Story 3 – Lesson Summary Generator (Priority: P2)

**Goal**: Teachers request Gemini 2.5 Flash summaries capped at 300 words with structured sections.

- [ ] T041 [P] [US3] Define AI contract for summaries/quizzes/questions in `RateMyTeacher/Services/IGeminiService.cs`.
- [ ] T042 [US3] Implement Gemini client with retry + timeout handling in `RateMyTeacher/Services/GeminiService.cs`.
- [ ] T043 [P] [US3] Model lesson metadata in `RateMyTeacher/Models/Lesson.cs` (notes, class, attachments).
- [ ] T044 [P] [US3] Model lesson summaries with export flags + checksum in `RateMyTeacher/Models/LessonSummary.cs`.
- [ ] T045 [US3] Register lesson entities and storage configuration in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [ ] T046 [US3] Add create/summary endpoints to `RateMyTeacher/Controllers/LessonsController.cs` wiring Gemini responses.
- [ ] T047 [P] [US3] Build lesson authoring view with structured summary display in `RateMyTeacher/Views/Lessons/Create.cshtml`.
- [ ] T048 [P] [US3] Implement summary request client script with optimistic UI + retries in `RateMyTeacher/wwwroot/js/lesson-summary.js`.
- [ ] T049 [US3] Register Gemini options (model name, safety settings) in `Program.cs`.
- [ ] T050 [US3] Persist generated summaries/errors via repository updates in `RateMyTeacher/Services/LessonService.cs`.

---

## Phase 5B: User Story 4 – Lesson Planning Assistant (Priority: P2)

**Goal**: Teachers receive curriculum-aligned activity suggestions and resource links before class.

- [ ] T051 [P] [US4] Introduce planning entities (`LessonPlanRequest`, `LessonPlanSuggestion`) in `RateMyTeacher/Models/LessonPlan.cs`.
- [ ] T052 [US4] Create planning service interface/implementation in `RateMyTeacher/Services/ILessonPlanningService.cs` + `LessonPlanningService.cs` that orchestrates Gemini prompts and OER lookups.
- [ ] T053 [US4] Store curriculum tags + grade metadata via new tables in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [ ] T054 [US4] Build planning endpoints in `RateMyTeacher/Controllers/LessonPlanningController.cs` with filters for subject/grade.
- [ ] T055 [P] [US4] Design planning workspace UI (`RateMyTeacher/Views/LessonPlanning/Index.cshtml`) with saved suggestions + export.
- [ ] T056 [US4] Implement bookmarking + export to PDF/Word via `RateMyTeacher/Services/ExportService.cs`.
- [ ] T057 [US4] Cache suggestion history and provide re-run controls in `RateMyTeacher/wwwroot/js/lesson-planning.js`.
- [ ] T058 [US4] Seed starter curriculum tags/resources in `RateMyTeacher/Data/Seed/SeedData.cs`.
- [ ] T059 [US4] Add health metrics/logging for planning prompts in `RateMyTeacher/Services/AIUsageService.cs`.
- [ ] T060 [US4] Document planning workflow + prompt guardrails in `specs/main/plan.md` notes section.

---

## Phase 6A: User Story 5 – Attendance & Timetable Automation (Priority: P2)

**Goal**: Teachers view today’s schedule, mark attendance (QR optional), admins assign substitutes, and parents get alerts.

- [ ] T061 [P] [US5] Create schedule entity with class timing metadata in `RateMyTeacher/Models/Schedule.cs` plus SignalR channel for now/next updates.
- [ ] T062 [P] [US5] Create attendance entity + status enum (Present/Sick/OutOfClass/Absent) in `RateMyTeacher/Models/Attendance.cs` with audit trail.
- [ ] T063 [US5] Configure schedule/attendance DbSets, unique indexes, and default "present" seeding job in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [ ] T064 [US5] Implement attendance management logic + audit logging in `RateMyTeacher/Services/AttendanceService.cs`.
- [ ] T065 [US5] Add schedule projection + substitution assignment endpoints in `RateMyTeacher/Controllers/ScheduleController.cs`.
- [ ] T066 [US5] Expose quick mark and QR endpoints in `RateMyTeacher/Controllers/AttendanceController.cs` plus parent notification hooks.
- [ ] T067 [P] [US5] Build schedule dashboard UI in `RateMyTeacher/Views/Schedule/Today.cshtml` showing current/next and absence flags.
- [ ] T068 [US5] Provide QR scanning + quick mark script in `RateMyTeacher/wwwroot/js/attendance.js`.
- [ ] T069 [US5] Seed sample schedules, rosters, and substitution scenarios in `RateMyTeacher/Data/Seed/SeedData.cs`.
- [ ] T070 [US5] Implement reminder background service (15-min warnings) in `RateMyTeacher/Services/ReminderService.cs` + hosted service registration.

---

## Phase 6B: User Story 6 – Teaching Log, Voice Notes & AI Reports (Priority: P2)

**Goal**: Teachers log daily topics, capture voice notes, and auto-generate weekly/monthly reports for admin review.

- [ ] T071 [P] [US6] Define teaching log entity + attachments in `RateMyTeacher/Models/TeachingLog.cs` and register DbSet.
- [ ] T072 [US6] Create voice note storage/transcription model in `RateMyTeacher/Models/VoiceNote.cs`.
- [ ] T073 [US6] Integrate speech-to-text provider inside `RateMyTeacher/Services/VoiceNoteService.cs` with en/id/zh locales.
- [ ] T074 [US6] Build teaching log controller & views (`RateMyTeacher/Controllers/TeachingLogController.cs`, `Views/TeachingLog/*.cshtml`).
- [ ] T075 [US6] Implement file upload + tagging UX in `RateMyTeacher/wwwroot/js/teaching-log.js`.
- [ ] T076 [US6] Model report snapshots (`ReportSnapshot`) and register in `Data/ApplicationDbContext.cs`.
- [ ] T077 [US6] Implement report generation service + scheduled job in `RateMyTeacher/Services/ReportService.cs`.
- [ ] T078 [P] [US6] Create admin UI for downloading reports in `RateMyTeacher/Views/Reports/Index.cshtml`.
- [ ] T079 [US6] Hook generated reports into notification pipeline/email in `RateMyTeacher/Services/NotificationService.cs`.
- [ ] T080 [US6] Update `specs/main/plan.md` and `quickstart.md` with voice-note prerequisites + permissions.

---

## Phase 7A: User Story 7 – Anonymous Feedback, Badges & Improvement Programme (Priority: P3)

**Goal**: Provide safe student feedback channels, automatic recognition, and AI-driven improvement plans.

- [ ] T081 [P] [US7] Model anonymous feedback entity with moderation flags in `RateMyTeacher/Models/AnonymousFeedback.cs`.
- [ ] T082 [US7] Add moderation workflow endpoints to `RateMyTeacher/Controllers/FeedbackController.cs` with role checks.
- [ ] T083 [US7] Implement content moderation + sentiment scoring pipeline in `RateMyTeacher/Services/FeedbackService.cs`.
- [ ] T084 [P] [US7] Build student feedback UI (`Views/Feedback/Create.cshtml`) with constructive guidelines.
- [ ] T085 [US7] Define badge entities + award criteria in `RateMyTeacher/Models/Badge.cs` and `Models/TeacherBadge.cs`.
- [ ] T086 [US7] Implement badge evaluator job in `RateMyTeacher/Services/BadgeService.cs` using metrics (punctuality, engagement, etc.).
- [ ] T087 [US7] Display badges on teacher profile + leaderboard views (`Views/Teachers/Details.cshtml`, `Views/Admin/Leaderboard.cshtml`).
- [ ] T088 [US7] Build teacher improvement recommendation service in `RateMyTeacher/Services/ImprovementService.cs` leveraging AI insights.
- [ ] T089 [US7] Surface improvement roadmap UI in `RateMyTeacher/Views/Teachers/Improvement.cshtml`.
- [ ] T090 [US7] Extend documentation in `SPECIFICATION.md`/`plan.md` to include badge & improvement acceptance criteria.

---

## Phase 7B: User Story 8 – Unified Gradebook & Analytics (Priority: P3)

**Goal**: Deliver gradebook, cross-term trends, subject comparisons, and AI anomaly alerts for counselors/admins.

- [ ] T091 [P] [US8] Model gradebook entities (`Grade`, `GradeCategory`, `GradeLog`) in `RateMyTeacher/Models/Grade.cs` et al.
- [ ] T092 [US8] Register gradebook tables + historical indexes in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [ ] T093 [US8] Implement grade entry/import services in `RateMyTeacher/Services/GradebookService.cs` (CSV + manual entry).
- [ ] T094 [US8] Build gradebook controller/views for teachers and counselors (`Controllers/GradebookController.cs`, `Views/Gradebook/*.cshtml`).
- [ ] T095 [US8] Add cross-term trend and radar chart visualizations via `wwwroot/js/grade-analytics.js` using Chart.js.
- [ ] T096 [US8] Implement AI anomaly detection service in `RateMyTeacher/Services/GradeAlertService.cs` with configurable thresholds.
- [ ] T097 [US8] Send alerts/notifications when anomalies fire via `RateMyTeacher/Services/NotificationService.cs`.
- [ ] T098 [US8] Expose API endpoints for mobile/BI (`RateMyTeacher/Controllers/Api/GradesController.cs`).
- [ ] T099 [US8] Document data governance/FERPA considerations in `SPECIFICATION.md` and `plan.md`.
- [ ] T100 [US8] Seed demo gradebook data for QA scenarios in `RateMyTeacher/Data/Seed/SeedData.cs`.

---

## Final Phase: Polish & Cross-Cutting Concerns

**Purpose**: Apply consistency, performance, and documentation improvements across stories.

- [ ] T101 Create `RateMyTeacher.Tests/` project with foundational service tests and add solution reference.
- [ ] T102 Expand deployment and migration guidance in `RateMyTeacher/README.md` and `specs/main/quickstart.md`.
- [ ] T103 Run schema synchronization (migrations + `dotnet ef database update`) and capture results in `specs/main/plan.md` delivery notes.

---

## Dependencies & Execution Order

- **Phase 1 → Phase 2**: Setup must complete before foundational work to guarantee configuration coherence.
- **Phase 2 → Phase 2A/2B**: Localization and AI governance rely on base services but unlock all AI-driven stories (US3–US8).
- **User Story Sequence**: P1 stories (US1, US2) deliver MVP core and unblock attendance/feedback data. P2 stories (US3–US6) depend on AI controls + enrollment data. P3 stories (US7–US8) leverage sentiment, logs, and grades generated earlier.
- **Data Seeds**: Seeding tasks (T031, T058, T069, T100) follow entity creation to avoid migration drift.
- **Migrations**: Run and review migrations after each model-heavy phase (T035, T045, T053, T063, T071, T091, T103) to prevent conflicts.

---

## Parallel Execution Examples

1. After Phase 2B, Gemini integration tasks (T041–T050) can proceed in parallel with lesson planning work (T051–T060) because they touch different controllers/services.
2. Attendance service work (T061–T065) can run alongside dashboard UI/JS tasks (T067–T068) once models exist.
3. Feedback moderation (T081–T085) and badge automation (T086–T089) may advance concurrently; they touch separate code paths.
4. Gradebook analytics visualization (T095) can proceed while anomaly detection service (T096) is under development.

---

## Implementation Strategy

1. **Compliance First**: Finish Phase 2B AI governance before expanding Gemini/AI usage.
2. **MVP Core**: Keep P1 user stories stable and regression-tested while layering AI lesson experiences (Phase 5A/5B).
3. **Operational Excellence**: Deliver attendance, teaching logs, and reports (Phase 6A/6B) to support day-to-day workflows.
4. **Insights & Recognition**: Implement anonymous feedback, badges, and grade analytics (Phase 7A/7B) to fulfill the remaining SPEC pillars.
5. **Harden & Document**: Use Final Phase to add automated tests, README upgrades, and migration validation before release.
