# Tasks: Application for Teacher (MVP)

**Input**: Design documents from `/specs/main/`
**Prerequisites**: plan.md (required), spec.md (required for user stories)

Tests are not explicitly requested; focus on implementation tasks that enable independent verification per user story.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish project scaffolding, configuration, and developer ergonomics.

- [ ] T001 Add required EF Core, SQLite, DotNetEnv, and Mscc.GenerativeAI package references in `RateMyTeacher/RateMyTeacher.csproj`.
- [ ] T002 Create `.env.example` with Gemini, database, and bonus configuration keys in `RateMyTeacher/.env.example`.
- [ ] T003 Update ignore rules for environment files and SQLite artifacts in `RateMyTeacher/.gitignore`.
- [ ] T004 Document project overview, environment setup, and spec references in `RateMyTeacher/README.md`.
- [ ] T005 Add placeholder documentation for data directory responsibilities in `RateMyTeacher/Data/README.md`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Provide core infrastructure all user stories depend on.

- [ ] T006 Create EF Core context with DbSets and configuration stubs in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [ ] T007 Configure dependency injection, DotNetEnv loading, and SQLite connection in `RateMyTeacher/Program.cs`.
- [ ] T008 [P] Implement base user aggregate with roles and metadata in `RateMyTeacher/Models/User.cs`.
- [ ] T009 [P] Implement teacher profile model with navigation properties in `RateMyTeacher/Models/Teacher.cs`.
- [ ] T010 [P] Implement student model including enrollment metadata in `RateMyTeacher/Models/Student.cs`.
- [ ] T011 Add design-time context factory to support migrations in `RateMyTeacher/Data/DesignTimeDbContextFactory.cs`.
- [ ] T012 Scaffold cookie-based authentication endpoints in `RateMyTeacher/Controllers/AccountController.cs` and associated views under `RateMyTeacher/Views/Account/`.
- [ ] T013 Refresh shared layout with navigation placeholders and theme toggle slot in `RateMyTeacher/Views/Shared/_Layout.cshtml`.

---

## Phase 3: User Story 1 - Teacher listing and ratings (Priority: P1) 🎯 MVP

**Goal**: Students can view teachers, submit a single rating per term, and see average scores.

**Independent Test**: Student submits rating via `/Teachers/Rate`; rating stored once per term; list shows updated average without affecting other stories.

- [ ] T014 [P] [US1] Define rating entity with validation attributes in `RateMyTeacher/Models/Rating.cs`.
- [ ] T015 [US1] Configure rating relationships, unique index `(StudentId, TeacherId, Semester)`, and seed hooks in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [ ] T016 [P] [US1] Declare rating contract in `RateMyTeacher/Services/IRatingService.cs`.
- [ ] T017 [US1] Implement rating logic (submit, list, averages) in `RateMyTeacher/Services/RatingService.cs`.
- [ ] T018 [P] [US1] Declare teacher retrieval contract in `RateMyTeacher/Services/ITeacherService.cs`.
- [ ] T019 [US1] Implement teacher listing with eager loading in `RateMyTeacher/Services/TeacherService.cs`.
- [ ] T020 [US1] Build MVC endpoints (Index, Details, Rate) in `RateMyTeacher/Controllers/TeachersController.cs`.
- [ ] T021 [P] [US1] Create teacher list UI with neuromorphic cards in `RateMyTeacher/Views/Teachers/Index.cshtml`.
- [ ] T022 [P] [US1] Create teacher detail view with rating summary in `RateMyTeacher/Views/Teachers/Details.cshtml`.
- [ ] T023 [P] [US1] Implement interactive rating widget and submission logic in `RateMyTeacher/wwwroot/js/rating.js`.
- [ ] T024 [US1] Seed sample teachers, students, and enrollments in `RateMyTeacher/Data/Seed/SeedData.cs` and wire execution at startup.
- [ ] T025 [US1] Register teacher and rating services plus MVC routes in `RateMyTeacher/Program.cs`.

---

## Phase 4: User Story 2 - Leaderboard & bonus calculation (Priority: P1)

**Goal**: Admins view semester rankings that respect vote thresholds and calculate bonuses.

**Independent Test**: Admin loads `/Admin/Leaderboard`, sees top teachers meeting minimum votes, and bonus amounts align with environment configuration.

- [ ] T026 [US2] Extend `RateMyTeacher/Services/RatingService.cs` with ranking aggregation filtered by semester and vote threshold.
- [ ] T027 [P] [US2] Create bonus payout entity in `RateMyTeacher/Models/Bonus.cs` with teacher linkage.
- [ ] T028 [US2] Register `Bonuses` DbSet and bonus constraints in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [ ] T029 [US2] Implement environment-driven payout logic in `RateMyTeacher/Services/BonusService.cs`.
- [ ] T030 [US2] Add admin leaderboard and bonus distribution actions in `RateMyTeacher/Controllers/AdminController.cs`.
- [ ] T031 [P] [US2] Build leaderboard view with rankings and payouts in `RateMyTeacher/Views/Admin/Leaderboard.cshtml`.
- [ ] T032 [US2] Capture bonus audit trail via EF migration artifacts under `RateMyTeacher/Migrations/`.

---

## Phase 5: User Story 3 - Lesson summary generation (Priority: P2)

**Goal**: Teachers request AI-generated summaries from lesson notes using Gemini.

**Independent Test**: Teacher submits lesson content on `/Lessons/Create`; receives <300 word summary stored for later reference.

- [ ] T033 [P] [US3] Define AI contract for summaries, quizzes, and questions in `RateMyTeacher/Services/IGeminiService.cs`.
- [ ] T034 [US3] Implement Gemini 2.5 Flash client with retry and timeout handling in `RateMyTeacher/Services/GeminiService.cs`.
- [ ] T035 [P] [US3] Model lesson metadata in `RateMyTeacher/Models/Lesson.cs`.
- [ ] T036 [P] [US3] Model lesson summaries with export flags in `RateMyTeacher/Models/LessonSummary.cs`.
- [ ] T037 [US3] Register lesson entities and storage configuration in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [ ] T038 [US3] Add create/summary endpoints to `RateMyTeacher/Controllers/LessonsController.cs` wiring Gemini responses.
- [ ] T039 [P] [US3] Build lesson authoring view with summary display in `RateMyTeacher/Views/Lessons/Create.cshtml`.
- [ ] T040 [P] [US3] Implement summary request client script in `RateMyTeacher/wwwroot/js/lesson-summary.js`.
- [ ] T041 [US3] Register Gemini service and related options in `RateMyTeacher/Program.cs`.
- [ ] T042 [US3] Persist generated summaries and errors via repository updates in `RateMyTeacher/Services/LessonService.cs`.

---

## Phase 6: User Story 4 - Attendance & basic schedule display (Priority: P2)

**Goal**: Teachers view today's schedule and mark attendance statuses quickly.

**Independent Test**: Teacher loads `/Schedule/Today`, sees current/next class, records attendance, and duplicate attendance entries are prevented.

- [ ] T043 [P] [US4] Create schedule entity with class timing metadata in `RateMyTeacher/Models/Schedule.cs`.
- [ ] T044 [P] [US4] Create attendance entity with status enum in `RateMyTeacher/Models/Attendance.cs`.
- [ ] T045 [US4] Configure schedule and attendance DbSets plus unique indexes in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [ ] T046 [US4] Add attendance management logic in `RateMyTeacher/Services/AttendanceService.cs`.
- [ ] T047 [US4] Implement schedule projection and attendance endpoints in `RateMyTeacher/Controllers/ScheduleController.cs`.
- [ ] T048 [US4] Expose quick mark and QR endpoints in `RateMyTeacher/Controllers/AttendanceController.cs`.
- [ ] T049 [P] [US4] Build schedule dashboard UI in `RateMyTeacher/Views/Schedule/Today.cshtml`.
- [ ] T050 [US4] Provide optional QR scanning handler in `RateMyTeacher/wwwroot/js/attendance.js`.
- [ ] T051 [US4] Seed sample schedules and class rosters in `RateMyTeacher/Data/Seed/SeedData.cs`.

---

## Phase 7: User Story 5 - Feedback sentiment analysis (Priority: P3)

**Goal**: Admins review sentiment trends and insights from student comments.

**Independent Test**: After ratings with comments exist, admin opens `/Admin/Sentiment` to see sentiment breakdown and insights stored per teacher.

- [ ] T052 [P] [US5] Define feedback entity linking ratings and sentiment in `RateMyTeacher/Models/Feedback.cs`.
- [ ] T053 [US5] Register feedback storage and cascade rules in `RateMyTeacher/Data/ApplicationDbContext.cs`.
- [ ] T054 [US5] Implement sentiment analysis wrapper leveraging Gemini in `RateMyTeacher/Services/SentimentService.cs`.
- [ ] T055 [US5] Trigger sentiment analysis during rating submission in `RateMyTeacher/Controllers/TeachersController.cs` and persist results.
- [ ] T056 [US5] Add sentiment aggregation endpoint to `RateMyTeacher/Controllers/AdminController.cs`.
- [ ] T057 [P] [US5] Create insights dashboard view in `RateMyTeacher/Views/Admin/Sentiment.cshtml` with Chart.js hooks.
- [ ] T058 [P] [US5] Build sentiment visualization script in `RateMyTeacher/wwwroot/js/sentiment.js`.
- [ ] T059 [US5] Produce EF migration capturing feedback tables under `RateMyTeacher/Migrations/`.

---

## Final Phase: Polish & Cross-Cutting Concerns

**Purpose**: Apply consistency, performance, and documentation improvements across stories.

- [ ] T060 Add localization resources and configure supported cultures in `RateMyTeacher/Resources/` and `RateMyTeacher/Program.cs`.
- [ ] T061 Introduce neuromorphic styling in `RateMyTeacher/wwwroot/css/neuromorphic.css` and link in `_Layout.cshtml`.
- [ ] T062 Implement theme toggle behavior in `RateMyTeacher/wwwroot/js/theme.js` and wire button in `_Layout.cshtml`.
- [ ] T063 Create `RateMyTeacher.Tests/` project with foundational service tests and add solution reference.
- [ ] T064 Expand deployment and migration guidance in `RateMyTeacher/README.md`.
- [ ] T065 Run schema synchronization (migrations + `dotnet ef database update`) and document outcomes in `RateMyTeacher/specs/main/plan.md` notes.

---

## Dependencies & Execution Order

- **Phase 1 → Phase 2**: Setup must complete before foundational work to guarantee configuration coherence.
- **Phase 2 → User Stories**: ApplicationDbContext, Program configuration, and authentication (T006–T013) unlock all user stories.
- **User Story Sequence**: P1 stories (US1, US2) deliver MVP core and should lead. P2 stories (US3, US4) rely on US1 data but can start once foundational tasks are done. P3 story (US5) depends on rating comments available after US1.
- **Data Seeds**: Seeding tasks (T024, T051) should follow corresponding entity implementations to avoid migration drift.
- **Migrations**: Tasks touching migrations (T032, T059, T065) should happen after entity changes; coordinate to avoid conflicts.

---

## Parallel Execution Examples

1. After Phase 2, tackle US1 models/services in parallel via tasks marked [P] (T014, T016, T018, T021, T022, T023).
2. While US1 controllers are in progress, another contributor can start T026–T029 for leaderboard logic independently.
3. Gemini integration tasks (T033–T040) can proceed concurrently with attendance modeling tasks (T043–T045) because they touch different areas.
4. Front-end enhancements (T061–T062) may run alongside test project setup (T063) once core stories stabilize.

---

## Implementation Strategy

1. **MVP First**: Complete Phase 1 and Phase 2, then finish US1 (Phase 3) to deliver the core student rating experience.
2. **Elevate Admin Value**: Implement US2 to unlock leaderboard payouts once ratings flow is validated.
3. **Layer AI Assistance**: Add US3 to showcase AI capabilities using Gemini, ensuring environment keys are configured early.
4. **Operational Tools**: Deliver US4 for daily teacher workflows followed by US5 analytics to round out insights.
5. **Polish & Harden**: Execute final phase for styling, localization, testing, and migration hygiene before release.
