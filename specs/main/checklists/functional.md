# Requirements Quality Checklist – Core Functional Scope

- Purpose: Self-review of Phase 1+ foundational feature requirements prior to Phase 2 implementation.
- Created: 2025-11-18
- Scope: Authentication, ratings, leaderboard/bonus system, AI summaries, attendance, permissions (functional focus). Non-functional items triaged separately per guidance.

## Requirement Completeness

- [ ] CHK001 Are all user-facing authentication flows (login, session lifetime, role-specific access) explicitly documented for Student/Teacher/Admin personas? [Completeness, Spec §Authentication & Authorization]
- [ ] CHK002 Do the requirements enumerate every data entity needed for P1 (User, Teacher, Student, Semester, Rating, BonusConfig, BonusTier, TeacherRanking) along with mandatory fields and relationships? [Completeness, Spec §US1; Spec §US2; Plan §Data Model]
- [ ] CHK003 Is the admin-configurable bonus management experience (create/edit/delete tiers, thresholds, ranges) fully described including UI triggers and validation states? [Completeness, Spec §US2]
- [ ] CHK004 Are seeding expectations for default roles, semesters, and bonus configurations captured so environments start in a known state? [Completeness, Plan §Quickstart; Spec §US2]
- [ ] CHK005 Do the requirements specify how attendance records (student daily defaults, teacher absences) are initialized and persisted across semesters? [Completeness, Spec §US4]
- [ ] CHK006 Are AI summary prerequisites (required inputs, word limits, retry behavior) detailed enough to block missing fields or invalid payloads before calling Gemini? [Completeness, Spec §US3]

## Requirement Clarity

- [ ] CHK007 Is the meaning of "one rating per student per teacher per semester" unambiguous regarding edits, deletions, and resubmissions? [Clarity, Spec §US1]
- [ ] CHK008 Are the terms "term" vs. "semester" applied consistently in every section and data artifact after the clarification note? [Clarity, Spec §Clarifications]
- [ ] CHK009 Does the spec explain precisely how bonus tiers that overlap (e.g., single position plus range) should be prioritized or combined? [Clarity, Spec §US2]
- [ ] CHK010 Are teacher absence visibility rules (hidden until substitute assigned) spelled out for all user groups to prevent interpretation drift? [Clarity, Spec §US4]
- [ ] CHK011 Is "structured summary under 300 words" defined with formatting expectations (sections, bullet points) so output can be validated deterministically? [Clarity, Spec §US3]

## Requirement Consistency

- [ ] CHK012 Do minimum rating thresholds align between Clarifications (10 ratings) and business rules (20 votes) or is reconciliation needed? [Consistency, Spec §Clarifications; Spec §3.1 RateMyTeacher]
- [ ] CHK013 Are bonus payout rules in Spec §US2 consistent with monetary values listed in SPEC §3.1 (1st $10, 2nd $5) or should configurable tiers supersede static amounts? [Consistency, Spec §US2; Spec §3.1 RateMyTeacher]
- [ ] CHK014 Are attendance workflows consistent between teacher-requested absences and student daily updates (approval, notification, visibility)? [Consistency, Spec §US4]

## Acceptance Criteria Quality

- [ ] CHK015 Can leaderboard correctness be objectively validated (e.g., deterministic tie handling, exclusion criteria) based on current acceptance criteria? [Acceptance Criteria, Spec §US2]
- [ ] CHK016 Are success metrics for AI summary generation (max latency, retry attempts, acceptable error messaging) defined to determine pass/fail states? [Acceptance Criteria, Spec §US3]\[Gap]
- [ ] CHK017 Do authentication requirements include measurable security conditions (session timeout, password rules) or is additional acceptance detail needed? [Acceptance Criteria, Spec §Authentication & Authorization]\[Gap]

## Scenario Coverage

- [ ] CHK018 Are alternate flows documented for students attempting to rate teachers they are not enrolled with or who lack qualifying semesters? [Coverage, Spec §US1]\[Gap]
- [ ] CHK019 Do requirements capture admin workflows for recalculating rankings after bonus config edits or late rating submissions? [Coverage, Spec §US2]
- [ ] CHK020 Are rollback or approval-denial flows described for teacher absence requests (e.g., rejection reasons, resubmission)? [Coverage, Spec §US4]\[Gap]

## Edge Case Coverage

- [ ] CHK021 Is behavior defined for teachers with zero ratings or those exactly at the minimum threshold (inclusion/exclusion, UI messaging)? [Edge Case, Spec §US2]
- [ ] CHK022 Are requirements present for handling semester transitions (closing ratings, archiving summaries) to prevent cross-semester contamination? [Edge Case, Spec §US1]\[Gap]
- [ ] CHK023 Is there guidance for AI summary failures after repeated retries (fallback content, logging, user notification)? [Edge Case, Spec §US3]

## Dependencies & Assumptions

- [ ] CHK024 Are external dependency expectations (Gemini API quotas, calendar integrations, notification services) documented with contingencies for outages? [Dependency, Spec §Clarifications; Spec §US3]\[Gap]
- [ ] CHK025 Are assumptions about authenticated-only access (no anonymous endpoints) reconciled with future needs such as marketing or status pages? [Assumption, Spec §Authentication & Authorization]

## Ambiguities & Conflicts

- [ ] CHK026 Does the spec resolve how AI companion controls interact with lesson summaries (e.g., if AI disabled for class, are summaries still allowed)? [Ambiguity, Spec §US3; Spec §US7]
- [ ] CHK027 Is the stated business rule "Only enrolled students can vote" enforceable with current data model, or are enrollment linkage requirements missing? [Ambiguity, Spec §3.1 RateMyTeacher; Plan §Data Model]
- [ ] CHK028 Are there clear rules for how bonus payouts adjust when ties occur (split amounts vs. duplicate payouts), or is additional guidance needed? [Ambiguity, Spec §3.1 RateMyTeacher]

## Non-Functional Requirements (Deferred Focus)

- [ ] CHK029 Are placeholders noted for future performance/accessibility requirements so they can be prioritized later without being forgotten? \[Gap], Plan §Summary; Spec §UX Principles
