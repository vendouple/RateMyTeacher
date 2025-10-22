# RateMyTeacher - Tasks (generated)

Feature: Application for Teacher (MVP)
Feature Dir: `specs/main`

## Phase 1 — Setup (project initialization)

- [x] T001 Create project skeleton: create ASP.NET Core 9 MVC project in `RateMyTeacher/` (Program.cs, Controllers/, Models/, Views/, wwwroot/)
- [x] T002 Add SQLite and EF Core packages to `RateMyTeacher/RateMyTeacher.csproj` (Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Tools, Microsoft.EntityFrameworkCore.Design)
- [x] T003 Add Google Gemini client and env loader to `RateMyTeacher/RateMyTeacher.csproj` (Mscc.GenerativeAI, DotNetEnv)
- [x] T004 Add `.env.example` to project root with GEMINI_API_KEY, GEMINI_MODEL, DATABASE_PATH, FIRST_PLACE_BONUS, SECOND_PLACE_BONUS, MINIMUM_VOTES_THRESHOLD
- [x] T005 Add `.gitignore` entries for `.env`, `Data/ratemyteacher.db`, `*.db-wal`, `*.db-shm`
- [x] T006 Create `specs/main/tasks.md` (this file) and reference in `README.md` at project root

## Phase 2 — Foundational (blocking prerequisites)

- [ ] T007 Create `Data/ApplicationDbContext.cs` with DbSets for Users, Teachers, Students, Ratings, Lessons, Attendance, Grades
- [ ] T008 Configure SQLite connection in `Program.cs` reading `DATABASE_PATH` from environment
- [ ] T009 Implement `Models/User.cs`, `Models/Teacher.cs`, `Models/Student.cs`, `Models/Rating.cs` in `RateMyTeacher/Models/`
- [ ] T010 Implement EF Core migrations initial create in `Migrations/` and seed minimal data (admin user, sample subjects) using `RateMyTeacher/Data/ApplicationDbContext.cs`
- [ ] T011 Add basic authentication scaffold (ASP.NET Identity or minimal cookie auth) in `Program.cs` and `Controllers/AccountController.cs`
- [ ] T012 Implement simple layout view `_Layout.cshtml` in `Views/Shared/` with theme toggle placeholder and link to `wwwroot/css/neuromorphic.css`

## Phase 3 — US1 (P1) - Teacher listing and ratings

Story goal: Students can view teachers and submit one rating per term.
Independent test criteria: Submit rating; rating stored; average recalculated; duplicate rating prevented.

- [ ] T013 [US1] Create `Controllers/TeachersController.cs` with `Index` and `Details` actions in `RateMyTeacher/Controllers/`
- [ ] T014 [US1] Create `Views/Teachers/Index.cshtml` and `Views/Teachers/Details.cshtml` to list teachers and show rating widget
- [ ] T015 [US1] Implement `Models/Rating.cs` in `RateMyTeacher/Models/Rating.cs` (StudentId, TeacherId, OverallRating, Comment, Semester, CreatedAt)
- [ ] T016 [US1] Implement `Services/IRatingService.cs` and `Services/RatingService.cs` with `SubmitRatingAsync`, `GetTeacherRatingsAsync`, `GetTeacherAverageAsync` in `RateMyTeacher/Services/`
- [ ] T017 [US1] Add server-side validation to enforce one rating per student per semester in `RatingService` and DB unique index in `ApplicationDbContext` (`StudentId, TeacherId, Semester`) in `RateMyTeacher/Data/ApplicationDbContext.cs`
- [ ] T018 [US1] Add client-side JS rating widget (`wwwroot/js/rating.js`) to POST rating to `/Teachers/Rate` endpoint
- [ ] T019 [US1] Create `Controllers/TeachersController.Rate` POST endpoint in `RateMyTeacher/Controllers/TeachersController.cs` to accept ratings and redirect to Details
- [ ] T020 [US1] Add average rating display field to `Views/Teachers/Details.cshtml` and compute via `RatingService.GetTeacherAverageAsync`

## Phase 4 — US2 (P1) - Leaderboard & bonus calculation

Story goal: Admin views rankings and bonuses calculated per semester.
Independent test criteria: Rankings show top teachers; bonus calculation respects min threshold and business rules.

- [ ] T021 [US2] Implement `Services/IRatingService.GetTeacherRankingsAsync(semester)` in `RateMyTeacher/Services/RatingService.cs`
- [ ] T022 [US2] Create `Controllers/AdminController.cs` with `Leaderboard` action in `RateMyTeacher/Controllers/`
- [ ] T023 [US2] Create `Views/Admin/Leaderboard.cshtml` to display rankings and bonus amounts
- [ ] T024 [US2] Implement bonus calculation logic in `Services/BonusService.cs` reading `FIRST_PLACE_BONUS`, `SECOND_PLACE_BONUS`, `MINIMUM_VOTES_THRESHOLD` from environment
- [ ] T025 [US2] Add an admin-only endpoint `POST /Admin/DistributeBonuses` in `Controllers/AdminController.cs` which records bonuses in `Models/Bonus.cs` in `RateMyTeacher/Models/`
- [ ] T026 [US2] Create DB migration to add `Bonuses` table and audit trail in `Migrations/`

## Phase 5 — US3 (P2) - Lesson summary generation (AI)

Story goal: Teachers request an AI summary of lesson notes using Gemini.
Independent test criteria: API call uses `GEMINI_API_KEY`; returns summary <300 words and saved.

- [ ] T027 [US3] Add `.env.example` entry for `GEMINI_API_KEY` and `GEMINI_MODEL` (if not present) in project root
- [ ] T028 [US3] Create `Services/IGeminiService.cs` and `Services/GeminiService.cs` to wrap Gemini calls in `RateMyTeacher/Services/`
- [ ] T029 [US3] Implement `Controllers/LessonsController.cs` POST action `GenerateSummary` at `RateMyTeacher/Controllers/LessonsController.cs` which calls `GeminiService.GenerateLessonSummary`
- [ ] T030 [US3] Create client-side JS `wwwroot/js/lesson-summary.js` that sends lesson notes to `/Lessons/GenerateSummary` and displays returned summary
- [ ] T031 [US3] Store generated summary in `Models/LessonSummary.cs` and `Data/ApplicationDbContext.cs` DbSet `LessonSummaries`
- [ ] T032 [US3] Add error handling and rate-limiting wrapper around Gemini calls in `GeminiService` to avoid hitting free-tier limits

## Phase 6 — US4 (P2) - Attendance & basic schedule display

Story goal: Teachers mark attendance and view today's schedule.
Independent test criteria: Attendance entries created and schedule shows current/next class.

- [ ] T033 [US4] Create `Models/Schedule.cs` and `Models/Attendance.cs` in `RateMyTeacher/Models/`
- [ ] T034 [US4] Implement `Controllers/ScheduleController.cs` with `Today` action showing today's schedule in `RateMyTeacher/Controllers/`
- [ ] T035 [US4] Create `Views/Schedule/Today.cshtml` to display current/next class and quick attendance buttons
- [ ] T036 [US4] Implement `Services/IAttendanceService.cs` and `Services/AttendanceService.cs` to mark attendance and enforce uniqueness (`StudentId, LessonId`) in `RateMyTeacher/Services/`
- [ ] T037 [US4] Add QR-code attendance endpoint `Controllers/AttendanceController.cs` POST `Scan` to mark attendance (optional)

## Phase 7 — US5 (P3) - Feedback sentiment analysis

Story goal: Admin views sentiment analysis of feedback comments.
Independent test criteria: Feedback sentiment labeled and stored; admin can view aggregated sentiment.

- [ ] T038 [US5] Implement `Services/SentimentService.cs` which calls Gemini for sentiment (or use simple rule-based fallback) in `RateMyTeacher/Services/`
- [ ] T039 [US5] On rating submission (T019), call `SentimentService.AnalyzeFeedbackSentiment` when comment present and persist result in `Feedback` table (`Sentiment`, `Themes`, `Insights`) in `RateMyTeacher/Models/Feedback.cs`
- [ ] T040 [US5] Create `Controllers/AdminController.Sentiment` action to aggregate sentiment per teacher in `RateMyTeacher/Controllers/AdminController.cs`
- [ ] T041 [US5] Create `Views/Admin/Sentiment.cshtml` to display sentiment breakdown and word cloud visualization (use Chart.js or a simple word cloud lib)

## Final Phase — Polish & Cross-Cutting Concerns

- [ ] T042 Add localization resource files for English in `Resources/` and wire ASP.NET Core localization in `Program.cs`
- [ ] T043 Implement neuromorphic CSS in `wwwroot/css/neuromorphic.css` and include dark/light toggle JS in `wwwroot/js/theme.js`
- [ ] T044 Add unit test project `RateMyTeacher.Tests` and include sample tests for `RatingService` and `GeminiService` in `RateMyTeacher.Tests/`
- [ ] T045 Add README.md with setup instructions and `.env.example` guidance at project root
- [ ] T046 Run `dotnet ef migrations add InitialCreate` and `dotnet ef database update` to create DB schema
- [ ] T047 Verify all checklist tasks follow the required format and update `specs/main/tasks.md` if any formatting errors remain

## Dependencies (story completion order)

1. Phase 1 (T001-T006) must complete before Phase 2 (T007-T012).
2. Phase 2 (T007-T012) must complete before US1 (T013-T020) and US2 (T021-T026).
3. US1 should be completed before US2 (ratings required for rankings).
4. US3 (AI) depends on Phase 1 & 2 but can be developed in parallel with US4 and US5 after foundational setup.

## Parallel execution examples

- [ ] Example P001: Implement `Models/` (T009, T015, T033) in parallel across team members as they are independent file additions
- [ ] Example P002: Work on `GeminiService.cs` (T028) while `LessonsController` UI (T030) is developed in parallel

## Implementation strategy

- MVP: Focus on US1 (Teacher listing and ratings) as the deliverable for the demo
- Deliver US2 (Leaderboard) after US1 stabilization
- Add US3-US5 as incremental features after core flows are stable

---

Generated: automatic
