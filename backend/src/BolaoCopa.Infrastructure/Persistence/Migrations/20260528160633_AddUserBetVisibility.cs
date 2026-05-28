using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BolaoCopa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBetVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowBetsPublicly",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowBetsPublicly",
                table: "Users");
        }
    }
}
