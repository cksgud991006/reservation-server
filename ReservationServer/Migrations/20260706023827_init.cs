using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationServer.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlightBookings",
                columns: table => new
                {
                    FlightId = table.Column<string>(type: "text", nullable: false),
                    SeatNumber = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightBookings", x => new { x.FlightId, x.SeatNumber });
                });

            migrationBuilder.CreateTable(
                name: "FlightInstances",
                columns: table => new
                {
                    FlightNumber = table.Column<string>(type: "text", nullable: false),
                    DepartureTime = table.Column<string>(type: "text", nullable: false),
                    FlightId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightInstances", x => new { x.FlightNumber, x.DepartureTime });
                });

            migrationBuilder.CreateTable(
                name: "FlightSeatCounts",
                columns: table => new
                {
                    FlightId = table.Column<string>(type: "text", nullable: false),
                    TotalSeatCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSeatCounts", x => x.FlightId);
                });

            migrationBuilder.CreateTable(
                name: "SeatLayouts",
                columns: table => new
                {
                    FlightNumber = table.Column<string>(type: "text", nullable: false),
                    SeatNumber = table.Column<string>(type: "text", nullable: false),
                    SeatClass = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatLayouts", x => new { x.FlightNumber, x.SeatNumber });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightBookings");

            migrationBuilder.DropTable(
                name: "FlightInstances");

            migrationBuilder.DropTable(
                name: "FlightSeatCounts");

            migrationBuilder.DropTable(
                name: "SeatLayouts");
        }
    }
}
