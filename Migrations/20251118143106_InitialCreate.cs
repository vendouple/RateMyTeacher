using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RateMyTeacher.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AcademicYear = table.Column<string>(type: "TEXT", nullable: false),
                    IsCurrent = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    AvatarUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TimeZone = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Locale = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedById = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Users_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AIUsageLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: true),
                    Query = table.Column<string>(type: "TEXT", nullable: false),
                    Response = table.Column<string>(type: "TEXT", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIUsageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIUsageLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BonusConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MinimumRatingsThreshold = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 20),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedById = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusConfigs_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    GradeLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentContact = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    Homeroom = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    EnrollmentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HireDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowAiTools = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teachers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BonusTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConfigId = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: true),
                    RangeStart = table.Column<int>(type: "INTEGER", nullable: true),
                    RangeEnd = table.Column<int>(type: "INTEGER", nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusTiers", x => x.Id);
                    table.CheckConstraint("CK_BonusTier_PositionOrRange", "(Position IS NOT NULL) OR (RangeStart IS NOT NULL AND RangeEnd IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_BonusTiers_BonusConfigs_ConfigId",
                        column: x => x.ConfigId,
                        principalTable: "BonusConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AISummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeacherId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: true),
                    LessonNotes = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AISummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AISummaries_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    TeacherId = table.Column<int>(type: "INTEGER", nullable: false),
                    SemesterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Stars = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.Id);
                    table.CheckConstraint("CK_Ratings_Stars", "Stars BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_Ratings_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ratings_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ratings_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherRankings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeacherId = table.Column<int>(type: "INTEGER", nullable: false),
                    SemesterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageRating = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: false),
                    TotalRatings = table.Column<int>(type: "INTEGER", nullable: false),
                    BonusAmount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherRankings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherRankings_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherRankings_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SentimentAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RatingId = table.Column<int>(type: "INTEGER", nullable: false),
                    Sentiment = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Confidence = table.Column<int>(type: "INTEGER", nullable: false),
                    KeywordsJson = table.Column<string>(type: "TEXT", nullable: true),
                    AnalyzedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentimentAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SentimentAnalyses_Ratings_RatingId",
                        column: x => x.RatingId,
                        principalTable: "Ratings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AISummaries_TeacherId",
                table: "AISummaries",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_AIUsageLogs_UserId",
                table: "AIUsageLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusConfigs_ModifiedById",
                table: "BonusConfigs",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_BonusTiers_ConfigId",
                table: "BonusTiers",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_SemesterId",
                table: "Ratings",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_StudentId_TeacherId_SemesterId",
                table: "Ratings",
                columns: new[] { "StudentId", "TeacherId", "SemesterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_TeacherId",
                table: "Ratings",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_Name",
                table: "Semesters",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SentimentAnalyses_RatingId",
                table: "SentimentAnalyses",
                column: "RatingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherRankings_SemesterId",
                table: "TeacherRankings",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherRankings_TeacherId_SemesterId",
                table: "TeacherRankings",
                columns: new[] { "TeacherId", "SemesterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_UserId",
                table: "Teachers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeletedById",
                table: "Users",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsDeleted",
                table: "Users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AISummaries");

            migrationBuilder.DropTable(
                name: "AIUsageLogs");

            migrationBuilder.DropTable(
                name: "BonusTiers");

            migrationBuilder.DropTable(
                name: "SentimentAnalyses");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "TeacherRankings");

            migrationBuilder.DropTable(
                name: "BonusConfigs");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "Semesters");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
