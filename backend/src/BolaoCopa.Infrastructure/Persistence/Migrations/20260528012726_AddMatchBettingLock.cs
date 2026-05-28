using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BolaoCopa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchBettingLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBettingLocked",
                table: "Matches",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBettingLocked",
                table: "Matches");
        }
    }
}
