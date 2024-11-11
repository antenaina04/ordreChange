using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ordreChange.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAgentWithAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Agents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Agents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Agents");
        }
    }
}
