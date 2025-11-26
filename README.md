> [!CAUTION]
> This is a demo application. Its very buggy, 100% vibe-coded. As a school project. This is now archived for documentation purposes only.
# RateMyTeacher - Teacher Rating & Leaderboard System

A modern ASP.NET Core 10 MVC application for student-teacher engagement with AI-powered features, ratings, and leaderboards.

## 🎯 Overview

RateMyTeacher is a demo application that enables students to rate teachers, view leaderboards, and receive AI-powered insights using Google Gemini 2.5 Flash. The application features a neuromorphic UI design with dark/light mode support and multi-language localization.

## 🏗️ Architecture

- **Framework**: ASP.NET Core 10 MVC (LTS)
- **Database**: SQLite (embedded, no external installation)
- **AI Model**: Google Gemini 2.5 Flash
- **Frontend**: Razor Views, vanilla JavaScript, Chart.js
- **Design**: Neuromorphic UI with dark/light theme support
- **Localization**: ASP.NET Core i18n (base: English)

## 📦 Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Google Gemini API Key](https://makersuite.google.com/app/apikey) (free tier available)
- Git (for version control)

## 🚀 Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd RateMyTeacher
```

### 2. Configure Environment Variables

Copy the example environment file and configure your settings:

```bash
cp .env.example .env
```

Edit `.env` and add your Google Gemini API key:

```env
GEMINI_API_KEY=your_actual_api_key_here
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Apply Database Migrations

```bash
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The application will be available at:

- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## 🔧 Configuration

### Environment Variables

| Variable                  | Description                 | Default                 |
| ------------------------- | --------------------------- | ----------------------- |
| `GEMINI_API_KEY`          | Google Gemini API key       | _Required_              |
| `DATABASE_PATH`           | SQLite database location    | `Data/ratemyteacher.db` |
| `FIRST_PLACE_BONUS`       | Bonus for 1st place teacher | `10.00`                 |
| `SECOND_PLACE_BONUS`      | Bonus for 2nd place teacher | `5.00`                  |
| `MINIMUM_VOTES_THRESHOLD` | Min votes for leaderboard   | `20`                    |
| `AI_TEMPERATURE`          | AI creativity (0.0-1.0)     | `0.7`                   |
| `AI_TIMEOUT_SECONDS`      | AI request timeout          | `30`                    |
| `BONUS_TIE_STRATEGY`      | Tie handling mode           | `split`                 |
| `AI_MAX_SUMMARY_WORDS`    | Target summary size         | `300`                   |

### Gemini API Limits (Free Tier)

- **Daily Requests**: 1,500 requests/day
- **Rate Limit**: 15 requests/minute
- **Context Window**: 1 million tokens

## 📋 Features

### Core Features (MVP)

✅ **User Story 1: Teacher Listing & Ratings (P1)**

- Browse all teachers
- Submit ratings (1-5 stars)
- Add text reviews
- View average ratings

✅ **User Story 2: Leaderboard & Bonuses (P1)**

- Top teachers ranked by rating
- 1st place: $10 bonus
- 2nd place: $5 bonus
- Minimum 20 votes required

### Advanced Features

🔄 **User Story 3: AI-Powered Lesson Summaries (P2)**

- Generate lesson summaries with Gemini
- Chat with lesson content
- Q&A functionality

🔄 **User Story 4: Performance Analytics (P2)**

- Student performance tracking
- Grade analysis charts
- Teacher effectiveness metrics

🔄 **User Story 5: Smart Scheduling (P3)**

- AI-optimized schedules
- Conflict detection
- Attendance tracking

## 🎨 Design Principles

This application follows strict design principles defined in [`PRINCIPLES.md`](./PRINCIPLES.md):

### Code Quality

- Clean Architecture (Controller → Service → Repository)
- SOLID principles
- Dependency Injection
- Async/await for I/O operations

### UX Design

- **Neuromorphic Design**: Soft shadows, subtle elevation
- **Dark/Light Mode**: CSS custom properties
- **Responsive**: Mobile-first approach
- **Accessibility**: ARIA labels, semantic HTML

## 📁 Project Structure

```text
RateMyTeacher/
├── Controllers/          # MVC Controllers
├── Models/              # Data models & ViewModels
├── Services/            # Business logic
├── Data/                # DbContext & migrations
│   └── ratemyteacher.db # SQLite database
├── Views/               # Razor views
│   ├── Home/
│   ├── Teachers/
│   ├── Ratings/
│   └── Shared/
├── wwwroot/             # Static files
│   ├── css/
│   ├── js/
│   └── lib/
├── Resources/           # Localization files
├── specs/               # Project specifications
│   └── main/
│       ├── plan.md      # Implementation plan
│       ├── spec.md      # User stories
│       └── tasks.md     # Task breakdown (47 tasks)
├── .env.example         # Environment template
├── PRINCIPLES.md        # Design constitution
├── SPECIFICATION.md     # Full feature spec
└── IMPLEMENTATION_PLAN.md # Technical guide
```

## 📝 Development Workflow

This project follows a task-driven development approach. See [`specs/main/tasks.md`](./specs/main/tasks.md) for the complete task breakdown (65 tasks across 8 phases).

### Task Phases

1. **Phase 1: Setup** (T001-T006) ✅

   - Project initialization
   - NuGet packages
   - Environment configuration

2. **Phase 2: Foundational** (T007-T012)

   - Database models
   - Authentication
   - Base layout

3. **Phase 3: US1 - Teacher Ratings** (T013-T020)

   - Teacher CRUD
   - Rating system
   - Review moderation

4. **Phase 4: US2 - Leaderboard** (T021-T025)

   - Ranking algorithm
   - Bonus calculation
   - Charts

5. **Phase 5-7: Advanced Features** (T026-T047)
   - AI integration
   - Analytics
   - Smart scheduling

## 🧪 Testing

```bash
# Run unit tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 🚢 Deployment

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Rollback to previous migration
dotnet ef database update PreviousMigrationName
```

### Production Build

```bash
dotnet publish -c Release -o ./publish
```

## 📚 Documentation

- **[PRINCIPLES.md](./PRINCIPLES.md)**: Code quality & UX guidelines
- **[SPECIFICATION.md](./SPECIFICATION.md)**: Complete feature specification
- **[IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md)**: Technical implementation guide
- **[specs/main/tasks.md](./specs/main/tasks.md)**: Detailed task breakdown

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Follow the principles in `PRINCIPLES.md`
4. Commit your changes (`git commit -m 'Add AmazingFeature'`)
5. Push to the branch (`git push origin feature/AmazingFeature`)
6. Open a Pull Request

## 📄 License

This is a demo application for educational purposes.

## 🙏 Acknowledgments

- **Google Gemini**: AI-powered features
- **ASP.NET Core Team**: Excellent framework
- **Chart.js**: Data visualization
- **Neuromorphism.io**: Design inspiration

## 🔗 Links

- [Project Specification](./SPECIFICATION.md)
- [Task Tracker](./specs/main/tasks.md)
- [Google Gemini Docs](https://ai.google.dev/docs)
- [ASP.NET Core Docs](https://learn.microsoft.com/en-us/aspnet/core/)

---

Built with ❤️ using ASP.NET Core 10 + Google Gemini 2.5 Flash.
