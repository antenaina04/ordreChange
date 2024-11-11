using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ordreChange.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrdreFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateDerniereModification",
                table: "Ordres",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDerniereModification",
                table: "Ordres");
        }
    }
}
