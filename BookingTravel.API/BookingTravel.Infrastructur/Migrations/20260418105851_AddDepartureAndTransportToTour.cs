using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingTravel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartureAndTransportToTour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartureLocation",
                table: "Tours",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Transport",
                table: "Tours",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartureLocation",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "Transport",
                table: "Tours");
        }
    }
}
