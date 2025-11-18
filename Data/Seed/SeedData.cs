using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using RateMyTeacher.Models;

namespace RateMyTeacher.Data.Seed;

public static class SeedData
{
    /// <summary>
    /// Placeholder seeding hook so Program.cs can safely ensure initial data.
    /// Will be expanded in T024+ once concrete seed requirements are finalized.
    /// </summary>
    public static void Initialize(ApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var currentSemester = EnsureSemester(context);

        var passwordHasher = new PasswordHasher<User>();
        _ = EnsureUser(
            context,
            passwordHasher,
            email: "admin@school.com",
            firstName: "Sam",
            lastName: "Administrator",
            role: "Admin");

        var teacherUser = EnsureUser(
            context,
            passwordHasher,
            email: "teacher.one@school.com",
            firstName: "Jordan",
            lastName: "Lee",
            role: "Teacher");

        var studentUser = EnsureUser(
            context,
            passwordHasher,
            email: "student.one@school.com",
            firstName: "Casey",
            lastName: "Morgan",
            role: "Student");

        var teacherProfile = EnsureTeacher(context, teacherUser, department: "Mathematics");
        var studentProfile = EnsureStudent(context, studentUser, gradeLevel: 10);

        EnsureSystemSettings(context);

        if (!context.Ratings.Any())
        {
            context.Ratings.Add(new Rating
            {
                StudentId = studentProfile.Id,
                TeacherId = teacherProfile.Id,
                SemesterId = currentSemester.Id,
                Stars = 5,
                Comment = "Explains complex topics clearly and keeps lessons engaging.",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            });
        }

        context.SaveChanges();
    }

    private static void EnsureSystemSettings(ApplicationDbContext context)
    {
        var descriptions = new Dictionary<string, string>
        {
            [SystemSetting.Keys.Ai.GlobalEnabled] = "Enables or disables AI features globally.",
            [SystemSetting.Keys.Ai.TeacherMode] = "Choose Unrestricted, Guided, or Off for teachers.",
            [SystemSetting.Keys.Ai.StudentMode] = "Choose Learning, Unrestricted, or Off for students."
        };

        foreach (var (key, defaultValue) in SystemSetting.Defaults.Ai)
        {
            var setting = context.SystemSettings.FirstOrDefault(s => s.Key == key);
            if (setting is null)
            {
                setting = new SystemSetting
                {
                    Key = key,
                    Value = defaultValue,
                    Description = descriptions.GetValueOrDefault(key),
                    ModifiedAt = DateTime.UtcNow
                };
                context.SystemSettings.Add(setting);
            }
        }

        context.SaveChanges();
    }

    private static Semester EnsureSemester(ApplicationDbContext context)
    {
        var semester = context.Semesters.FirstOrDefault(s => s.IsCurrent);
        if (semester is not null)
        {
            return semester;
        }

        semester = context.Semesters
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefault();

        if (semester is null)
        {
            semester = new Semester
            {
                Name = "Fall 2025",
                AcademicYear = "2025/2026",
                StartDate = new DateOnly(2025, 8, 1),
                EndDate = new DateOnly(2025, 12, 31),
                IsCurrent = true
            };
            context.Semesters.Add(semester);
            context.SaveChanges();
            return semester;
        }

        semester.IsCurrent = true;
        context.SaveChanges();
        return semester;
    }

    private static User EnsureUser(
        ApplicationDbContext context,
        PasswordHasher<User> passwordHasher,
        string email,
        string firstName,
        string lastName,
        string role)
    {
        var user = context.Users.FirstOrDefault(u => u.Email == email);
        if (user is not null)
        {
            return user;
        }

        user = new User
        {
            Email = email,
            Username = email.Split('@')[0],
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            TimeZone = "America/New_York",
            Locale = "en-US"
        };

        user.PasswordHash = passwordHasher.HashPassword(user, "P@ssw0rd!");
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    private static Teacher EnsureTeacher(ApplicationDbContext context, User user, string department)
    {
        var teacher = context.Teachers.FirstOrDefault(t => t.UserId == user.Id);
        if (teacher is not null)
        {
            return teacher;
        }

        teacher = new Teacher
        {
            UserId = user.Id,
            Department = department,
            Bio = "Passionate about making calculus approachable and fun.",
            ProfileImageUrl = "https://placehold.co/200x200",
            HireDate = DateTime.UtcNow.AddYears(-4)
        };

        context.Teachers.Add(teacher);
        context.SaveChanges();
        return teacher;
    }

    private static Student EnsureStudent(ApplicationDbContext context, User user, int gradeLevel)
    {
        var student = context.Students.FirstOrDefault(s => s.UserId == user.Id);
        if (student is not null)
        {
            return student;
        }

        student = new Student
        {
            UserId = user.Id,
            GradeLevel = gradeLevel,
            ParentContact = "+15555551234",
            StudentNumber = "STU-1001",
            Homeroom = "Hawk 10B"
        };

        context.Students.Add(student);
        context.SaveChanges();
        return student;
    }
}
