using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDotMM.Win.Migrations
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
                    AnzahlProbeschuss = table.Column<int>(type: "INTEGER", nullable: false),
                    ProbeNurAufErsterScheibe = table.Column<bool>(type: "INTEGER", nullable: false),
                    AnzahlWertungsSchuss = table.Column<int>(type: "INTEGER", nullable: false),
                    AnzahlSerien = table.Column<int>(type: "INTEGER", nullable: false),
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
                name: "Serien",
                columns: table => new
                {
                    SerienId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SchuetzeID = table.Column<Guid>(type: "TEXT", nullable: false),
                    SerieAbgeschlossen = table.Column<bool>(type: "INTEGER", nullable: false),
                    SchussgeldBezahlt = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Serien", x => x.SerienId);
                    table.ForeignKey(
                        name: "FK_Serien_Schuetzen_SchuetzeID",
                        column: x => x.SchuetzeID,
                        principalTable: "Schuetzen",
                        principalColumn: "SchuetzenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ergebnisse",
                columns: table => new
                {
                    ErgebnisId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Zeitstempel = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SerienID = table.Column<Guid>(type: "TEXT", nullable: false),
                    LfdNummer = table.Column<int>(type: "INTEGER", nullable: false),
                    BezahltesSchussGeld = table.Column<decimal>(type: "TEXT", nullable: false),
                    ErgebnisAbgeschlossen = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ergebnisse", x => x.ErgebnisId);
                    table.ForeignKey(
                        name: "FK_Ergebnisse_Serien_SerienID",
                        column: x => x.SerienID,
                        principalTable: "Serien",
                        principalColumn: "SerienId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schuesse",
                columns: table => new
                {
                    SchussId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Zeitstempel = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LfdSchussNummer = table.Column<int>(type: "INTEGER", nullable: false),
                    Wert = table.Column<decimal>(type: "TEXT", nullable: false),
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
                name: "IX_Ergebnisse_SerienID",
                table: "Ergebnisse",
                column: "SerienID");

            migrationBuilder.CreateIndex(
                name: "IX_Schuesse_ErgebnisID",
                table: "Schuesse",
                column: "ErgebnisID");

            migrationBuilder.CreateIndex(
                name: "IX_Schuetzen_WettbewerbID",
                table: "Schuetzen",
                column: "WettbewerbID");

            migrationBuilder.CreateIndex(
                name: "IX_Serien_SchuetzeID",
                table: "Serien",
                column: "SchuetzeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schuesse");

            migrationBuilder.DropTable(
                name: "Ergebnisse");

            migrationBuilder.DropTable(
                name: "Serien");

            migrationBuilder.DropTable(
                name: "Schuetzen");

            migrationBuilder.DropTable(
                name: "Wettbewerbe");
        }
    }
}
