using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GasStationApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserKarma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ADD COLUMN IF NOT EXISTS karma integer DEFAULT 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "karma",
                table: "AspNetUsers");
        }
    }
}