using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freddie.Migrations
{
    /// <inheritdoc />
    public partial class Freddie1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    ResumeUrl = table.Column<string>(type: "TEXT", nullable: false),
                    ResumeText = table.Column<string>(type: "TEXT", nullable: false),
                    KeyStrengths = table.Column<string>(type: "TEXT", nullable: false),
                    BiggestWeakness = table.Column<string>(type: "TEXT", nullable: false),
                    AvailableImmediately = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Contacted = table.Column<bool>(type: "INTEGER", nullable: true),
                    ContactedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidates", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CandidateId = table.Column<string>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    EvaluationNotes = table.Column<string>(type: "TEXT", nullable: false),
                    EvaluationDate = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluations_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_CandidateId",
                table: "Evaluations",
                column: "CandidateId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "Candidates");
        }
    }
}
