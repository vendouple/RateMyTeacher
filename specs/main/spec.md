# RateMyTeacher - Feature Spec

Priority order: P1 highest

## Clarifications

### Session 2025-10-23

- Q: The spec uses both "term" and "semester" - are these the same concept or different? → A: Same thing - use "semester" consistently. Two semesters per year (6 months each).
- Q: What is the minimum vote threshold for teachers to qualify for rankings? → A: Minimum 10 ratings required to appear in rankings.
- Q: What authentication method should be used and how are user roles managed? → A: Email/password authentication with role-based access (Student/Teacher/Admin).
- Q: How should Gemini API failures be handled (rate limits, network issues, invalid API key)? → A: Show user-friendly error message and allow retry.
- Q: How should attendance be tracked for teachers and students? → A: Teacher attendance: Teachers mark themselves absent with reason for admin/headmaster approval (not visible to students until substitute arrives). Student attendance: Tracked per day (present for whole day unless teacher updates mid-day status to sick/out-of-class/competition). Students and teachers can view attendance records.
- Q: What permission/role system should be implemented? → A: Hierarchical system with default templates (Admin, Teacher, Student). Optional Department feature (can be disabled). Structure: Global Permissions → Optional Departments → Classes → Role Templates. Admins can create custom templates per department. Global Admins have full access. Teachers with "Create Class" permission become Class Admins. Class Admins can assign custom or template-based permissions to other teachers.
- Q: How should AI companion control work at different levels? → A: Three-level control: 1) Global Admin can disable AI completely for everyone system-wide, 2) Admin can disable AI per specific class, 3) Teacher can disable AI for their own class only. When disabled for a class, AI features are completely unavailable for that class context (students cannot use AI for that class's assignments, questions, or content), but students can still use AI for other classes where it's enabled.
- Q: What level of granularity should permission templates support? → A: Discord-style hierarchical permissions with expandable categories. Top-level categories (e.g., "Grades Management") can be quickly enabled/disabled, but can also be expanded to show granular sub-permissions (View, Edit, Delete). Admins can keep it simple (enable whole category) or customize deeply (select specific sub-permissions). Permission categories include: Grades, Attendance, Assignments, AI Companion, Class Settings, Students, Analytics, etc.

## Authentication & Authorization

- Email/password authentication required for all users
- Three user roles with distinct permissions:
  - **Student**: Can view teachers, submit ratings (once per teacher per semester), view own rating history
  - **Teacher**: Can view own ratings/comments, mark attendance, view schedule, request AI summaries
  - **Admin**: Can view all data, access leaderboards, calculate bonuses, view sentiment analysis
- Anonymous access not permitted
- Users must be logged in to access any feature

## US1 (P1) - Teacher listing and ratings

- As a student, I can view a list of teachers and rate a teacher 1-5 stars once per semester.
- Acceptance:
  - Student can submit a rating with optional comment
  - Average rating is calculated
  - One rating per student per teacher per semester enforced
  - Semester definition: 6-month periods, 2 per academic year

## US2 (P1) - Leaderboard & bonus calculation

- As admin, I can view teacher rankings per semester and distribute bonuses (1st $10, 2nd $5).
- As admin, I can configure bonus amounts and ranking positions dynamically without restarting the app.
- Acceptance:
  - Rankings computed correctly with minimum 10 ratings threshold per teacher
  - Bonus configuration stored in database settings table (not environment variables)
  - Admin can add/edit/remove bonus tiers (e.g., 1st place: $10, 2nd: $5, or extend to 10th-1st with custom amounts)
  - Support both range-based bonuses (e.g., 5th-10th place: $2) and single position bonuses (e.g., 1st place: $10)
  - Bonus calculation follows admin-configured rules
  - Minimum rating threshold also configurable by admin
  - Teachers with fewer than configured minimum ratings are excluded from rankings

## US3 (P2) - Lesson summary generation (AI)

- As a teacher, I can request an AI-generated lesson summary from lesson notes.
- Acceptance:
  - AI returns a structured summary under 300 words
  - Uses Gemini model via API key in .env
  - On API failure: Display user-friendly error message with retry option
  - Error messages do not expose technical details to users

## US4 (P2) - Attendance & basic schedule display

- As a teacher, I can mark my own attendance (absent with reason) for admin/headmaster approval.
- As a teacher, I can mark student attendance per day and update mid-day status if needed.
- As a teacher, I can see my schedule for the day.
- As a student, I can view my attendance record.
- Acceptance:
  - Teacher absence: Submit absence with reason, requires admin approval, not visible to students until substitute assigned
  - Student attendance: Default is present for whole day
  - Teachers can update student status mid-day: sick, out-of-class (competition/event), etc.
  - Schedule displays current and next class
  - Both students and teachers can view their own attendance history

## US5 (P3) - Feedback sentiment analysis

- As admin, I can see sentiment analysis of student comments for a teacher.
- Acceptance:
  - Sentiment labeled Positive/Neutral/Negative
  - Insights stored

## US6 (P3) - Student Dashboard & Learning Management

- As a student, I can access a comprehensive dashboard to manage my academic activities.
- Acceptance:
  - View upcoming homework/assignments with due dates
  - Upload assignment files (universal system replacing Google Classroom)
  - View grades, personal stats, and class averages per subject
  - See teacher notes for next class (e.g., "Read Module B pages 25-41")
  - Track reading assignments with completion status and comprehension checks
  - Access AI study companion that guides learning (shows answers or provides step-by-step guidance, never solves directly)
  - View calendar with all due dates and upcoming events
    - Upload and share notes with other students (gamified)
  - View scheduled extra classes/meetings (Google Meet, Zoom links)
  - See what's next, urgent assignments, and required pre-class reading
  - AI can explain concepts but won't solve homework problems directly
  - System tracks if students have completed and understood required readings

## US7 (P2) - Advanced Permission & Role Management System

- As a Global Admin, I can create and manage hierarchical permission system with granular control.
- As a Global Admin, I can enable/disable optional Department feature system-wide.
- As a Global Admin or Department Admin, I can create custom role templates with Discord-style permissions.
- As a Class Admin (teacher with create-class permission), I can assign roles and permissions to other teachers for my classes.
- Acceptance:
  - **Default Roles**: Admin, Teacher, Student templates exist by default
  - **Optional Departments**: Can be enabled/disabled globally (if disabled, structure is Global → Classes)
  - **Hierarchy** (when departments enabled): Global Permissions → Departments → Classes → Role Templates
  - **Global Admin**: Full system access, cannot be removed from any class/department, can manage all settings
  - **Class Creator**: Teacher with "Create Class" permission automatically becomes Class Admin for their created classes
  - **Permission Granularity**: Discord-style expandable categories
    - Top-level categories can be toggled as a whole (e.g., "Grades Management" → all grades permissions)
    - Categories can be expanded to show sub-permissions (View, Edit, Delete) for fine-grained control
    - Permission Categories: Grades (View/Edit/Delete), Attendance (View/Mark/Edit), Assignments (View/Create/Edit/Delete/Grade), AI Companion (Enable/Disable/Configure), Class Settings (Edit/Manage), Students (View/Add/Remove/Manage), Analytics (View/Export), Resources (View/Upload/Delete)
  - **Role Templates**: Admins can create reusable templates (e.g., "Chemistry Viewer", "Physics Editor")
  - **Template Assignment**: Templates can be department-specific or global
  - **Class-Level Control**: Class Admins can assign templates or custom permissions to teachers for their specific class
  - **AI Companion Control**: Three levels (Global disable, Per-class disable by admin, Per-class disable by teacher)
  - **Permission Override**: More specific permissions override broader ones (Class-level > Department-level > Global-level)
  - **Permission UI**: Collapsible/expandable interface for simple or detailed permission management

Optional docs: data-model.md, contracts/

```

```
