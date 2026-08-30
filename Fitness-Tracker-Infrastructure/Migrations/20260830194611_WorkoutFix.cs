using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitness_Tracker_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WorkoutFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutSets_Exercises_ExerciseId",
                table: "WorkoutSets");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Workouts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Workouts",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutSets_Exercises_ExerciseId",
                table: "WorkoutSets",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutSets_Exercises_ExerciseId",
                table: "WorkoutSets");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Workouts");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutSets_Exercises_ExerciseId",
                table: "WorkoutSets",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
