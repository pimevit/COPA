using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BolaoCopa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserBetsPublicByDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "ShowBetsPublicly",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.Sql("UPDATE [Users] SET [ShowBetsPublicly] = CAST(1 AS bit);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "ShowBetsPublicly",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.Sql("UPDATE [Users] SET [ShowBetsPublicly] = CAST(0 AS bit);");
        }
    }
}
