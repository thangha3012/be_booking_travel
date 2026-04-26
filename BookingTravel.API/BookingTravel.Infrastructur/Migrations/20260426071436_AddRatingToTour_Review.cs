using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingTravel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingToTour_Review : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Tours",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Tours");
        }
    }
}
