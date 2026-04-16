using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GasStationApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "reports",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reports_station_id_created_at",
                table: "reports",
                columns: new[] { "station_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_reports_station_id_created_at",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "price",
                table: "reports");

            migrationBuilder.CreateIndex(
                name: "IX_reports_station_id",
                table: "reports",
                column: "station_id");
        }
    }
}
