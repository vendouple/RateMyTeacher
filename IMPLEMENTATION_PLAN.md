# RateMyTeacher - Implementation Plan

## Project Configuration

### Technology Stack Overview

**Framework**: ASP.NET Core 9.0 MVC  
**Database**: SQLite (Embedded, No Installation Required)  
**AI Model**: Google Gemini 2.5 Flash  
**Target**: Demo/Prototype Application  
**Deployment**: Standalone executable with embedded database

---

## 1. Project Setup & Configuration

### 1.1 Create New ASP.NET Core MVC Project

```powershell
# Create new ASP.NET Core 9 MVC project
dotnet new mvc -n RateMyTeacher -f net9.0

# Navigate to project directory
cd RateMyTeacher

# Add required NuGet packages
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Google.Generative.AI --version 1.0.0
dotnet add package DotNetEnv --version 3.1.1
```

### 1.2 Environment Configuration (.env)

Create `.env` file in project root:

```env
# Google Gemini API Configuration
GEMINI_API_KEY=your_api_key_here
GEMINI_MODEL=gemini-2.5-flash

# Application Settings
ASPNETCORE_ENVIRONMENT=Development
DATABASE_PATH=Data/ratemyteacher.db

# Bonus Configuration
FIRST_PLACE_BONUS=10.00
SECOND_PLACE_BONUS=5.00
MINIMUM_VOTES_THRESHOLD=20

# AI Configuration
AI_MAX_TOKENS=8192
AI_TEMPERATURE=0.7
AI_TIMEOUT_SECONDS=30
```

### 1.3 .gitignore Updates

Add to `.gitignore`:

```
# Environment files
.env
.env.*
!.env.example

# SQLite Database
*.db
*.db-shm
*.db-wal
Data/
```

### 1.4 Create .env.example

```env
# Copy this file to .env and fill in your actual values
GEMINI_API_KEY=your_gemini_api_key_from_ai_google_dev
GEMINI_MODEL=gemini-2.5-flash
ASPNETCORE_ENVIRONMENT=Development
DATABASE_PATH=Data/ratemyteacher.db
FIRST_PLACE_BONUS=10.00
SECOND_PLACE_BONUS=5.00
MINIMUM_VOTES_THRESHOLD=20
AI_MAX_TOKENS=8192
AI_TEMPERATURE=0.7
AI_TIMEOUT_SECONDS=30
```

---

## 2. Database Architecture (SQLite)

### 2.1 Database Models

Create `Models/` folder structure:

```
Models/
├── User.cs
├── Teacher.cs
├── Student.cs
├── Subject.cs
├── Class.cs
├── Schedule.cs
├── Lesson.cs
├── LessonLog.cs
├── Attendance.cs
├── Grade.cs
├── Assignment.cs
├── Rating.cs
├── Feedback.cs
├── Announcement.cs
├── Resource.cs
├── Badge.cs
└── Behavior.cs
```

#### Example: Teacher.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models
{
    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(200)]
        public string Email { get; set; }

        [MaxLength(500)]
        public string Bio { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; }

        public int TotalRatings { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalBonusEarned { get; set; }

        public DateTime HireDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Class> Classes { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
        public ICollection<Badge> Badges { get; set; }
    }
}
```

#### Example: Rating.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        [Range(1, 5)]
        public int OverallRating { get; set; }

        [Range(1, 5)]
        public int ClarityRating { get; set; }

        [Range(1, 5)]
        public int EngagementRating { get; set; }

        [Range(1, 5)]
        public int FairnessRating { get; set; }

        [Range(1, 5)]
        public int HelpfulnessRating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }

        [Required]
        [MaxLength(50)]
        public string Semester { get; set; } // e.g., "2025-Fall"

        public bool IsVerified { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

### 2.2 DbContext Configuration

Create `Data/ApplicationDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Models;

namespace RateMyTeacher.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonLog> LessonLogs { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<Behavior> Behaviors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User table
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Rating table - One rating per student per teacher per semester
            modelBuilder.Entity<Rating>()
                .HasIndex(r => new { r.StudentId, r.TeacherId, r.Semester })
                .IsUnique();

            // Attendance composite index
            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.StudentId, a.LessonId })
                .IsUnique();

            // Grade composite index
            modelBuilder.Entity<Grade>()
                .HasIndex(g => new { g.StudentId, g.SubjectId, g.Semester });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "admin@ratemyteacher.com",
                    Username = "admin",
                    PasswordHash = "hashed_password", // Use proper hashing
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed subjects
            modelBuilder.Entity<Subject>().HasData(
                new Subject { SubjectId = 1, Name = "Mathematics", Code = "MATH", CreatedAt = DateTime.UtcNow },
                new Subject { SubjectId = 2, Name = "English", Code = "ENG", CreatedAt = DateTime.UtcNow },
                new Subject { SubjectId = 3, Name = "Science", Code = "SCI", CreatedAt = DateTime.UtcNow },
                new Subject { SubjectId = 4, Name = "History", Code = "HIST", CreatedAt = DateTime.UtcNow },
                new Subject { SubjectId = 5, Name = "Physical Education", Code = "PE", CreatedAt = DateTime.UtcNow }
            );

            // Seed badge types
            modelBuilder.Entity<Badge>().HasData(
                new Badge { BadgeId = 1, Name = "Engagement Master", Description = "High student participation rates", IconUrl = "/images/badges/engagement.svg" },
                new Badge { BadgeId = 2, Name = "Punctuality Champion", Description = "Always on time", IconUrl = "/images/badges/punctuality.svg" },
                new Badge { BadgeId = 3, Name = "Innovation Award", Description = "Creative teaching methods", IconUrl = "/images/badges/innovation.svg" },
                new Badge { BadgeId = 4, Name = "Student Favorite", Description = "Consistently high ratings", IconUrl = "/images/badges/favorite.svg" }
            );
        }
    }
}
```

### 2.3 Program.cs Configuration

Update `Program.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;
using RateMyTeacher.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
Env.Load();

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure SQLite Database
var databasePath = Environment.GetEnvironmentVariable("DATABASE_PATH") ?? "Data/ratemyteacher.db";
var connectionString = $"Data Source={databasePath}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Register Gemini AI Service
builder.Services.AddSingleton<IGeminiService, GeminiService>();

// Register other services
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

## 3. Gemini AI Integration

### 3.1 Gemini Service Interface

Create `Services/IGeminiService.cs`:

```csharp
namespace RateMyTeacher.Services
{
    public interface IGeminiService
    {
        Task<string> GenerateLessonSummary(string lessonContent, string topics);
        Task<string> GenerateQuizQuestions(string lessonContent, int numberOfQuestions);
        Task<string> AnswerStudentQuestion(string question, string context);
        Task<string> SuggestLessonPlan(string subject, string previousTopics, string curriculum);
        Task<string> AnalyzeFeedbackSentiment(string feedback);
        Task<string> GenerateWeeklyReport(string teacherName, Dictionary<string, object> metrics);
        Task<List<string>> SuggestImprovementAreas(double rating, List<string> feedbackComments);
    }
}
```

### 3.2 Gemini Service Implementation

Create `Services/GeminiService.cs`:

```csharp
using Google.Generative.AI;
using Google.Generative.AI.Models;

namespace RateMyTeacher.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly string _apiKey;
        private readonly string _modelName;
        private readonly int _maxTokens;
        private readonly float _temperature;
        private readonly int _timeoutSeconds;

        public GeminiService()
        {
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                ?? throw new InvalidOperationException("GEMINI_API_KEY not found in environment variables");
            _modelName = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.5-flash";
            _maxTokens = int.Parse(Environment.GetEnvironmentVariable("AI_MAX_TOKENS") ?? "8192");
            _temperature = float.Parse(Environment.GetEnvironmentVariable("AI_TEMPERATURE") ?? "0.7");
            _timeoutSeconds = int.Parse(Environment.GetEnvironmentVariable("AI_TIMEOUT_SECONDS") ?? "30");
        }

        private async Task<string> CallGeminiAPI(string prompt)
        {
            try
            {
                var client = new GenerativeAI(_apiKey);
                var model = client.GenerativeModel(_modelName);

                var generationConfig = new GenerationConfig
                {
                    Temperature = _temperature,
                    MaxOutputTokens = _maxTokens
                };

                var response = await model.GenerateContentAsync(prompt, generationConfig);
                return response.Text;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Gemini API Error: {ex.Message}");
                throw new Exception("Failed to generate AI response", ex);
            }
        }

        public async Task<string> GenerateLessonSummary(string lessonContent, string topics)
        {
            var prompt = $@"You are an educational assistant. Generate a concise lesson summary for students.

Lesson Content:
{lessonContent}

Topics Covered:
{topics}

Please create a clear, structured summary including:
1. Main topics covered
2. Key concepts
3. Important takeaways
4. Study tips

Keep it under 300 words.";

            return await CallGeminiAPI(prompt);
        }

        public async Task<string> GenerateQuizQuestions(string lessonContent, int numberOfQuestions)
        {
            var prompt = $@"You are a quiz generator for teachers. Create {numberOfQuestions} multiple-choice questions based on this lesson content:

{lessonContent}

For each question provide:
- The question
- 4 answer options (A, B, C, D)
- The correct answer
- A brief explanation

Format as JSON array with structure:
{{
  ""questions"": [
    {{
      ""question"": ""..."",
      ""options"": [""A. ..."", ""B. ..."", ""C. ..."", ""D. ...""],
      ""correctAnswer"": ""A"",
      ""explanation"": ""...""
    }}
  ]
}}";

            return await CallGeminiAPI(prompt);
        }

        public async Task<string> AnswerStudentQuestion(string question, string context)
        {
            var prompt = $@"You are a helpful teaching assistant. Answer this student's question based on the lesson material.

Student Question:
{question}

Lesson Material:
{context}

Provide a clear, educational answer that:
1. Directly answers the question
2. References the lesson material
3. Gives examples if helpful
4. Encourages further learning

Keep the answer concise (under 200 words).";

            return await CallGeminiAPI(prompt);
        }

        public async Task<string> SuggestLessonPlan(string subject, string previousTopics, string curriculum)
        {
            var prompt = $@"You are an educational curriculum planner. Suggest the next lesson plan.

Subject: {subject}
Previous Topics Covered: {previousTopics}
Curriculum Standards: {curriculum}

Create a lesson plan suggestion including:
1. Next topic to cover
2. Learning objectives
3. Suggested activities (2-3)
4. Required materials
5. Estimated duration
6. Assessment methods

Format as structured text.";

            return await CallGeminiAPI(prompt);
        }

        public async Task<string> AnalyzeFeedbackSentiment(string feedback)
        {
            var prompt = $@"Analyze the sentiment of this student feedback and categorize it.

Feedback:
{feedback}

Provide:
1. Overall sentiment (Positive/Neutral/Negative)
2. Key themes mentioned
3. Actionable insights for the teacher

Format as JSON:
{{
  ""sentiment"": ""Positive|Neutral|Negative"",
  ""themes"": [""theme1"", ""theme2""],
  ""insights"": ""...""
}}";

            return await CallGeminiAPI(prompt);
        }

        public async Task<string> GenerateWeeklyReport(string teacherName, Dictionary<string, object> metrics)
        {
            var metricsText = string.Join("\n", metrics.Select(m => $"{m.Key}: {m.Value}"));

            var prompt = $@"Generate a professional weekly teaching report for {teacherName}.

Metrics:
{metricsText}

Create a report including:
1. Performance Summary
2. Key Achievements
3. Areas for Improvement
4. Student Engagement Analysis
5. Recommendations for Next Week

Format as professional markdown.";

            return await CallGeminiAPI(prompt);
        }

        public async Task<List<string>> SuggestImprovementAreas(double rating, List<string> feedbackComments)
        {
            var feedbackText = string.Join("\n- ", feedbackComments);

            var prompt = $@"You are an educational consultant. Analyze this teacher's performance data.

Average Rating: {rating}/5
Student Feedback:
- {feedbackText}

Suggest 3-5 specific, actionable improvement areas for this teacher.
Format as JSON array:
{{
  ""suggestions"": [""suggestion1"", ""suggestion2"", ...]
}}";

            var response = await CallGeminiAPI(prompt);

            // Parse JSON response (simplified - add proper JSON parsing)
            try
            {
                // Use System.Text.Json or Newtonsoft.Json to parse
                var suggestions = new List<string>();
                // Parse and extract suggestions
                return suggestions;
            }
            catch
            {
                return new List<string> { response };
            }
        }
    }
}
```

---

## 4. Key Controllers

### 4.1 Teachers Controller

Create `Controllers/TeachersController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using RateMyTeacher.Services;
using RateMyTeacher.Models;

namespace RateMyTeacher.Controllers
{
    public class TeachersController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IRatingService _ratingService;
        private readonly IGeminiService _geminiService;

        public TeachersController(
            ITeacherService teacherService,
            IRatingService ratingService,
            IGeminiService geminiService)
        {
            _teacherService = teacherService;
            _ratingService = ratingService;
            _geminiService = geminiService;
        }

        // GET: /Teachers
        public async Task<IActionResult> Index()
        {
            var teachers = await _teacherService.GetAllTeachersAsync();
            return View(teachers);
        }

        // GET: /Teachers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            if (teacher == null)
                return NotFound();

            var ratings = await _ratingService.GetTeacherRatingsAsync(id);
            ViewBag.Ratings = ratings;

            return View(teacher);
        }

        // GET: /Teachers/Leaderboard
        public async Task<IActionResult> Leaderboard(string semester)
        {
            var rankings = await _ratingService.GetTeacherRankingsAsync(semester);
            ViewBag.Semester = semester;
            return View(rankings);
        }

        // POST: /Teachers/Rate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(Rating rating)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _ratingService.SubmitRatingAsync(rating);

            if (result.Success)
            {
                // Analyze sentiment using Gemini
                if (!string.IsNullOrEmpty(rating.Comment))
                {
                    var sentiment = await _geminiService.AnalyzeFeedbackSentiment(rating.Comment);
                    // Store sentiment analysis
                }

                return RedirectToAction("Details", new { id = rating.TeacherId });
            }

            return View("Error");
        }
    }
}
```

### 4.2 Lessons Controller

Create `Controllers/LessonsController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using RateMyTeacher.Services;
using RateMyTeacher.Models;

namespace RateMyTeacher.Controllers
{
    public class LessonsController : Controller
    {
        private readonly ILessonService _lessonService;
        private readonly IGeminiService _geminiService;

        public LessonsController(ILessonService lessonService, IGeminiService geminiService)
        {
            _lessonService = lessonService;
            _geminiService = geminiService;
        }

        // GET: /Lessons/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Lessons/GenerateSummary
        [HttpPost]
        public async Task<IActionResult> GenerateSummary([FromBody] LessonSummaryRequest request)
        {
            try
            {
                var summary = await _geminiService.GenerateLessonSummary(
                    request.LessonContent,
                    request.Topics);

                return Json(new { success = true, summary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // POST: /Lessons/GenerateQuiz
        [HttpPost]
        public async Task<IActionResult> GenerateQuiz([FromBody] QuizRequest request)
        {
            try
            {
                var quiz = await _geminiService.GenerateQuizQuestions(
                    request.LessonContent,
                    request.NumberOfQuestions);

                return Json(new { success = true, quiz });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // POST: /Lessons/AskAI
        [HttpPost]
        public async Task<IActionResult> AskAI([FromBody] AIQuestionRequest request)
        {
            try
            {
                var answer = await _geminiService.AnswerStudentQuestion(
                    request.Question,
                    request.Context);

                return Json(new { success = true, answer });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
```

---

## 5. Frontend Implementation

### 5.1 Neuromorphic CSS Setup

Create `wwwroot/css/neuromorphic.css`:

```css
:root {
  /* Light Mode Colors */
  --bg-primary: #e4e8ec;
  --bg-secondary: #d8dde3;
  --surface: #e4e8ec;
  --text-primary: #2c3e50;
  --text-secondary: #5a6c7d;
  --shadow-light: rgba(255, 255, 255, 0.8);
  --shadow-dark: rgba(0, 0, 0, 0.15);
  --accent: #3498db;
  --accent-hover: #2980b9;
}

[data-theme="dark"] {
  /* Dark Mode Colors */
  --bg-primary: #1a1d23;
  --bg-secondary: #24282f;
  --surface: #2a2f38;
  --text-primary: #e4e8ec;
  --text-secondary: #a0a8b4;
  --shadow-light: rgba(255, 255, 255, 0.05);
  --shadow-dark: rgba(0, 0, 0, 0.4);
  --accent: #5dade2;
  --accent-hover: #3498db;
}

body {
  background-color: var(--bg-primary);
  color: var(--text-primary);
  font-family: "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
  transition: background-color 0.3s ease, color 0.3s ease;
}

/* Neuromorphic Button */
.neuro-btn {
  background: var(--surface);
  border: none;
  border-radius: 12px;
  padding: 12px 24px;
  color: var(--text-primary);
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: -5px -5px 10px var(--shadow-light), 5px 5px 10px var(--shadow-dark);
}

.neuro-btn:hover {
  box-shadow: -7px -7px 14px var(--shadow-light), 7px 7px 14px var(--shadow-dark);
}

.neuro-btn:active {
  box-shadow: inset -3px -3px 8px var(--shadow-light), inset 3px 3px 8px var(--shadow-dark);
}

/* Neuromorphic Card */
.neuro-card {
  background: var(--surface);
  border-radius: 16px;
  padding: 24px;
  margin: 16px 0;
  box-shadow: -8px -8px 16px var(--shadow-light), 8px 8px 16px var(--shadow-dark);
  transition: all 0.3s ease;
}

.neuro-card:hover {
  box-shadow: -12px -12px 20px var(--shadow-light), 12px 12px 20px var(--shadow-dark);
}

/* Neuromorphic Input */
.neuro-input {
  background: var(--surface);
  border: none;
  border-radius: 10px;
  padding: 12px 16px;
  color: var(--text-primary);
  box-shadow: inset -3px -3px 8px var(--shadow-light), inset 3px 3px 8px var(--shadow-dark);
  transition: all 0.3s ease;
}

.neuro-input:focus {
  outline: none;
  box-shadow: inset -3px -3px 8px var(--shadow-light), inset 3px 3px 8px var(--shadow-dark),
    0 0 0 3px rgba(52, 152, 219, 0.3);
}

/* Rating Stars */
.rating-stars {
  display: flex;
  gap: 8px;
  font-size: 24px;
}

.star {
  color: #ddd;
  cursor: pointer;
  transition: color 0.2s ease;
}

.star.active,
.star:hover {
  color: #f39c12;
}

/* Theme Toggle */
.theme-toggle {
  position: fixed;
  top: 20px;
  right: 20px;
  background: var(--surface);
  border: none;
  border-radius: 50%;
  width: 50px;
  height: 50px;
  cursor: pointer;
  box-shadow: -5px -5px 10px var(--shadow-light), 5px 5px 10px var(--shadow-dark);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  transition: all 0.3s ease;
}

.theme-toggle:hover {
  transform: rotate(180deg);
}
```

### 5.2 Theme Toggle JavaScript

Create `wwwroot/js/theme.js`:

```javascript
// Theme Management
const themeToggle = document.getElementById("themeToggle");
const html = document.documentElement;

// Load saved theme or detect system preference
const savedTheme = localStorage.getItem("theme");
const systemPrefersDark = window.matchMedia(
  "(prefers-color-scheme: dark)"
).matches;

if (savedTheme) {
  html.setAttribute("data-theme", savedTheme);
} else if (systemPrefersDark) {
  html.setAttribute("data-theme", "dark");
}

// Toggle theme
themeToggle?.addEventListener("click", () => {
  const currentTheme = html.getAttribute("data-theme");
  const newTheme = currentTheme === "dark" ? "light" : "dark";

  html.setAttribute("data-theme", newTheme);
  localStorage.setItem("theme", newTheme);

  // Update icon
  themeToggle.textContent = newTheme === "dark" ? "☀️" : "🌙";
});

// Set initial icon
if (themeToggle) {
  const currentTheme = html.getAttribute("data-theme");
  themeToggle.textContent = currentTheme === "dark" ? "☀️" : "🌙";
}
```

### 5.3 AI Chat Interface

Create `wwwroot/js/ai-chat.js`:

```javascript
// AI Question Answering
async function askAI(question, context) {
  const response = await fetch("/Lessons/AskAI", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      question: question,
      context: context,
    }),
  });

  const data = await response.json();
  return data;
}

// Initialize AI Chat
document.addEventListener("DOMContentLoaded", () => {
  const chatForm = document.getElementById("aiChatForm");
  const chatInput = document.getElementById("chatInput");
  const chatMessages = document.getElementById("chatMessages");

  chatForm?.addEventListener("submit", async (e) => {
    e.preventDefault();

    const question = chatInput.value.trim();
    if (!question) return;

    // Add user message
    addMessage(question, "user");
    chatInput.value = "";

    // Show loading
    const loadingId = addMessage("Thinking...", "ai", true);

    try {
      // Get lesson context (from page data)
      const context =
        document.getElementById("lessonContext")?.textContent || "";

      const result = await askAI(question, context);

      // Remove loading, add response
      removeMessage(loadingId);
      addMessage(result.answer, "ai");
    } catch (error) {
      removeMessage(loadingId);
      addMessage("Sorry, I encountered an error. Please try again.", "ai");
    }
  });
});

function addMessage(text, sender, isLoading = false) {
  const chatMessages = document.getElementById("chatMessages");
  const messageId = `msg-${Date.now()}`;

  const messageDiv = document.createElement("div");
  messageDiv.id = messageId;
  messageDiv.className = `chat-message ${sender} ${isLoading ? "loading" : ""}`;
  messageDiv.textContent = text;

  chatMessages.appendChild(messageDiv);
  chatMessages.scrollTop = chatMessages.scrollHeight;

  return messageId;
}

function removeMessage(messageId) {
  const message = document.getElementById(messageId);
  message?.remove();
}
```

---

## 6. Localization Setup

### 6.1 Resource Files Structure

Create resource files:

```
Resources/
├── Controllers/
│   ├── HomeController.en.resx
│   ├── TeachersController.en.resx
│   └── LessonsController.en.resx
└── Views/
    ├── Shared.en.resx
    └── Home.en.resx
```

### 6.2 Update Program.cs for Localization

```csharp
// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "id", "zh" };
    options.SetDefaultCulture("en")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// After app is built, use localization middleware
app.UseRequestLocalization();
```

---

## 7. Development Workflow

### 7.1 Initial Setup Commands

```powershell
# Clone/create project
git clone https://github.com/vendouple/RateMyTeacher.git
cd RateMyTeacher

# Copy environment file
Copy-Item .env.example .env

# Edit .env and add your Gemini API key
# Get API key from: https://aistudio.google.com/apikey

# Restore packages
dotnet restore

# Create database and apply migrations
dotnet ef database update

# Run the application
dotnet run
```

### 7.2 Database Migration Commands

```powershell
# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Add new migration (after model changes)
dotnet ef migrations add AddNewFeature

# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

---

## 8. Testing Strategy

### 8.1 Unit Tests Setup

```powershell
# Create test project
dotnet new xunit -n RateMyTeacher.Tests

# Add reference to main project
cd RateMyTeacher.Tests
dotnet add reference ../RateMyTeacher/RateMyTeacher.csproj

# Add testing packages
dotnet add package Moq
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package FluentAssertions
```

### 8.2 Example Test

```csharp
using Xunit;
using Moq;
using RateMyTeacher.Services;
using FluentAssertions;

public class GeminiServiceTests
{
    [Fact]
    public async Task GenerateLessonSummary_ShouldReturnSummary()
    {
        // Arrange
        var service = new GeminiService();
        var lessonContent = "Photosynthesis is the process...";
        var topics = "Photosynthesis, Plant Biology";

        // Act
        var result = await service.GenerateLessonSummary(lessonContent, topics);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("photosynthesis");
    }
}
```

---

## 9. Deployment Configuration

### 9.1 Publish for Production

```powershell
# Publish as self-contained executable
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

# Output will be in: bin/Release/net9.0/win-x64/publish/
```

### 9.2 Docker Support (Optional)

Create `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RateMyTeacher/RateMyTeacher.csproj", "RateMyTeacher/"]
RUN dotnet restore "RateMyTeacher/RateMyTeacher.csproj"
COPY . .
WORKDIR "/src/RateMyTeacher"
RUN dotnet build "RateMyTeacher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RateMyTeacher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RateMyTeacher.dll"]
```

---

## 10. Performance Optimization

### 10.1 Caching Strategy

```csharp
// Add memory caching in Program.cs
builder.Services.AddMemoryCache();

// Use caching in services
public class TeacherService : ITeacherService
{
    private readonly IMemoryCache _cache;

    public async Task<List<Teacher>> GetTopTeachersAsync()
    {
        return await _cache.GetOrCreateAsync("top-teachers", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            return await _dbContext.Teachers
                .OrderByDescending(t => t.AverageRating)
                .Take(10)
                .ToListAsync();
        });
    }
}
```

### 10.2 Query Optimization

```csharp
// Use AsNoTracking for read-only queries
var teachers = await _dbContext.Teachers
    .AsNoTracking()
    .Include(t => t.Ratings)
    .ToListAsync();

// Use Select to fetch only needed fields
var teacherNames = await _dbContext.Teachers
    .Select(t => new { t.TeacherId, t.FirstName, t.LastName })
    .ToListAsync();
```

---

## 11. Security Checklist

- [ ] Add API key validation for Gemini service
- [ ] Implement user authentication (ASP.NET Identity)
- [ ] Add CSRF protection to all forms
- [ ] Sanitize all user inputs
- [ ] Implement rate limiting for AI endpoints
- [ ] Add SQL injection protection (using EF Core)
- [ ] Secure .env file (never commit to git)
- [ ] Add HTTPS enforcement
- [ ] Implement proper password hashing
- [ ] Add logging for security events

---

## 12. Phase 1 Implementation Checklist (MVP)

### Week 1-2: Foundation

- [ ] Create ASP.NET Core 9 MVC project
- [ ] Configure SQLite database
- [ ] Set up Entity Framework Core
- [ ] Create core models (User, Teacher, Student, Rating)
- [ ] Configure .env file with Gemini API key
- [ ] Implement basic authentication

### Week 3-4: Core Features

- [ ] Implement teacher listing page
- [ ] Create teacher rating system
- [ ] Add rating validation (one per student per semester)
- [ ] Calculate average ratings
- [ ] Implement ranking system
- [ ] Create bonus calculation logic

### Week 5-6: AI Integration

- [ ] Integrate Gemini 2.5 Flash API
- [ ] Implement lesson summary generation
- [ ] Add AI question answering feature
- [ ] Create quiz generation endpoint
- [ ] Add sentiment analysis for feedback

### Week 7-8: UI/UX

- [ ] Implement neuromorphic design system
- [ ] Add dark/light mode toggle
- [ ] Create responsive layouts
- [ ] Design rating interface with stars
- [ ] Add loading states and animations
- [ ] Implement form validations

### Week 9-10: Testing & Deployment

- [ ] Write unit tests for services
- [ ] Test AI integration thoroughly
- [ ] Perform security audit
- [ ] Optimize database queries
- [ ] Create deployment package
- [ ] Write user documentation

---

## 13. API Costs & Limits (Gemini 2.5 Flash)

### Free Tier Limits

- **Rate Limit**: 15 requests per minute (RPM)
- **Daily Limit**: 1,500 requests per day
- **Input Tokens**: Free (up to limit)
- **Output Tokens**: Free (up to limit)
- **Context Window**: 1 million tokens
- **Cost**: $0

### Paid Tier (if scaling)

- **Input**: $0.075 per 1M tokens
- **Output**: $0.30 per 1M tokens
- **Rate Limit**: Much higher (60+ RPM)

### Recommendations for Demo

- Use **free tier** for development and demo
- Implement caching to reduce API calls
- Add rate limiting on client side
- Queue AI requests during high load
- Consider upgrading if > 1,500 requests/day

---

## 14. File Structure Summary

```
RateMyTeacher/
├── .env                          # Environment variables (API keys)
├── .env.example                  # Template for .env
├── .gitignore                    # Git ignore file
├── appsettings.json              # App configuration
├── Program.cs                    # Application entry point
├── Controllers/
│   ├── HomeController.cs
│   ├── TeachersController.cs
│   ├── LessonsController.cs
│   ├── GradesController.cs
│   └── StudentsController.cs
├── Models/
│   ├── User.cs
│   ├── Teacher.cs
│   ├── Student.cs
│   ├── Rating.cs
│   ├── Lesson.cs
│   ├── Grade.cs
│   └── ... (other models)
├── Services/
│   ├── IGeminiService.cs
│   ├── GeminiService.cs
│   ├── ITeacherService.cs
│   ├── TeacherService.cs
│   └── ... (other services)
├── Data/
│   ├── ApplicationDbContext.cs
│   └── ratemyteacher.db         # SQLite database file
├── Views/
│   ├── Home/
│   ├── Teachers/
│   ├── Lessons/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   └── _ViewImports.cshtml
├── wwwroot/
│   ├── css/
│   │   ├── site.css
│   │   └── neuromorphic.css
│   ├── js/
│   │   ├── site.js
│   │   ├── theme.js
│   │   └── ai-chat.js
│   ├── images/
│   └── lib/
├── Resources/                    # Localization files
│   ├── Controllers/
│   └── Views/
└── Migrations/                   # EF Core migrations
```

---

## 15. Getting Started Guide

### Prerequisites

1. .NET 9 SDK installed
2. Visual Studio Code or Visual Studio 2022
3. Gemini API key from [Google AI Studio](https://aistudio.google.com/apikey)

### Quick Start

1. **Clone the repository** (or create new project)
2. **Copy `.env.example` to `.env`**
3. **Add your Gemini API key** to `.env`
4. **Run**: `dotnet restore`
5. **Run**: `dotnet ef database update`
6. **Run**: `dotnet run`
7. **Open**: https://localhost:5001

### First Time Setup

1. Create admin account
2. Add sample teachers and students
3. Test rating system
4. Try AI features (summary, Q&A, quiz generation)
5. Toggle dark/light mode
6. Test on mobile devices

---

## 16. Troubleshooting

### Common Issues

**Issue**: Gemini API key not found

- **Solution**: Ensure `.env` file exists and contains `GEMINI_API_KEY=your_key`

**Issue**: Database not created

- **Solution**: Run `dotnet ef database update`

**Issue**: AI requests timing out

- **Solution**: Check internet connection, verify API key, check rate limits

**Issue**: Dark mode not working

- **Solution**: Clear browser cache, check `theme.js` is loaded

**Issue**: Rating not saving

- **Solution**: Check ModelState validation, verify student is enrolled

---

## Next Steps

After completing Phase 1 MVP, proceed with:

1. **Phase 2**: Advanced features (voice-to-text, advanced analytics)
2. **Phase 3**: Mobile app (optional)
3. **Phase 4**: Multi-school support
4. **Production**: Deploy to Azure/AWS

---

_Document Version: 1.0_  
_Last Updated: October 22, 2025_  
_Technology: ASP.NET Core 9 MVC + SQLite + Gemini 2.5 Flash_
