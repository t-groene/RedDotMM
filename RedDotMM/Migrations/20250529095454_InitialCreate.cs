using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDotMM.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wettbewerbe",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Datum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Probeschuss = table.Column<int>(type: "INTEGER", nullable: false),
                    Wertung = table.Column<int>(type: "INTEGER", nullable: false),
                    Teilerwertung = table.Column<bool>(type: "INTEGER", nullable: false),
                    SchussGeld = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wettbewerbe", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "Schuetzen",
                columns: table => new
                {
                    SchuetzenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WettbewerbID = table.Column<Guid>(type: "TEXT", nullable: false),
                    LfdNummer = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Vorname = table.Column<string>(type: "TEXT", nullable: false),
                    Zusatz = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schuetzen", x => x.SchuetzenId);
                    table.ForeignKey(
                        name: "FK_Schuetzen_Wettbewerbe_WettbewerbID",
                        column: x => x.WettbewerbID,
                        principalTable: "Wettbewerbe",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ergebnisse",
                columns: table => new
                {
                    ErgebnisId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Zeitstempel = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SchuetzeID = table.Column<Guid>(type: "TEXT", nullable: false),
                    LfdNummer = table.Column<int>(type: "INTEGER", nullable: false),
                    SchussGeldBezahlt = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErgebnisAbgeschlossen = table.Column<bool>(type: "INTEGER", nullable: false),
                    WettbewerbGuid = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ergebnisse", x => x.ErgebnisId);
                    table.ForeignKey(
                        name: "FK_Ergebnisse_Schuetzen_SchuetzeID",
                        column: x => x.SchuetzeID,
                        principalTable: "Schuetzen",
                        principalColumn: "SchuetzenId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ergebnisse_Wettbewerbe_WettbewerbGuid",
                        column: x => x.WettbewerbGuid,
                        principalTable: "Wettbewerbe",
                        principalColumn: "Guid");
                });

            migrationBuilder.CreateTable(
                name: "Schuesse",
                columns: table => new
                {
                    SchussId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Zeitstempel = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LfdSchussNummer = table.Column<int>(type: "INTEGER", nullable: false),
                    Wert = table.Column<double>(type: "REAL", nullable: false),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false),
                    Distanz = table.Column<double>(type: "REAL", nullable: false),
                    Typ = table.Column<int>(type: "INTEGER", nullable: false),
                    ErgebnisID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schuesse", x => x.SchussId);
                    table.ForeignKey(
                        name: "FK_Schuesse_Ergebnisse_ErgebnisID",
                        column: x => x.ErgebnisID,
                        principalTable: "Ergebnisse",
                        principalColumn: "ErgebnisId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ergebnisse_SchuetzeID",
                table: "Ergebnisse",
                column: "SchuetzeID");

            migrationBuilder.CreateIndex(
                name: "IX_Ergebnisse_WettbewerbGuid",
                table: "Ergebnisse",
                column: "WettbewerbGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Schuesse_ErgebnisID",
                table: "Schuesse",
                column: "ErgebnisID");

            migrationBuilder.CreateIndex(
                name: "IX_Schuetzen_WettbewerbID",
                table: "Schuetzen",
                column: "WettbewerbID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schuesse");

            migrationBuilder.DropTable(
                name: "Ergebnisse");

            migrationBuilder.DropTable(
                name: "Schuetzen");

            migrationBuilder.DropTable(
                name: "Wettbewerbe");
        }
    }
}
