# Data Model: RateMyTeacher

**Version**: 1.0.0  
**Date**: 2025-10-23  
**Database**: SQLite (via Entity Framework Core 9)

---

## Entity Relationship Overview

```
User (base) ──┬── Teacher ──┬── Rating ──── Semester
              │             ├── TeacherAbsence
              │             ├── Schedule
              │             └── AISummary
              │
              ├── Student ──┬── Rating
              │             ├── Attendance
              │             ├── AssignmentSubmission
              │             └── StudentNote
              │
              └── Admin

Department (hierarchical) ──── Class ──── Schedule

Role ──── RolePermission ──── Permission ──── PermissionCategory (hierarchical)
  │
  └── UserRole ──── User

BonusConfig ──── BonusTier
             └── TeacherRanking
```

---

## Phase 1 (P1) Entities - Core Rating System

### User (Base Entity)

**Purpose**: Base class for all user types (Student, Teacher, Admin)

| Field        | Type        | Constraints               | Notes                     |
| ------------ | ----------- | ------------------------- | ------------------------- |
| Id           | int         | PK, Identity              |                           |
| Email        | string(255) | Unique, Required, Index   | Authentication identifier |
| PasswordHash | string(500) | Required                  | Hashed with bcrypt        |
| FirstName    | string(100) | Required                  |                           |
| LastName     | string(100) | Required                  |                           |
| Role         | enum        | Required                  | Student/Teacher/Admin     |
| CreatedAt    | DateTime    | Required, Default: UtcNow |                           |
| IsDeleted    | bool        | Default: false            | Soft delete flag          |
| DeletedAt    | DateTime?   | Nullable                  | When soft deleted         |
| DeletedBy    | int?        | FK to User, Nullable      | Who deleted this user     |

**Validation Rules:**

- Email must be valid format (regex: `^[^@]+@[^@]+\.[^@]+$`)
- PasswordHash minimum 60 characters (bcrypt output)
- FirstName and LastName: 2-100 characters, letters and spaces only

**State Transitions:**

- Created → Active (default)
- Active → Deleted (soft delete)
- Deleted → Active (restore, admin only)

**Indexes:**

- `IX_User_Email` (unique)
- `IX_User_IsDeleted` (filter queries)

---

### Teacher (Extends User)

**Purpose**: Teacher-specific profile data

| Field        | Type         | Constraints    | Notes                                                         |
| ------------ | ------------ | -------------- | ------------------------------------------------------------- |
| Id           | int          | PK, FK to User | Shared primary key inheritance                                |
| Bio          | string(1000) | Nullable       | Teacher description                                           |
| ProfileImage | string(500)  | Nullable       | URL to profile picture                                        |
| HireDate     | DateTime     | Required       |                                                               |
| Department   | string(100)  | Nullable       | E.g., "Chemistry", "Mathematics" (Phase 1 string, Phase 2 FK) |

**Relationships:**

- One-to-Many with Rating (TeacherId)
- One-to-Many with Schedule (TeacherId)
- One-to-Many with AISummary (TeacherId)
- One-to-Many with TeacherAbsence (TeacherId)

**Validation Rules:**

- Bio: Maximum 1000 characters
- HireDate: Cannot be in the future

---

### Student (Extends User)

**Purpose**: Student-specific profile data

| Field          | Type       | Constraints    | Notes                          |
| -------------- | ---------- | -------------- | ------------------------------ |
| Id             | int        | PK, FK to User | Shared primary key inheritance |
| GradeLevel     | int        | Required       | 1-12 (or 9-12 for high school) |
| ParentContact  | string(15) | Nullable       | Phone number for emergencies   |
| EnrollmentDate | DateTime   | Required       |                                |

**Relationships:**

- One-to-Many with Rating (StudentId)
- One-to-Many with Attendance (StudentId)
- One-to-Many with AssignmentSubmission (StudentId)
- One-to-Many with StudentNote (StudentId)

**Validation Rules:**

- GradeLevel: 1-12
- ParentContact: Regex `^\+?[0-9]{10,15}$` (international format)

---

### Semester

**Purpose**: Academic period (6 months, 2 per year)

| Field        | Type       | Constraints      | Notes                            |
| ------------ | ---------- | ---------------- | -------------------------------- |
| Id           | int        | PK, Identity     |                                  |
| Name         | string(50) | Required, Unique | E.g., "Fall 2024", "Spring 2025" |
| StartDate    | Date       | Required         |                                  |
| EndDate      | Date       | Required         |                                  |
| AcademicYear | string(10) | Required         | E.g., "2024-2025"                |
| IsCurrent    | bool       | Default: false   | Only one semester can be current |

**Relationships:**

- One-to-Many with Rating (SemesterId)
- One-to-Many with TeacherRanking (SemesterId)

**Validation Rules:**

- EndDate > StartDate
- Duration: 5-7 months (150-210 days)
- Only one semester with IsCurrent = true (database trigger or application logic)

**Indexes:**

- `IX_Semester_IsCurrent` (for current semester queries)
- `IX_Semester_AcademicYear` (for annual reports)

---

### Rating

**Purpose**: Student ratings of teachers (1-5 stars)

| Field      | Type        | Constraints               | Notes                     |
| ---------- | ----------- | ------------------------- | ------------------------- |
| Id         | int         | PK, Identity              |                           |
| StudentId  | int         | FK to Student, Required   |                           |
| TeacherId  | int         | FK to Teacher, Required   |                           |
| SemesterId | int         | FK to Semester, Required  |                           |
| Stars      | int         | Required, Range: 1-5      |                           |
| Comment    | string(500) | Nullable                  | Optional written feedback |
| CreatedAt  | DateTime    | Required, Default: UtcNow |                           |
| UpdatedAt  | DateTime?   | Nullable                  | If student edits rating   |

**Unique Constraint**: `(StudentId, TeacherId, SemesterId)` - One rating per student per teacher per semester

**Relationships:**

- Many-to-One with Student (StudentId)
- Many-to-One with Teacher (TeacherId)
- Many-to-One with Semester (SemesterId)
- One-to-One with SentimentAnalysis (RatingId)

**Validation Rules:**

- Stars: 1-5 (inclusive)
- Comment: Maximum 500 characters, profanity filter (future)
- Student cannot rate themselves (if student is also teacher)

**Indexes:**

- `IX_Rating_TeacherId_SemesterId` (for leaderboard calculations)
- `IX_Rating_StudentId` (for student rating history)
- `UX_Rating_StudentTeacherSemester` (unique constraint)

---

### BonusConfig

**Purpose**: Configurable bonus settings (singleton or per-semester)

| Field                   | Type     | Constraints               | Notes                                   |
| ----------------------- | -------- | ------------------------- | --------------------------------------- |
| Id                      | int      | PK, Identity              |                                         |
| MinimumRatingsThreshold | int      | Required, Default: 10     | Minimum ratings to qualify for rankings |
| CreatedAt               | DateTime | Required, Default: UtcNow |                                         |
| ModifiedAt              | DateTime | Required, Default: UtcNow | Updated on each edit                    |
| ModifiedBy              | int      | FK to User, Nullable      | Admin who last modified                 |

**Relationships:**

- One-to-Many with BonusTier (ConfigId)

**Validation Rules:**

- MinimumRatingsThreshold: 1-100 (reasonable range)

**Notes:**

- Typically one active config at a time (latest by CreatedAt)
- Historical configs retained for auditing

---

### BonusTier

**Purpose**: Bonus amounts for specific ranks or rank ranges

| Field      | Type          | Constraints                 | Notes                                                          |
| ---------- | ------------- | --------------------------- | -------------------------------------------------------------- |
| Id         | int           | PK, Identity                |                                                                |
| ConfigId   | int           | FK to BonusConfig, Required |                                                                |
| Position   | int?          | Nullable                    | Exact position (e.g., 1 for 1st place), NULL if range-based    |
| RangeStart | int?          | Nullable                    | Start of range (e.g., 5 for 5th-10th), NULL if single position |
| RangeEnd   | int?          | Nullable                    | End of range (e.g., 10 for 5th-10th), NULL if single position  |
| Amount     | decimal(10,2) | Required, >= 0              | Bonus amount in dollars                                        |

**Relationships:**

- Many-to-One with BonusConfig (ConfigId)

**Validation Rules:**

- Either Position is NOT NULL OR (RangeStart AND RangeEnd are NOT NULL)
- If range-based: RangeEnd >= RangeStart
- Amount >= 0

**Examples:**

- Single position: `Position=1, RangeStart=NULL, RangeEnd=NULL, Amount=10.00` → 1st place: $10
- Range: `Position=NULL, RangeStart=5, RangeEnd=10, Amount=2.00` → 5th-10th place: $2 each

---

### TeacherRanking

**Purpose**: Computed rankings per semester (materialized for performance)

| Field         | Type          | Constraints               | Notes                      |
| ------------- | ------------- | ------------------------- | -------------------------- |
| Id            | int           | PK, Identity              |                            |
| TeacherId     | int           | FK to Teacher, Required   |                            |
| SemesterId    | int           | FK to Semester, Required  |                            |
| Rank          | int           | Required                  | 1 = highest average rating |
| AverageRating | decimal(3,2)  | Required                  | E.g., 4.75                 |
| TotalRatings  | int           | Required                  | Number of ratings received |
| BonusAmount   | decimal(10,2) | Required, >= 0            | Calculated bonus           |
| CalculatedAt  | DateTime      | Required, Default: UtcNow | When ranking was computed  |

**Unique Constraint**: `(TeacherId, SemesterId)` - One ranking per teacher per semester

**Relationships:**

- Many-to-One with Teacher (TeacherId)
- Many-to-One with Semester (SemesterId)

**Indexes:**

- `IX_TeacherRanking_SemesterId_Rank` (for leaderboard display)
- `UX_TeacherRanking_TeacherSemester` (unique constraint)

**Notes:**

- Recalculated periodically (e.g., nightly job) or on-demand
- Only teachers with >= MinimumRatingsThreshold appear here

---

### AISummary

**Purpose**: AI-generated lesson summaries (Phase 1, expanded in P3)

| Field       | Type       | Constraints                                    | Notes                                        |
| ----------- | ---------- | ---------------------------------------------- | -------------------------------------------- |
| Id          | int        | PK, Identity                                   |                                              |
| TeacherId   | int        | FK to Teacher, Required                        |                                              |
| ClassId     | int?       | FK to Class, Nullable (P1: NULL, P2: Required) |                                              |
| LessonNotes | text       | Required                                       | Original teacher notes (input)               |
| Summary     | text       | Required                                       | AI-generated summary (output, max 300 words) |
| GeneratedAt | DateTime   | Required, Default: UtcNow                      |                                              |
| Model       | string(50) | Required, Default: "gemini-2.0-flash-exp"      | AI model version                             |

**Relationships:**

- Many-to-One with Teacher (TeacherId)
- Many-to-One with Class (ClassId, P2+)

**Validation Rules:**

- LessonNotes: 50-10,000 characters (minimum viable input)
- Summary: Maximum 500 characters (~300 words)

**Indexes:**

- `IX_AISummary_TeacherId` (for teacher's summary history)

---

## Phase 2 (P2) Entities - Permissions & Attendance

### Department (Hierarchical)

**Purpose**: Optional organizational unit (can be disabled globally)

| Field              | Type        | Constraints                | Notes                                      |
| ------------------ | ----------- | -------------------------- | ------------------------------------------ |
| Id                 | int         | PK, Identity               |                                            |
| Name               | string(100) | Required, Unique           | E.g., "Science", "Mathematics"             |
| ParentDepartmentId | int?        | FK to Department, Nullable | For nested departments                     |
| IsEnabled          | bool        | Default: true              | Department feature toggle (global setting) |
| IsDeleted          | bool        | Default: false             | Soft delete                                |
| DeletedAt          | DateTime?   | Nullable                   |                                            |
| DeletedBy          | int?        | FK to User, Nullable       |                                            |

**Relationships:**

- Self-referential: One-to-Many (ParentDepartmentId)
- One-to-Many with Class (DepartmentId)

**Validation Rules:**

- No circular references (ParentDepartmentId != Id)
- Maximum depth: 3 levels (e.g., Science → Chemistry → Organic Chemistry)

**Indexes:**

- `IX_Department_ParentDepartmentId` (for hierarchy queries)
- `IX_Department_IsDeleted` (filter soft deletes)

---

### PermissionCategory (Hierarchical)

**Purpose**: Hierarchical permission grouping (Discord-style)

| Field            | Type        | Constraints                        | Notes                                    |
| ---------------- | ----------- | ---------------------------------- | ---------------------------------------- |
| Id               | int         | PK, Identity                       |                                          |
| Name             | string(100) | Required                           | E.g., "Grades Management", "View Grades" |
| Code             | string(50)  | Required, Unique                   | E.g., "grades_management", "grades_view" |
| ParentCategoryId | int?        | FK to PermissionCategory, Nullable | For nested categories                    |
| IsExpandable     | bool        | Default: true                      | Can be expanded to show sub-permissions  |
| DisplayOrder     | int         | Required, Default: 0               | UI sort order                            |

**Relationships:**

- Self-referential: One-to-Many (ParentCategoryId)
- One-to-Many with Permission (CategoryId)

**Validation Rules:**

- Maximum depth: 2 levels (Category → Sub-Permission)
- Code: Lowercase, underscores only (regex: `^[a-z_]+$`)

**Examples:**

- Top-level: "Grades Management" (IsExpandable=true)
  - Sub: "View Grades" (ParentCategoryId = Grades Management)
  - Sub: "Edit Grades"
  - Sub: "Delete Grades"

**Indexes:**

- `IX_PermissionCategory_ParentCategoryId`
- `UX_PermissionCategory_Code` (unique)

---

### Permission

**Purpose**: Granular permission definition

| Field       | Type        | Constraints                        | Notes                                  |
| ----------- | ----------- | ---------------------------------- | -------------------------------------- |
| Id          | int         | PK, Identity                       |                                        |
| Name        | string(100) | Required                           | E.g., "View Grades", "Edit Attendance" |
| Code        | string(50)  | Required, Unique                   | E.g., "grades.view", "attendance.edit" |
| CategoryId  | int         | FK to PermissionCategory, Required |                                        |
| Description | string(255) | Nullable                           | What this permission allows            |

**Relationships:**

- Many-to-One with PermissionCategory (CategoryId)
- Many-to-Many with Role via RolePermission

**Validation Rules:**

- Code: Lowercase, dots for hierarchy (regex: `^[a-z]+(\.[a-z]+)*$`)
- Name: 3-100 characters

**Examples:**
| Code | Name | Category | Description |
|------|------|----------|-------------|
| grades.view | View Grades | Grades Management | Can view all grades |
| grades.edit | Edit Grades | Grades Management | Can modify existing grades |
| attendance.mark | Mark Attendance | Attendance | Can mark students present/absent |
| ai.disable | Disable AI Companion | AI Companion | Can disable AI for class |

**Indexes:**

- `UX_Permission_Code` (unique)
- `IX_Permission_CategoryId`

---

### Role

**Purpose**: Permission template (e.g., Admin, Teacher, Student)

| Field        | Type        | Constraints          | Notes                                      |
| ------------ | ----------- | -------------------- | ------------------------------------------ |
| Id           | int         | PK, Identity         |                                            |
| Name         | string(100) | Required, Unique     | E.g., "Admin", "Teacher", "Student"        |
| Rank         | int         | Required             | Hierarchy level (higher = more privileged) |
| IsSystemRole | bool        | Default: false       | System roles cannot be deleted             |
| IsDeleted    | bool        | Default: false       | Soft delete                                |
| DeletedAt    | DateTime?   | Nullable             |                                            |
| DeletedBy    | int?        | FK to User, Nullable |                                            |

**Relationships:**

- Many-to-Many with Permission via RolePermission
- Many-to-Many with User via UserRole

**Validation Rules:**

- Rank: 0-100 (Admin=100, Teacher=50, Student=10)
- System roles (IsSystemRole=true) cannot be deleted or modified

**Default Roles** (seeded):
| Name | Rank | IsSystemRole | Description |
|------|------|--------------|-------------|
| Admin | 100 | true | Full system access |
| Teacher | 50 | true | Class management, own data |
| Student | 10 | true | Own data, rating submission |

**Indexes:**

- `IX_Role_Rank` (for hierarchy checks)
- `UX_Role_Name` (unique)

---

### RolePermission (Join Table)

**Purpose**: Many-to-many relationship between Role and Permission

| Field        | Type | Constraints                      | Notes |
| ------------ | ---- | -------------------------------- | ----- |
| RoleId       | int  | PK (composite), FK to Role       |       |
| PermissionId | int  | PK (composite), FK to Permission |       |

**Relationships:**

- Many-to-One with Role (RoleId)
- Many-to-One with Permission (PermissionId)

**Primary Key**: `(RoleId, PermissionId)`

---

### UserRole (Join Table with Payload)

**Purpose**: Assign multiple roles to users with scope (Global/Department/Class)

| Field      | Type     | Constraints               | Notes                         |
| ---------- | -------- | ------------------------- | ----------------------------- |
| Id         | int      | PK, Identity              |                               |
| UserId     | int      | FK to User, Required      |                               |
| RoleId     | int      | FK to Role, Required      |                               |
| Scope      | enum     | Required                  | Global/Department/Class       |
| ScopeId    | int?     | Nullable                  | Department/Class ID if scoped |
| AssignedBy | int      | FK to User, Required      | Who assigned this role        |
| AssignedAt | DateTime | Required, Default: UtcNow |                               |

**Unique Constraint**: `(UserId, RoleId, Scope, ScopeId)` - No duplicate role assignments

**Relationships:**

- Many-to-One with User (UserId)
- Many-to-One with Role (RoleId)
- Many-to-One with User (AssignedBy)

**Validation Rules:**

- If Scope = Global: ScopeId MUST be NULL
- If Scope = Department: ScopeId MUST be valid DepartmentId
- If Scope = Class: ScopeId MUST be valid ClassId
- User cannot assign roles with Rank >= their highest role Rank

**Indexes:**

- `IX_UserRole_UserId` (for permission evaluation)
- `IX_UserRole_Scope_ScopeId` (for context-specific queries)
- `UX_UserRole_Composite` (unique constraint)

---

### ClassPermission

**Purpose**: Direct permission grants at class level (overrides role permissions)

| Field        | Type     | Constraints                | Notes                        |
| ------------ | -------- | -------------------------- | ---------------------------- |
| Id           | int      | PK, Identity               |                              |
| ClassId      | int      | FK to Class, Required      |                              |
| UserId       | int      | FK to User, Required       |                              |
| PermissionId | int      | FK to Permission, Required |                              |
| GrantedBy    | int      | FK to User, Required       | Class Admin who granted this |
| GrantedAt    | DateTime | Required, Default: UtcNow  |                              |

**Unique Constraint**: `(ClassId, UserId, PermissionId)` - No duplicate permission grants

**Relationships:**

- Many-to-One with Class (ClassId)
- Many-to-One with User (UserId)
- Many-to-One with Permission (PermissionId)
- Many-to-One with User (GrantedBy)

**Indexes:**

- `IX_ClassPermission_ClassId_UserId` (for permission evaluation)
- `UX_ClassPermission_Composite` (unique constraint)

---

### Schedule

**Purpose**: Class schedule (when classes meet)

| Field     | Type       | Constraints             | Notes                |
| --------- | ---------- | ----------------------- | -------------------- |
| Id        | int        | PK, Identity            |                      |
| ClassId   | int        | FK to Class, Required   |                      |
| TeacherId | int        | FK to Teacher, Required |                      |
| DayOfWeek | int        | Required, Range: 0-6    | 0=Sunday, 6=Saturday |
| StartTime | TimeSpan   | Required                | E.g., 09:00:00       |
| EndTime   | TimeSpan   | Required                | E.g., 10:30:00       |
| Room      | string(50) | Nullable                | Classroom number     |

**Relationships:**

- Many-to-One with Class (ClassId)
- Many-to-One with Teacher (TeacherId)

**Validation Rules:**

- EndTime > StartTime
- DayOfWeek: 0-6 (or 1-5 for weekdays only)

**Indexes:**

- `IX_Schedule_TeacherId_DayOfWeek` (for "today's schedule" queries)

---

### Attendance

**Purpose**: Daily student attendance (default: present)

| Field     | Type       | Constraints                          | Notes                          |
| --------- | ---------- | ------------------------------------ | ------------------------------ |
| Id        | int        | PK, Identity                         |                                |
| StudentId | int        | FK to Student, Required              |                                |
| ClassId   | int?       | FK to Class, Nullable (P2: optional) |                                |
| Date      | Date       | Required                             |                                |
| Status    | enum       | Required                             | Present/Absent/Sick/OutOfClass |
| UpdatedBy | string(50) | Required                             | "System" or UserId             |
| UpdatedAt | DateTime   | Required, Default: UtcNow            |                                |

**Unique Constraint**: `(StudentId, Date)` - One attendance record per student per day

**Relationships:**

- Many-to-One with Student (StudentId)
- Many-to-One with Class (ClassId, optional)
- One-to-Many with AttendanceLog (AttendanceId)

**Validation Rules:**

- Date cannot be in the future
- Status: Present (default), Absent, Sick, OutOfClass

**Indexes:**

- `IX_Attendance_StudentId_Date` (for student attendance history)
- `UX_Attendance_StudentDate` (unique constraint)

---

### AttendanceLog (Audit Trail)

**Purpose**: Track all attendance changes

| Field        | Type        | Constraints                | Notes                |
| ------------ | ----------- | -------------------------- | -------------------- |
| Id           | int         | PK, Identity               |                      |
| AttendanceId | int         | FK to Attendance, Required |                      |
| OldStatus    | enum        | Required                   | Previous status      |
| NewStatus    | enum        | Required                   | New status           |
| ChangedBy    | int         | FK to User, Required       |                      |
| ChangedAt    | DateTime    | Required, Default: UtcNow  |                      |
| Reason       | string(255) | Nullable                   | Optional explanation |

**Relationships:**

- Many-to-One with Attendance (AttendanceId)
- Many-to-One with User (ChangedBy)

**Indexes:**

- `IX_AttendanceLog_AttendanceId` (for audit trail queries)

---

### TeacherAbsence

**Purpose**: Teacher absence requests with approval workflow

| Field               | Type        | Constraints               | Notes                                    |
| ------------------- | ----------- | ------------------------- | ---------------------------------------- |
| Id                  | int         | PK, Identity              |                                          |
| TeacherId           | int         | FK to Teacher, Required   |                                          |
| Date                | Date        | Required                  |                                          |
| Reason              | string(255) | Required                  | E.g., "Sick", "Professional Development" |
| Status              | enum        | Required                  | Pending/Approved/Rejected                |
| RequestedAt         | DateTime    | Required, Default: UtcNow |                                          |
| ApprovedBy          | int?        | FK to User, Nullable      | Admin who approved/rejected              |
| ApprovedAt          | DateTime?   | Nullable                  |                                          |
| SubstituteTeacherId | int?        | FK to Teacher, Nullable   | Assigned substitute                      |

**Relationships:**

- Many-to-One with Teacher (TeacherId)
- Many-to-One with User (ApprovedBy)
- Many-to-One with Teacher (SubstituteTeacherId)

**Validation Rules:**

- Date cannot be in the past (when requesting)
- Reason: 10-255 characters

**Indexes:**

- `IX_TeacherAbsence_TeacherId_Date` (for teacher's absence history)
- `IX_TeacherAbsence_Status` (for pending approvals)

---

## Phase 3 (P3) Entities - Student LMS

### Assignment

**Purpose**: Homework/assignments created by teachers

| Field       | Type        | Constraints                    | Notes              |
| ----------- | ----------- | ------------------------------ | ------------------ |
| Id          | int         | PK, Identity                   |                    |
| ClassId     | int         | FK to Class, Required          |                    |
| Title       | string(200) | Required                       |                    |
| Description | text        | Nullable                       | Assignment details |
| DueDate     | DateTime    | Required                       |                    |
| MaxPoints   | int         | Required, Default: 100         |                    |
| CreatedBy   | int         | FK to User (Teacher), Required |                    |
| CreatedAt   | DateTime    | Required, Default: UtcNow      |                    |

**Relationships:**

- Many-to-One with Class (ClassId)
- Many-to-One with User (CreatedBy)
- One-to-Many with AssignmentSubmission (AssignmentId)

**Indexes:**

- `IX_Assignment_ClassId_DueDate` (for upcoming assignments)

---

### AssignmentSubmission

**Purpose**: Student submissions for assignments

| Field        | Type          | Constraints                    | Notes              |
| ------------ | ------------- | ------------------------------ | ------------------ |
| Id           | int           | PK, Identity                   |                    |
| AssignmentId | int           | FK to Assignment, Required     |                    |
| StudentId    | int           | FK to Student, Required        |                    |
| FileUrl      | string(500)   | Nullable                       | Uploaded file path |
| SubmittedAt  | DateTime      | Required, Default: UtcNow      |                    |
| Grade        | decimal(5,2)? | Nullable                       | Graded score       |
| GradedBy     | int?          | FK to User (Teacher), Nullable |                    |
| GradedAt     | DateTime?     | Nullable                       |                    |

**Unique Constraint**: `(AssignmentId, StudentId)` - One submission per student per assignment

**Relationships:**

- Many-to-One with Assignment (AssignmentId)
- Many-to-One with Student (StudentId)
- Many-to-One with User (GradedBy)

**Indexes:**

- `IX_AssignmentSubmission_StudentId` (for student's submissions)
- `UX_AssignmentSubmission_AssignmentStudent` (unique)

---

### Grade

**Purpose**: Grades with audit trail

| Field        | Type         | Constraints                    | Notes                          |
| ------------ | ------------ | ------------------------------ | ------------------------------ |
| Id           | int          | PK, Identity                   |                                |
| StudentId    | int          | FK to Student, Required        |                                |
| ClassId      | int          | FK to Class, Required          |                                |
| AssignmentId | int?         | FK to Assignment, Nullable     | NULL for non-assignment grades |
| Value        | decimal(5,2) | Required                       | Actual grade                   |
| MaxValue     | decimal(5,2) | Required                       | Maximum possible (e.g., 100)   |
| GradedBy     | int          | FK to User (Teacher), Required |                                |
| GradedAt     | DateTime     | Required, Default: UtcNow      |                                |

**Relationships:**

- Many-to-One with Student (StudentId)
- Many-to-One with Class (ClassId)
- Many-to-One with Assignment (AssignmentId, optional)
- Many-to-One with User (GradedBy)
- One-to-Many with GradeLog (GradeId)

**Indexes:**

- `IX_Grade_StudentId_ClassId` (for student's grade report)

---

### GradeLog (Audit Trail)

**Purpose**: Track grade changes

| Field     | Type         | Constraints               | Notes                |
| --------- | ------------ | ------------------------- | -------------------- |
| Id        | int          | PK, Identity              |                      |
| GradeId   | int          | FK to Grade, Required     |                      |
| OldValue  | decimal(5,2) | Required                  |                      |
| NewValue  | decimal(5,2) | Required                  |                      |
| ChangedBy | int          | FK to User, Required      |                      |
| ChangedAt | DateTime     | Required, Default: UtcNow |                      |
| Reason    | string(255)  | Nullable                  | Optional explanation |

**Relationships:**

- Many-to-One with Grade (GradeId)
- Many-to-One with User (ChangedBy)

**Indexes:**

- `IX_GradeLog_GradeId`

---

### ReadingAssignment

**Purpose**: Required readings with comprehension tracking

| Field                    | Type        | Constraints                    | Notes                                 |
| ------------------------ | ----------- | ------------------------------ | ------------------------------------- |
| Id                       | int         | PK, Identity                   |                                       |
| ClassId                  | int         | FK to Class, Required          |                                       |
| Title                    | string(200) | Required                       | E.g., "Module B pages 25-41"          |
| Content                  | text        | Nullable                       | Reading material or link              |
| DueDate                  | DateTime    | Required                       |                                       |
| IsCompleted              | bool        | Default: false                 | Student-specific (future: join table) |
| ComprehensionCheckPassed | bool        | Default: false                 | Quiz/test passed                      |
| CreatedBy                | int         | FK to User (Teacher), Required |                                       |
| CreatedAt                | DateTime    | Required, Default: UtcNow      |                                       |

**Relationships:**

- Many-to-One with Class (ClassId)
- Many-to-One with User (CreatedBy)

**Notes:**

- Phase 3 MVP: Simple per-class reading
- Future: Many-to-many with Student for individual tracking

---

### StudentNote

**Purpose**: Student notes with sharing and gamification

| Field     | Type        | Constraints               | Notes                     |
| --------- | ----------- | ------------------------- | ------------------------- |
| Id        | int         | PK, Identity              |                           |
| StudentId | int         | FK to Student, Required   |                           |
| ClassId   | int         | FK to Class, Required     |                           |
| Title     | string(200) | Required                  |                           |
| Content   | text        | Required                  | Note body                 |
| IsShared  | bool        | Default: false            | Visible to other students |
| Likes     | int         | Default: 0                | Gamification: upvotes     |
| CreatedAt | DateTime    | Required, Default: UtcNow |                           |
| UpdatedAt | DateTime?   | Nullable                  |                           |

**Relationships:**

- Many-to-One with Student (StudentId)
- Many-to-One with Class (ClassId)

**Indexes:**

- `IX_StudentNote_ClassId_IsShared` (for shared notes browsing)
- `IX_StudentNote_Likes` (for top notes leaderboard)

---

### ExtraClass

**Purpose**: Extra meetings/classes (Zoom, Google Meet links)

| Field       | Type        | Constraints                    | Notes                                  |
| ----------- | ----------- | ------------------------------ | -------------------------------------- |
| Id          | int         | PK, Identity                   |                                        |
| ClassId     | int         | FK to Class, Required          |                                        |
| Title       | string(200) | Required                       | E.g., "Office Hours", "Review Session" |
| MeetingUrl  | string(500) | Required                       | Zoom/Meet link                         |
| ScheduledAt | DateTime    | Required                       | Meeting start time                     |
| CreatedBy   | int         | FK to User (Teacher), Required |                                        |
| CreatedAt   | DateTime    | Required, Default: UtcNow      |                                        |

**Relationships:**

- Many-to-One with Class (ClassId)
- Many-to-One with User (CreatedBy)

**Validation Rules:**

- MeetingUrl: Valid URL (regex: `^https?://`)

**Indexes:**

- `IX_ExtraClass_ClassId_ScheduledAt` (for upcoming meetings)

---

## AI & Settings Entities

### AIUsageLog

**Purpose**: Audit trail for AI companion usage

| Field     | Type     | Constraints               | Notes                    |
| --------- | -------- | ------------------------- | ------------------------ |
| Id        | int      | PK, Identity              |                          |
| UserId    | int      | FK to User, Required      |                          |
| ClassId   | int?     | FK to Class, Nullable     |                          |
| Query     | text     | Required                  | Student's question       |
| Response  | text     | Required                  | AI's answer              |
| Mode      | enum     | Required                  | Explain/Guide/ShowAnswer |
| Timestamp | DateTime | Required, Default: UtcNow |                          |

**Relationships:**

- Many-to-One with User (UserId)
- Many-to-One with Class (ClassId, optional)

**Retention Policy**: 90 days (automated cleanup)

**Indexes:**

- `IX_AIUsageLog_UserId_Timestamp` (for user's AI history)
- `IX_AIUsageLog_Timestamp` (for cleanup job)

---

### SentimentAnalysis

**Purpose**: Sentiment of rating comments (Positive/Neutral/Negative)

| Field      | Type         | Constraints                    | Notes                      |
| ---------- | ------------ | ------------------------------ | -------------------------- |
| Id         | int          | PK, Identity                   |                            |
| RatingId   | int          | FK to Rating, Required, Unique |                            |
| Sentiment  | enum         | Required                       | Positive/Neutral/Negative  |
| Confidence | decimal(3,2) | Required, Range: 0-1           | E.g., 0.85 = 85% confident |
| AnalyzedAt | DateTime     | Required, Default: UtcNow      |                            |

**Relationships:**

- One-to-One with Rating (RatingId)

**Indexes:**

- `UX_SentimentAnalysis_RatingId` (unique)

---

### SystemSetting

**Purpose**: Global system settings (key-value store)

| Field       | Type        | Constraints               | Notes                                        |
| ----------- | ----------- | ------------------------- | -------------------------------------------- |
| Id          | int         | PK, Identity              |                                              |
| Key         | string(100) | Required, Unique          | E.g., "EnableDepartments", "MaintenanceMode" |
| Value       | string(500) | Required                  | String representation (parse as needed)      |
| Description | string(255) | Nullable                  | What this setting does                       |
| ModifiedAt  | DateTime    | Required, Default: UtcNow |                                              |
| ModifiedBy  | int?        | FK to User, Nullable      |                                              |

**Example Settings:**
| Key | Value | Description |
|-----|-------|-------------|
| EnableDepartments | "true" | Enable/disable department feature |
| MaintenanceMode | "false" | Put system in maintenance mode |
| AIGlobalDisabled | "false" | Disable AI system-wide |

**Indexes:**

- `UX_SystemSetting_Key` (unique)

---

## Entity Count Summary

| Phase       | Entity Count | Key Features                                                                                                                                     |
| ----------- | ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| P1          | 8            | User, Teacher, Student, Semester, Rating, BonusConfig, BonusTier, TeacherRanking, AISummary                                                      |
| P2          | 11           | Department, Permission, PermissionCategory, Role, RolePermission, UserRole, ClassPermission, Schedule, Attendance, AttendanceLog, TeacherAbsence |
| P3          | 6            | Assignment, AssignmentSubmission, Grade, GradeLog, ReadingAssignment, StudentNote, ExtraClass                                                    |
| AI/Settings | 3            | AIUsageLog, SentimentAnalysis, SystemSetting                                                                                                     |
| **Total**   | **28**       | Comprehensive educational platform                                                                                                               |

---

## Database Indexes Summary

**Critical Indexes** (must be created):

- User: `IX_User_Email` (unique), `IX_User_IsDeleted`
- Rating: `IX_Rating_TeacherId_SemesterId`, `UX_Rating_StudentTeacherSemester` (unique)
- TeacherRanking: `IX_TeacherRanking_SemesterId_Rank`
- Permission: `UX_Permission_Code` (unique)
- UserRole: `IX_UserRole_UserId`, `IX_UserRole_Scope_ScopeId`
- Attendance: `IX_Attendance_StudentId_Date`, `UX_Attendance_StudentDate` (unique)

**Total Indexes**: ~40 (for optimal query performance)

---

## Next Steps

1. ✅ Data model complete (28 entities)
2. ⏳ Generate EF Core entity classes (Models/)
3. ⏳ Generate DbContext configuration (Data/ApplicationDbContext.cs)
4. ⏳ Generate initial migration (`dotnet ef migrations add InitialCreate`)
5. ⏳ Seed default data (Admin user, system roles, default permissions)
6. ⏳ Proceed to Phase 1: Contracts (optional JSON endpoints)
