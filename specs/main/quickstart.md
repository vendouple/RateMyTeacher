# QuickStart Guide: RateMyTeacher

**Version**: 1.0.0  
**Last Updated**: 2025-10-23

---

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 10 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
  - Verify: `dotnet --version` (should show 9.0.x)
- **Git** (for version control)
- **VS Code** or **Visual Studio** (recommended IDEs)
- **Google Gemini API Key** ([Get one here](https://ai.google.dev/))

**Optional:**

- SQLite Browser ([DB Browser for SQLite](https://sqlitebrowser.org/)) for database inspection

---

## 1. Clone the Repository

```bash
git clone https://github.com/yourusername/RateMyTeacher.git
cd RateMyTeacher
```

---

## 2. Install Dependencies

Restore NuGet packages:

```bash
dotnet restore
```

**Expected packages:**

- Microsoft.EntityFrameworkCore (10.0)
- Microsoft.EntityFrameworkCore.Sqlite (10.0)
- Microsoft.EntityFrameworkCore.Tools (10.0)
- Google.GenerativeAI (latest)
- DotNetEnv (latest)

---

## 3. Configure Environment Variables

Create a `.env` file in the project root:

```bash
touch .env
```

Add your Gemini API key:

```env
GEMINI_API_KEY=your_api_key_here
```

**Important:**

- Never commit `.env` to version control (already in `.gitignore`)
- Get your API key from [Google AI Studio](https://ai.google.dev/)
- Free tier: 1500 requests/day, 1M tokens/month

---

## 4. Database Setup

### Apply Migrations

Run Entity Framework Core migrations to create the SQLite database:

```bash
dotnet ef database update
```

This will:

- Create `RateMyTeacher.db` in the project root
- Create all 28 database tables
- Apply foreign key constraints and indexes

**Verify database creation:**

```bash
ls -lh RateMyTeacher.db
# Should show a new SQLite database file
```

### Seed Default Data

The application automatically seeds:

- **Default Admin User**:
  - Email: `admin@school.com`
  - Password: `Admin@123` (change after first login!)
- **System Roles**: Admin (rank 100), Teacher (rank 50), Student (rank 10)
- **Default Permissions**: 50+ permissions across 8 categories
- **Current Semester**: Auto-detected or Fall 2024
- **Bonus Config**: Minimum 10 ratings, 1st: $10, 2nd: $5

**Seeding happens automatically on first run.** Check console output for confirmation.

---

## 5. Run the Application

### Development Mode

Start the application with hot-reload:

```bash
dotnet run
```

Or use `dotnet watch` for automatic restarts on file changes:

```bash
dotnet watch run
```

**Application URLs:**

- **HTTPS**: <https://localhost:5001> (recommended)
- **HTTP**: <http://localhost:5000>

**Console output should show:**

```text
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

## 6. Access the Application

Open your browser and navigate to:

👉 **[https://localhost:5001](https://localhost:5001)**

### First Login (Admin)

1. Click **Login** (top-right corner)
2. Enter default admin credentials:
   - **Email**: `admin@school.com`
   - **Password**: `Admin@123`
3. ⚠️ **Change password immediately** (Settings → Profile)

### Create Additional Users

As an admin, you can:

- Add teachers: **Users → Add Teacher**
- Add students: **Users → Add Student**
- Assign roles: **Users → Manage Roles**

---

## 7. Explore Key Features

### Phase 1 (P1) Features - Available Now

✅ **Teacher Ratings**

- Students rate teachers (1-5 stars)
- Optional comments
- One rating per teacher per semester

✅ **Leaderboard**

- View top-rated teachers
- Configure bonus tiers (Settings → Bonuses)
- Minimum 10 ratings to qualify (configurable)

✅ **AI Lesson Summaries**

- Teachers generate summaries from lesson notes
- Powered by Google Gemini 2.5 Flash
- Maximum 300 words

✅ **Authentication**

- Email/password login
- Role-based access (Admin/Teacher/Student)

### Phase 2 (P2) Features - Coming Soon

🔄 **Permission System**

- Discord-style multi-role assignments
- Hierarchical permissions
- Department support (optional)

🔄 **Attendance Tracking**

- Daily student attendance
- Teacher absence requests with approval

🔄 **AI Companion Controls**

- Three-level disable (Global/Admin/Teacher)

### Phase 3 (P3) Features - Future

📅 **Student LMS**

- Assignment submissions
- Grade tracking
- Note sharing with gamification
- Extra classes (Zoom/Meet links)

---

## 8. Common Tasks

### View Database

Using DB Browser for SQLite:

```bash
# Open database file
open RateMyTeacher.db
```

Or use command-line SQLite:

```bash
sqlite3 RateMyTeacher.db
sqlite> .tables
sqlite> SELECT * FROM Users;
```

### Create a New Migration

After modifying entity models:

```bash
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### Reset Database (⚠️ WARNING: Deletes all data)

```bash
dotnet ef database drop --force
dotnet ef database update
```

### Run Tests (when available)

```bash
dotnet test
```

---

## 9. Development Workflow

### Recommended VS Code Extensions

- **C# Dev Kit** (Microsoft)
- **SQLite Viewer** (alexcvzz)
- **.NET Core Test Explorer** (formulahendry)
- **Prettier** (for HTML/CSS/JS)

### Enable Hot Reload

In `Program.cs`, hot reload is enabled by default in Development mode. Changes to:

- Razor views (.cshtml)
- CSS files
- JavaScript files

...will apply immediately without restarting.

For C# code changes, use:

```bash
dotnet watch run
```

---

## 10. Troubleshooting

### Error: "Gemini API key not found"

**Solution**: Ensure `.env` file exists with `GEMINI_API_KEY=your_key_here`

Verify `.env` is loaded:

```bash
cat .env
# Should show your API key
```

### Error: "Database locked" (SQLite)

**Solution**: Close any database browsers and restart the application.

```bash
# Force close any SQLite processes
killall sqlite3 || true
dotnet run
```

### Error: "Migration already applied"

**Solution**: Drop and recreate database:

```bash
dotnet ef database drop --force
dotnet ef migrations remove  # Remove last migration
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Port 5001 Already in Use

**Solution**: Change port in `launchSettings.json`:

```json
"applicationUrl": "https://localhost:5555;http://localhost:5000"
```

### Can't Login (Invalid Credentials)

**Solution**: Check if database was seeded properly:

```bash
sqlite3 RateMyTeacher.db "SELECT Email, Role FROM Users WHERE Role = 'Admin';"
# Should show admin@school.com
```

If no admin exists, re-run migrations:

```bash
dotnet ef database drop --force
dotnet ef database update
```

---

## 11. Next Steps

Now that the application is running:

1. ✅ **Change admin password** (Settings → Profile)
2. 📝 **Create test users** (2-3 teachers, 5-10 students)
3. ⭐ **Submit test ratings** (students rate teachers)
4. 🏆 **View leaderboard** (verify rankings work)
5. 🤖 **Test AI summaries** (create lesson notes → generate summary)
6. ⚙️ **Configure bonuses** (Settings → Bonuses → Edit tiers)

### Development Tasks

See `specs/main/tasks.md` for detailed implementation tasks (generated via `/speckit.tasks`).

### Useful Commands

```bash
# Check project info
dotnet --info

# List all migrations
dotnet ef migrations list

# Generate SQL script from migrations (for review)
dotnet ef migrations script

# View active connections (SQLite)
sqlite3 RateMyTeacher.db ".databases"

# Backup database
cp RateMyTeacher.db RateMyTeacher.backup.db
```

---

## 12. Production Deployment (Future)

For production deployment, consider:

- **HTTPS**: Use Let's Encrypt for SSL certificates
- **Database**: Migrate to PostgreSQL or SQL Server for better concurrency
- **API Key Security**: Use Azure Key Vault or AWS Secrets Manager
- **Hosting**: Azure App Service, AWS Elastic Beanstalk, or Docker container
- **CI/CD**: GitHub Actions for automated testing and deployment

See `specs/main/plan.md` for detailed production considerations.

---

## Support & Resources

- **Documentation**: `docs/` folder (coming soon)
- **Constitution**: `.specify/memory/constitution.md` (project principles)
- **Spec**: `specs/main/spec.md` (feature requirements)
- **Plan**: `specs/main/plan.md` (implementation plan)
- **Research**: `specs/main/research.md` (technical decisions)

**Need help?** File an issue on GitHub or contact the development team.

---

Happy Coding! 🎓
