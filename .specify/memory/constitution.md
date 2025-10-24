# RateMyTeacher Constitution

<!--
Sync Impact Report (v1.0.0 - Initial Constitution):
- Version: TEMPLATE → 1.0.0
- Modified Principles: All (initial definition)
- Added Sections: All core principles, Security & Permissions, Development Standards, Governance
- Templates Status:
  ✅ spec-template.md - Aligned with authentication & permission requirements
  ✅ plan-template.md - Aligned with phased development approach
  ✅ tasks-template.md - Aligned with test-driven and security-first principles
- Follow-up TODOs: None
- Rationale: Initial constitution establishing governance framework for RateMyTeacher project
-->

## Core Principles

### I. Security-First Development (NON-NEGOTIABLE)

**All features MUST be designed with security as the foundation, not an afterthought.**

- Input validation required on both client and server sides for all user inputs
- SQL injection prevention mandatory through parameterized queries or EF Core
- XSS protection enforced via output sanitization and Content Security Policy
- HTTPS-only connections in production environments
- Authentication required for all features; anonymous access explicitly forbidden
- Role-based access control (RBAC) with granular permissions mandatory
- Sensitive data (API keys, credentials) MUST be stored in environment variables, never committed to version control

**Rationale**: Educational platforms handle sensitive student and teacher data. Security breaches can expose personal information, grades, and comments, leading to privacy violations and loss of trust.

### II. Discord-Style Multi-Role Permission System

**Users can be assigned multiple roles; permissions are cumulative with hierarchical protection.**

- Users may hold multiple roles simultaneously (e.g., Admin + SrAdmin + JrAdminExtras)
- Effective permissions = union of all assigned role permissions
- Role hierarchy MUST be enforced: Junior roles cannot modify senior role permissions
  - Example: JrAdmin cannot edit permissions of SrAdmin or Admin roles
  - Example: JrAdmin cannot assign/remove SrAdmin or Admin roles to/from users
- Permission categories are hierarchical with expandable sub-permissions (Discord-style)
  - Top-level: Enable/disable entire category (e.g., "Grades Management")
  - Expanded: Granular controls (View, Edit, Delete, Create)
- Three permission scopes: Global → Department (optional) → Class
- Class-level permissions override department/global permissions
- Global Admins have irrevocable full access; cannot be removed from any context

**Rationale**: Complex educational environments require flexible permission models. A chemistry teacher may need admin rights in their class while having view-only access to the physics department. Multi-role assignments prevent role explosion and simplify management.

### III. Test-Driven Development for Core Features (NON-NEGOTIABLE)

**P1 (Priority 1) features MUST follow TDD; P2/P3 features STRONGLY RECOMMENDED.**

- Red-Green-Refactor cycle strictly enforced for P1 features:
  1. Write failing test with user approval
  2. Implement minimum code to pass
  3. Refactor while maintaining green tests
- Target: 80%+ code coverage on business logic and services
- Integration tests required for: Authentication flows, permission checks, database operations, external API integrations (Gemini AI)
- Unit tests required for: Service layer methods, permission evaluation logic, bonus calculation algorithms

**Rationale**: Permission systems and grade calculations are critical. Bugs can lead to unauthorized data access or incorrect academic records. Tests serve as executable documentation and prevent regressions.

### IV. AI Transparency & Control

**AI features MUST be controllable at multiple levels; students MUST understand AI limitations.**

- Three-level AI control hierarchy (all MUST be respected):
  1. Global Admin: Can disable AI system-wide
  2. Class Admin: Can disable AI for specific class
  3. Teacher: Can disable AI for their own class
- AI companion MUST NOT solve homework directly; only guide or show answers
- AI mode MUST be visible to students: "Explain", "Guide", or "Show Answer"
- AI failures MUST show user-friendly messages with retry option; never expose technical errors
- AI usage MUST be logged for auditing and safety monitoring

**Rationale**: Uncontrolled AI can enable cheating or create dependency. Teachers must retain pedagogical control. Transparent AI modes set appropriate expectations for student learning.

### V. Data Integrity & Audit Trail

**Critical data modifications MUST be traceable; integrity constraints MUST be enforced.**

- One rating per student per teacher per semester enforced at database level (unique constraint)
- Attendance modifications MUST log: who changed, when, previous value, new value
- Grade changes MUST be auditable: teacher, timestamp, reason (if provided)
- Teacher absence requests MUST track approval workflow: submitted, approved/rejected, approver, timestamp
- Bonus tier changes MUST be versioned: who changed, when, previous configuration
- Soft deletes preferred over hard deletes for audit purposes

**Rationale**: Academic records are permanent and legally significant. Disputes over grades or attendance require proof. Audit trails enable accountability and forensic analysis.

### VI. Phased Development with Feature Flags

**New features MUST be developed in phases; incomplete features MUST be hidden or flagged.**

- Phase 1 (P1): Core ratings, authentication, leaderboards, basic AI summaries
- Phase 2 (P2): Permission system, attendance tracking, settings management, AI companion controls
- Phase 3 (P3): Student LMS, gamification, note sharing, extra classes, advanced AI study tools
- Department feature MUST be toggleable via system setting (can be disabled entirely)
- Features under development MUST be hidden from production users or clearly marked "BETA"
- Database schema MUST support all phases from Phase 1 (use nullable columns or separate tables for future features)

**Rationale**: Delivering value incrementally reduces risk. Feature flags allow safe production deployment while development continues. Users see polished features, not half-built prototypes.

### VII. Accessibility & Localization-Ready

**UI MUST support multiple languages and accessibility standards.**

- ASP.NET Core i18n localization framework MUST be used (base: English)
- ARIA labels required for interactive elements
- Keyboard navigation MUST work for all features
- Dark/light mode support required (neuromorphic design system)
- Color contrast ratios MUST meet WCAG 2.1 AA standards
- Localization resource files MUST be structured by controller and view

**Rationale**: Educational tools serve diverse populations. International schools need multi-language support. Accessibility compliance is often legally required and ensures inclusivity.

## Security & Permissions Architecture

### Permission Model Rules

1. **Permission Evaluation Order** (most specific wins):
   - Class-level overrides → Department-level overrides → Global defaults
2. **Role Assignment**:
   - Users can have unlimited role assignments
   - Each role assignment can be scoped: Global, Department-specific, or Class-specific
3. **Permission Calculation**:
   - Effective permissions = UNION of all role permissions in applicable scope
   - Explicit DENY does not exist; absence of permission = denial
4. **Role Hierarchy Protection**:
   - Each role has an implicit "rank" or "level"
   - Users cannot modify roles with equal or higher rank
   - Users cannot assign roles with higher rank than their own highest role
5. **Default Roles** (system-managed, cannot be deleted):
   - Admin (rank 100): Full system access
   - Teacher (rank 50): Class management, own data access
   - Student (rank 10): Own data, rating submission, view permissions

### AI Companion Safety Rules

- AI responses MUST be rate-limited per student per class (prevent abuse)
- AI MUST refuse to answer questions explicitly about test answers
- AI companion MUST be disabled during scheduled assessments/exams (admin control)
- AI conversation history MUST be retained for 90 days for safety review
- Inappropriate AI usage MUST trigger alerts to class teacher and admin

## Development Standards

### Code Quality Gates

- All PRs MUST pass: Linting, unit tests, integration tests (if applicable), security scans
- Business logic MUST reside in service layer; controllers MUST remain thin
- Database queries MUST use EF Core or parameterized SQL (never string concatenation)
- Secrets MUST be loaded from `.env` file via DotNetEnv library
- Error messages to users MUST be friendly; detailed errors only in logs

### Database Design Standards

- Foreign key constraints MUST be defined for referential integrity
- Indexes MUST be added for: Foreign keys, frequent query filters, unique constraints
- Enum values stored as strings preferred over integers (readability in DB tools)
- DateTime fields MUST use UTC; conversion to local time in UI layer only
- Soft delete pattern used for: Users, Classes, Roles, Departments (retain history)

### API & External Service Standards

- Gemini AI integration MUST handle: Rate limits, network failures, invalid API keys
- External meeting platforms (Zoom, Google Meet) MUST validate links before storing
- File uploads (assignments, notes) MUST validate: File type, size, virus scanning (future)
- All external API calls MUST have configurable timeouts (default: 30 seconds)

## Governance

### Amendment Procedure

1. Proposed changes submitted via PR to `.specify/memory/constitution.md`
2. Impact analysis required: Which templates, docs, code affected?
3. Version bump determined:
   - **MAJOR**: Removing/redefining principles, breaking governance changes
   - **MINOR**: Adding new principles, expanding sections materially
   - **PATCH**: Clarifications, typo fixes, non-semantic refinements
4. Sync Impact Report added as HTML comment at top of constitution
5. Approval required from project owner or designated constitutional authority
6. Migration plan required if changes affect existing code or processes

### Compliance Review

- All feature specifications MUST reference applicable constitutional principles
- All PRs MUST be checked against constitution during code review
- Quarterly constitutional review scheduled to ensure alignment with project evolution
- Violations MUST be documented and remediated; repeat violations trigger process review

### Version History

- Constitution changes tracked in git history with detailed commit messages
- Breaking changes MUST be announced in release notes and migration guides
- Deprecated principles retained in appendix for historical reference (future versions)

**Version**: 1.0.0 | **Ratified**: 2025-10-23 | **Last Amended**: 2025-10-23
