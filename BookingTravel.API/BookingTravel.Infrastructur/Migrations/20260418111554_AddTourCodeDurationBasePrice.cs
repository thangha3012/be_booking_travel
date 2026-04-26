using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingTravel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourCodeDurationBasePrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Tours",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "Tours",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TourCode",
                table: "Tours",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "TourCode",
                table: "Tours");
        }
    }
}
