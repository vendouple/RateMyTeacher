# RateMyTeacher - Feature Spec

Priority order: P1 highest

## US1 (P1) - Teacher listing and ratings

- As a student, I can view a list of teachers and rate a teacher 1-5 stars once per term.
- Acceptance:
  - Student can submit a rating with optional comment
  - Average rating is calculated
  - One rating per student per teacher per semester enforced

## US2 (P1) - Leaderboard & bonus calculation

- As admin, I can view teacher rankings per semester and distribute bonuses (1st $10, 2nd $5).
- Acceptance:
  - Rankings computed correctly with minimum vote threshold
  - Bonus calculation follows business rules

## US3 (P2) - Lesson summary generation (AI)

- As a teacher, I can request an AI-generated lesson summary from lesson notes.
- Acceptance:
  - AI returns a structured summary under 300 words
  - Uses Gemini model via API key in .env

## US4 (P2) - Attendance & basic schedule display

- As a teacher, I can mark attendance and see my schedule for the day.
- Acceptance:
  - Attendance can be marked present/absent/late
  - Schedule displays current and next class

## US5 (P3) - Feedback sentiment analysis

- As admin, I can see sentiment analysis of student comments for a teacher.
- Acceptance:
  - Sentiment labeled Positive/Neutral/Negative
  - Insights stored

Optional docs: data-model.md, contracts/
