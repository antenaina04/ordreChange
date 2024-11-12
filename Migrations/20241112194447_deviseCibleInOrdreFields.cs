using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ordreChange.Migrations
{
    /// <inheritdoc />
    public partial class deviseCibleInOrdreFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviseCible",
                table: "Ordres",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviseCible",
                table: "Ordres");
        }
    }
}
