# RateMyTeacher - Feature Spec

Priority order: P1 highest. This document must remain in lock-step with `SPECIFICATION.md`, `IMPLEMENTATION_PLAN.md`, and `PRINCIPLES.md`.

## Clarifications

### Session 2025-11-18

- Roles are now limited to **Admin, Teacher, Student**. No custom templates, departments, or parent-facing roles exist. Admin subsumes every management responsibility (including former department head/principal/parent workflows).
- Authorization is linear: Admin > Teacher > Student. Future requirements referencing department heads or parents should be interpreted as Admin access.
- AI governance shrank to a single global enable switch plus Teacher AI Mode (Unrestricted/Guided/Off) and Student AI Mode (Learning/Unrestricted/Off). Scoped overrides were removed.
- Neuromorphic styling is deferred; keep Bootstrap defaults tidy and ensure accessibility basics (contrast, keyboard focus) remain.
- Password resets are Admin-driven with a `MustChangePassword` enforcement loop after temporary credentials are issued.

### Session 2025-10-23

- "Term" and "semester" refer to the same 6‑month period; use "semester" everywhere.
- Teachers require at least 10 ratings to appear in rankings unless admins raise the threshold via settings.
- Email/password authentication with role-based access (Student/Teacher/Admin). Anonymous access is disallowed.
- Gemini API failures must show user-friendly retries. Never leak stack traces.
- Attendance: Teachers self-report absences for admin approval, while student attendance defaults to present unless updated mid-day (sick/out-of-class/competition). Students and teachers can read their own records.
- Permission system: previously described Discord-style hierarchy is deprecated. The build now enforces only the three fixed roles with Admin as the sole management persona.
- AI control: replaced with the simplified global toggle + role-based modes described in Session 2025-11-18.
- Permission categories include Grades, Attendance, Assignments, AI Companion, Class Settings, Students, Analytics, Resources, and future LMS items.

### Session 2025-02-14

- Attendance filters must support date range + status filters and display a "No attendance record" card when empty.
- AI usage logs must track whether the student viewed the response, store a timestamp, and scope visibility (Admin: all, Teacher: their classes, Student: personal).
- Bonus tier inputs must validate non-overlapping ranges and ensure `startRank ≤ endRank`.
- Lesson summaries must ship exactly four sections—**Main Topics**, **Key Concepts**, **Important Takeaways**, **Study Tips**—each as bullets, capped at 300 words before the Gemini call.
- Teacher-submitted absences are hidden from students/parents until a substitute is assigned, but admins/headmasters can review immediately.
- Schedule automation requires reminders 15 minutes before class and conflict detection when assigning substitutes.

## Non-Functional Requirements

- **Performance**: MVC actions must render within 200 ms p95; permission checks <100 ms. Gemini calls must time out within 30 seconds including retries.
- **Security & Audit**: Enforce authentication on every endpoint, multi-role authorization, and audit trails for ratings, attendance edits, AI usage, bonus payouts, permission updates, and grade changes.
- **Accessibility & UI Baseline**: Focus on clean Bootstrap layouts with ARIA labels, keyboard focus states, and prefers-reduced-motion handling. Neuromorphic polish is deferred; prioritize clarity over custom theming.
- **Internationalization**: Use ASP.NET Core localization with resources per controller/view, supporting `en-US`, `id-ID`, `zh-CN` initially and persisting language preference in cookies.
- **AI Transparency**: Always show AI mode indicators (Explain/Guide/Show Answer), log AI usage for 90 days, and block homework-answer requests.
- **Notifications & Scheduling**: Provide reminders 15 minutes prior to class, parent notifications for absences, and substitution alerts.
- **Data Export**: Support PDF/Word exports for lesson summaries, CSV/Excel for attendance/grades, and JSON snapshot endpoints for leaderboards with checksums.

## Edge Cases & Scenarios

- Duplicate ratings or missing enrollments must block submission and hide the CTA, surfacing actionable errors.
- Attendance gaps show a "No attendance record" tile instead of blank space.
- Substitution conflicts warn admins and require explicit confirmation.
- Bonus ties split payouts per tier definition and store the calculation checksum for replay.
- AI failures prompt users to trim content or retry, preserving drafts locally.
- Theme preferences fall back to system defaults if corrupt.

## Authentication & Authorization

- Email/password auth with exactly one role per user (Admin, Teacher, or Student).
- Admin is a strict superset role that owns every management action formerly tied to department heads, principals, or parents.
- All pages require login; Admin accounts cannot be deleted or demoted from within the UI.
- Authorization checks now compare against these three roles only; no scope or hierarchy overrides remain.

## US1 (P1) – Teacher listing and ratings

- Students can browse teachers, rate them 1‑5 stars (with comments) once per semester.
- Acceptance: enforce enrollment-based eligibility, unique `(Student, Teacher, Semester)` constraint, minimum rating threshold enforcement, and clean Bootstrap cards for teacher listings with localization-ready copy.

## US2 (P1) – Leaderboard & bonus calculation

- Admins view rankings per semester and distribute bonuses configurable at runtime (position and range tiers, currency, split rules).
- Acceptance: rankings honor minimum vote thresholds, store snapshot checksums, bonus config lives in database settings (not env vars), CRUD UI exists, range tiers and single tiers coexist, payouts follow admin rules, and audit entries capture who awarded bonuses.

## US3 (P2) – Lesson summary generation (AI)

- Teachers request AI summaries from lesson notes.
- Acceptance: enforce four-section order with pre-validation under 300 words, allow PDF/Word export and archival, display friendly retries, respect AI controls, and log each request.

## US4 (P2) – Attendance & schedule experience

- Teachers mark their own absences, record student attendance, and see schedules. Students view their attendance.
- Acceptance: admin-approved teacher absence workflow with substitutions, parent notifications upon student absence, mid-day status updates, date/status filters, "No record" hints, conflict detection, and straightforward Bootstrap schedule cards showing current/next class with time-to-start countdowns.

## US5 (P3) – Feedback sentiment analysis

- Admins review sentiment trends across ratings/comments.
- Acceptance: sentiment labeled Positive/Neutral/Negative, actionable insights persisted, Chart.js dashboards with filters/export, and hooks into teacher improvement recommendations.

## US6 (P3) – Student dashboard & LMS

- Unified dashboard replacing fragmented tools.
- Acceptance: assignments with uploads, gradebook view, teacher notes, reading assignment tracking with comprehension checks, AI companion (explain/guide), calendar, gamified note sharing, extra classes (Zoom/Meet links), urgency indicators, recognition badges, and AI-generated nudges summarizing weekly progress.

## US7 (P2) – Simplified user management & password resets

- Admin-focused workflow to add teachers/students, reset passwords, and enforce the `MustChangePassword` gate before users access the app.
- Acceptance: only Admin/Teacher/Student roles appear in the UI, role changes audit who made the update, password resets issue temporary credentials + flags, and navigation stays consistent with the simplified hierarchy.

## US8 (P2) – Lesson & schedule automation

- Teachers see timetable with reminders; admins manage substitutions and detect conflicts.
- Acceptance: real-time "Now/Next" display, 15-minute reminders (email/push), Google/Outlook calendar sync, substitution workflow that reassigns schedules plus notifications, and conflict resolution tooling.

## US9 (P3) – Teaching log & feedback

- Teachers log topics/materials, attach files, and optionally dictate voice notes; students submit quick feedback.
- Acceptance: searchable logs with tagging, attachment storage, voice-to-text with multi-language support, anonymous feedback with moderation, trend reports comparing curriculum plans vs. actual coverage, and immediate teacher notifications for critical feedback.

## US10 (P3) – AI summary & weekly reports

- Admins/teachers generate weekly/monthly AI-assisted reports summarizing performance.
- Acceptance: scheduled job aggregates metrics, Gemini produces insights (Performance Summary, Key Achievements, Areas for Improvement, Engagement Analysis, Recommendations), outputs PDF/Excel, emails stakeholders, and archives reports with rerun capability.

## Personas

- **Student**: Consumes dashboards, submits ratings/feedback, uploads assignments, interacts with AI companion, views attendance.
- **Teacher**: Manages classes, attendance, lesson notes, AI summaries, responds to feedback, and views badges/reports.
- **Admin/Department Head**: Oversees leaderboards, permission system, AI controls, attendance workflows, reports, and global settings.

Optional docs: `data-model.md`, `contracts/`
