using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fintess_Tracker_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoodId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MoodId",
                table: "FitnesTests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoodId",
                table: "FitnesTests");
        }
    }
}
