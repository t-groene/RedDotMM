using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDotMM.Win.Migrations
{
    /// <inheritdoc />
    public partial class LfdInSerie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LfdInSerie",
                table: "Ergebnisse",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LfdInSerie",
                table: "Ergebnisse");
        }
    }
}
