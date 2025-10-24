# API Contracts: RateMyTeacher

**Version**: 1.0.0  
**Date**: 2025-10-23  
**Format**: REST JSON

---

## Overview

RateMyTeacher is primarily a server-rendered MVC application, but exposes JSON endpoints for:

- AJAX requests (ratings, AI summaries)
- JavaScript-heavy features (AI companion, real-time updates)
- Future mobile app integration (Phase 3)

All API endpoints require authentication unless explicitly marked `[AllowAnonymous]`.

---

## Authentication

**Base URL**: `https://localhost:5001/api`

**Authentication**: Cookie-based (ASP.NET Core Identity)

- Login: `POST /auth/login`
- Logout: `POST /auth/logout`
- Session maintained via encrypted cookie

**Error Responses** (all endpoints):

```json
{
  "success": false,
  "error": {
    "code": "UNAUTHORIZED",
    "message": "You must be logged in to perform this action."
  }
}
```

**Common HTTP Status Codes**:

- `200 OK`: Success
- `201 Created`: Resource created
- `400 Bad Request`: Validation error
- `401 Unauthorized`: Not logged in
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

---

## Endpoints

### 1. Submit Rating

**Endpoint**: `POST /api/ratings`  
**Permission**: Student role required  
**Description**: Submit a rating for a teacher (once per semester)

**Request Body**:

```json
{
  "teacherId": 5,
  "semesterId": 2,
  "stars": 4,
  "comment": "Great teacher! Very clear explanations."
}
```

**Validation**:

- `teacherId`: Required, must exist
- `semesterId`: Required, must exist
- `stars`: Required, 1-5 (inclusive)
- `comment`: Optional, max 500 characters

**Success Response** (`201 Created`):

```json
{
  "success": true,
  "data": {
    "ratingId": 123,
    "teacherId": 5,
    "semesterId": 2,
    "stars": 4,
    "comment": "Great teacher! Very clear explanations.",
    "createdAt": "2025-10-23T14:30:00Z"
  }
}
```

**Error Response** (`400 Bad Request`):

```json
{
  "success": false,
  "error": {
    "code": "DUPLICATE_RATING",
    "message": "You have already rated this teacher for this semester.",
    "details": {
      "existingRatingId": 100,
      "existingRatingDate": "2025-09-15T10:00:00Z"
    }
  }
}
```

---

### 2. Get Leaderboard

**Endpoint**: `GET /api/leaderboard/{semesterId}`  
**Permission**: Admin role required  
**Description**: Retrieve teacher rankings for a semester

**Request Parameters**:

- `semesterId` (path): Semester ID
- `limit` (query, optional): Maximum results (default: 10)

**Example**: `GET /api/leaderboard/2?limit=5`

**Success Response** (`200 OK`):

```json
{
  "success": true,
  "data": {
    "semesterId": 2,
    "semesterName": "Fall 2024",
    "minimumRatings": 10,
    "rankings": [
      {
        "rank": 1,
        "teacherId": 5,
        "teacherName": "John Smith",
        "averageRating": 4.75,
        "totalRatings": 45,
        "bonusAmount": 10.0
      },
      {
        "rank": 2,
        "teacherId": 8,
        "teacherName": "Jane Doe",
        "averageRating": 4.65,
        "totalRatings": 38,
        "bonusAmount": 5.0
      }
    ],
    "calculatedAt": "2025-10-23T00:00:00Z"
  }
}
```

---

### 3. Generate AI Summary

**Endpoint**: `POST /api/ai/summary`  
**Permission**: Teacher role required  
**Description**: Generate AI lesson summary from notes

**Request Body**:

```json
{
  "lessonNotes": "Today we covered photosynthesis...",
  "classId": 12
}
```

**Validation**:

- `lessonNotes`: Required, 50-10,000 characters
- `classId`: Optional (P2+), must exist if provided

**Success Response** (`200 OK`):

```json
{
  "success": true,
  "data": {
    "summaryId": 456,
    "summary": "Key points from today's lesson: 1) Photosynthesis converts light energy to chemical energy...",
    "wordCount": 287,
    "generatedAt": "2025-10-23T14:35:00Z",
    "model": "gemini-2.0-flash-exp"
  }
}
```

**Error Response - Rate Limit** (`429 Too Many Requests`):

```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "AI service is busy. Please try again in a few moments.",
    "retryAfter": 5
  }
}
```

**Error Response - API Failure** (`500 Internal Server Error`):

```json
{
  "success": false,
  "error": {
    "code": "AI_SERVICE_ERROR",
    "message": "Unable to generate summary. Please try again later.",
    "canRetry": true
  }
}
```

---

### 4. AI Companion (Chat)

**Endpoint**: `POST /api/ai/companion`  
**Permission**: Student role required  
**Description**: Ask AI study companion a question (Phase 3)

**Request Body**:

```json
{
  "classId": 12,
  "query": "Can you explain the difference between mitosis and meiosis?",
  "mode": "Explain"
}
```

**Validation**:

- `classId`: Required, must exist
- `query`: Required, 5-500 characters
- `mode`: Required, one of: `Explain`, `Guide`, `ShowAnswer`

**Success Response** (`200 OK`):

```json
{
  "success": true,
  "data": {
    "response": "Mitosis produces two identical daughter cells (diploid), while meiosis produces four unique cells (haploid) for sexual reproduction. Mitosis is for growth and repair...",
    "mode": "Explain",
    "timestamp": "2025-10-23T14:40:00Z",
    "followUpSuggestions": [
      "What are the phases of mitosis?",
      "Why is meiosis important for genetic diversity?"
    ]
  }
}
```

**Error Response - AI Disabled** (`403 Forbidden`):

```json
{
  "success": false,
  "error": {
    "code": "AI_DISABLED_FOR_CLASS",
    "message": "AI companion has been disabled for this class by your teacher.",
    "disabledBy": "Teacher",
    "disabledAt": "2025-10-20T09:00:00Z"
  }
}
```

---

### 5. Check Permission

**Endpoint**: `GET /api/permissions/check`  
**Permission**: Any authenticated user  
**Description**: Check if user has specific permission in context (Phase 2)

**Request Parameters**:

- `permission` (query): Permission code (e.g., `grades.view`)
- `classId` (query, optional): Class context

**Example**: `GET /api/permissions/check?permission=grades.edit&classId=12`

**Success Response** (`200 OK`):

```json
{
  "success": true,
  "data": {
    "hasPermission": true,
    "grantedBy": "Role:Teacher",
    "scope": "Class",
    "scopeId": 12
  }
}
```

**Denied Response** (`200 OK`):

```json
{
  "success": true,
  "data": {
    "hasPermission": false,
    "reason": "Permission not granted for this context."
  }
}
```

---

## Webhooks (Future - Phase 3)

For real-time notifications:

- **Assignment Due Soon**: Notify students 24 hours before deadline
- **Grade Posted**: Notify student when grade is entered
- **Attendance Marked**: Notify parent if student marked absent

**Webhook Payload Example**:

```json
{
  "event": "assignment.due_soon",
  "timestamp": "2025-10-23T14:00:00Z",
  "data": {
    "assignmentId": 89,
    "assignmentTitle": "Chemistry Lab Report",
    "dueDate": "2025-10-24T23:59:59Z",
    "studentIds": [10, 11, 12, 13]
  }
}
```

---

## Rate Limiting

**Default Limits**:

- Standard endpoints: 100 requests/minute per user
- AI endpoints: 10 requests/minute per user
- Leaderboard: 20 requests/minute per user

**Rate Limit Headers** (included in all responses):

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1698073800
```

**Rate Limit Exceeded Response** (`429 Too Many Requests`):

```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests. Please try again later.",
    "retryAfter": 42
  }
}
```

---

## Pagination (for List Endpoints)

**Standard Pagination Parameters**:

- `page` (query, default: 1): Page number
- `pageSize` (query, default: 20, max: 100): Items per page

**Paginated Response Format**:

```json
{
  "success": true,
  "data": {
    "items": [
      /* array of items */
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 20,
      "totalPages": 5,
      "totalItems": 95,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

## Error Codes Reference

| Code                  | HTTP Status | Description                     |
| --------------------- | ----------- | ------------------------------- |
| `VALIDATION_ERROR`    | 400         | Request validation failed       |
| `DUPLICATE_RATING`    | 400         | User already rated this teacher |
| `UNAUTHORIZED`        | 401         | Not logged in                   |
| `FORBIDDEN`           | 403         | Insufficient permissions        |
| `NOT_FOUND`           | 404         | Resource not found              |
| `RATE_LIMIT_EXCEEDED` | 429         | Too many requests               |
| `AI_SERVICE_ERROR`    | 500         | Gemini API error                |
| `DATABASE_ERROR`      | 500         | Database operation failed       |
| `INTERNAL_ERROR`      | 500         | Unhandled server error          |

---

## Next Steps

1. ✅ API contracts documented
2. ⏳ Implement controllers (Controllers/Api/)
3. ⏳ Add model validation attributes ([Range], [Required], etc.)
4. ⏳ Implement rate limiting middleware
5. ⏳ Add API integration tests
6. ⏳ Generate OpenAPI/Swagger documentation (optional)

**For detailed implementation, see**: `specs/main/tasks.md` (generated via `/speckit.tasks`)
