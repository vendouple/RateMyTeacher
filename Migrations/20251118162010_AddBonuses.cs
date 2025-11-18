using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RateMyTeacher.Migrations
{
    /// <inheritdoc />
    public partial class AddBonuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bonuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BatchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeacherId = table.Column<int>(type: "INTEGER", nullable: false),
                    SemesterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false),
                    AwardedAmount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    BaseAmount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    TierLabel = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SplitAcrossTies = table.Column<bool>(type: "INTEGER", nullable: false),
                    AwardedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AwardedById = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bonuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bonuses_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bonuses_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bonuses_Users_AwardedById",
                        column: x => x.AwardedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_AwardedById",
                table: "Bonuses",
                column: "AwardedById");

            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_BatchId",
                table: "Bonuses",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_SemesterId_Rank",
                table: "Bonuses",
                columns: new[] { "SemesterId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_TeacherId",
                table: "Bonuses",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bonuses");
        }
    }
}
