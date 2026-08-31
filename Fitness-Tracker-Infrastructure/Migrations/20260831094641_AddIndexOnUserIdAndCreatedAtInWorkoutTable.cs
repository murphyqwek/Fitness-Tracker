using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitness_Tracker_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexOnUserIdAndCreatedAtInWorkoutTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workouts_UserId",
                table: "Workouts");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Workouts",
                newName: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_UserId_CreatedAt",
                table: "Workouts",
                columns: new[] { "UserId", "CreatedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workouts_UserId_CreatedAt",
                table: "Workouts");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Workouts",
                newName: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_UserId",
                table: "Workouts",
                column: "UserId");
        }
    }
}
