using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDotMM.Migrations
{
    /// <inheritdoc />
    public partial class SchussGeld : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchussGeldBezahlt",
                table: "Ergebnisse");

            migrationBuilder.AddColumn<decimal>(
                name: "BezahltesSchussGeld",
                table: "Ergebnisse",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BezahltesSchussGeld",
                table: "Ergebnisse");

            migrationBuilder.AddColumn<bool>(
                name: "SchussGeldBezahlt",
                table: "Ergebnisse",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
