using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ordreChange.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    IdAgent = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.IdAgent);
                });

            migrationBuilder.CreateTable(
                name: "Ordres",
                columns: table => new
                {
                    IdOrdre = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Montant = table.Column<float>(type: "real", nullable: false),
                    Devise = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TypeTransaction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontantConverti = table.Column<float>(type: "real", nullable: false),
                    IdAgent = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordres", x => x.IdOrdre);
                    table.ForeignKey(
                        name: "FK_Ordres_Agents_IdAgent",
                        column: x => x.IdAgent,
                        principalTable: "Agents",
                        principalColumn: "IdAgent",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoriqueOrdres",
                columns: table => new
                {
                    IdHistorique = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Montant = table.Column<float>(type: "real", nullable: false),
                    IdOrdre = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriqueOrdres", x => x.IdHistorique);
                    table.ForeignKey(
                        name: "FK_HistoriqueOrdres_Ordres_IdOrdre",
                        column: x => x.IdOrdre,
                        principalTable: "Ordres",
                        principalColumn: "IdOrdre",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueOrdres_IdOrdre",
                table: "HistoriqueOrdres",
                column: "IdOrdre");

            migrationBuilder.CreateIndex(
                name: "IX_Ordres_IdAgent",
                table: "Ordres",
                column: "IdAgent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoriqueOrdres");

            migrationBuilder.DropTable(
                name: "Ordres");

            migrationBuilder.DropTable(
                name: "Agents");
        }
    }
}
