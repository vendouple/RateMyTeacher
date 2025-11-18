using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RateMyTeacher.Migrations
{
    /// <inheritdoc />
    public partial class AddAiGovernance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedAt",
                table: "AIUsageLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AIControlSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Scope = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ScopeId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedById = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIControlSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIControlSettings_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIUsageLogs_Timestamp",
                table: "AIUsageLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AIControlSettings_ModifiedById",
                table: "AIControlSettings",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_AIControlSettings_Scope_ScopeId",
                table: "AIControlSettings",
                columns: new[] { "Scope", "ScopeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIControlSettings");

            migrationBuilder.DropIndex(
                name: "IX_AIUsageLogs_Timestamp",
                table: "AIUsageLogs");

            migrationBuilder.DropColumn(
                name: "ViewedAt",
                table: "AIUsageLogs");
        }
    }
}
