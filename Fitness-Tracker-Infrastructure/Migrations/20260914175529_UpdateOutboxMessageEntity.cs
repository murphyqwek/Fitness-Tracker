using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitness_Tracker_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOutboxMessageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "OutboxMessages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "OutboxMessages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "OutboxMessages");
        }
    }
}
