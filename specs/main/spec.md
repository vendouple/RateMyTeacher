# RateMyTeacher - Feature Spec

Priority order: P1 highest. This document must remain in lock-step with `SPECIFICATION.md`, `IMPLEMENTATION_PLAN.md`, and `PRINCIPLES.md`.

## Clarifications

### Session 2025-10-23

- "Term" and "semester" refer to the same 6‑month period; use "semester" everywhere.
- Teachers require at least 10 ratings to appear in rankings unless admins raise the threshold via settings.
- Email/password authentication with role-based access (Student/Teacher/Admin). Anonymous access is disallowed.
- Gemini API failures must show user-friendly retries. Never leak stack traces.
- Attendance: Teachers self-report absences for admin approval, while student attendance defaults to present unless updated mid-day (sick/out-of-class/competition). Students and teachers can read their own records.
- Permission system: Discord-style hierarchy (Global → Department → Class) with default Admin/Teacher/Student templates. Teachers with "Create Class" become Class Admins and can delegate within their classes.
- AI control: Three-level disable switches (Global Admin, Class Admin, Teacher). If disabled for a class, every AI entrypoint (summaries, companion) is hidden for that class context but available elsewhere.
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
- **Accessibility & Neuromorphic Design**: Implement the design language in `PRINCIPLES.md` with dark/light parity, ARIA labels, keyboard focus states, and prefers-reduced-motion handling. The layout must not show the stock ASP.NET template.
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

- Email/password auth with multi-role assignments per the constitution.
- Roles: Student, Teacher, Admin, plus custom templates.
- All pages require login; global admins cannot be removed.
- RBAC must respect scope ordering (Global → Department → Class) with overrides.

## US1 (P1) – Teacher listing and ratings

- Students can browse teachers, rate them 1‑5 stars (with comments) once per semester.
- Acceptance: enforce enrollment-based eligibility, unique `(Student, Teacher, Semester)` constraint, minimum rating threshold enforcement, and neuromorphic cards for teacher listings with localization-ready copy.

## US2 (P1) – Leaderboard & bonus calculation

- Admins view rankings per semester and distribute bonuses configurable at runtime (position and range tiers, currency, split rules).
- Acceptance: rankings honor minimum vote thresholds, store snapshot checksums, bonus config lives in database settings (not env vars), CRUD UI exists, range tiers and single tiers coexist, payouts follow admin rules, and audit entries capture who awarded bonuses.

## US3 (P2) – Lesson summary generation (AI)

- Teachers request AI summaries from lesson notes.
- Acceptance: enforce four-section order with pre-validation under 300 words, allow PDF/Word export and archival, display friendly retries, respect AI controls, and log each request.

## US4 (P2) – Attendance & schedule experience

- Teachers mark their own absences, record student attendance, and see schedules. Students view their attendance.
- Acceptance: admin-approved teacher absence workflow with substitutions, parent notifications upon student absence, mid-day status updates, date/status filters, "No record" hints, conflict detection, and neuromorphic schedule cards showing current/next class with time-to-start countdowns.

## US5 (P3) – Feedback sentiment analysis

- Admins review sentiment trends across ratings/comments.
- Acceptance: sentiment labeled Positive/Neutral/Negative, actionable insights persisted, Chart.js dashboards with filters/export, and hooks into teacher improvement recommendations.

## US6 (P3) – Student dashboard & LMS

- Unified dashboard replacing fragmented tools.
- Acceptance: assignments with uploads, gradebook view, teacher notes, reading assignment tracking with comprehension checks, AI companion (explain/guide), calendar, gamified note sharing, extra classes (Zoom/Meet links), urgency indicators, recognition badges, and AI-generated nudges summarizing weekly progress.

## US7 (P2) – Advanced permission & role management

- Discord-style permission system with multi-role assignments.
- Acceptance: default role templates, optional departments, rank-based hierarchy, expandable categories/sub-permissions, template CRUD, class-level overrides, AI control toggles, audit logging for every change, localization-ready permission UI, and analytics on role usage.

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
