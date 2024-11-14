using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ordreChange.Migrations
{
    /// <inheritdoc />
    public partial class AjoutChampsActionDansHistoriqueOrdre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "HistoriqueOrdres",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "HistoriqueOrdres");
        }
    }
}
