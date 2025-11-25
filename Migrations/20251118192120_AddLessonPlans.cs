using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RateMyTeacher.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LessonPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeacherId = table.Column<int>(type: "INTEGER", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    GradeLevel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TopicFocus = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    StudentNeeds = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    SectionsJson = table.Column<string>(type: "TEXT", nullable: false),
                    ResourcesJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonPlans_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_TeacherId",
                table: "LessonPlans",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonPlans");
        }
    }
}
