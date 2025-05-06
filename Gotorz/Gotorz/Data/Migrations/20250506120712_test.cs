using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gotorz.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResourceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResourceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowDetails = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightOffer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AirlineCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableSeats = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightOffer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hotel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalHotelId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Itineraries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Duration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlightOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itineraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Itineraries_FlightOffer_FlightOfferId",
                        column: x => x.FlightOfferId,
                        principalTable: "FlightOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HotelOffer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HotelDbId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RoomType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BedType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfBeds = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CancellationPolicy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelOffer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HotelOffer_Hotel_HotelDbId",
                        column: x => x.HotelDbId,
                        principalTable: "Hotel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TravelPackages",
                columns: table => new
                {
                    TravelPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutboundFlightId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnFlightId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Adults = table.Column<int>(type: "int", nullable: false),
                    OriginCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DestinationCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelPackages", x => x.TravelPackageId);
                    table.ForeignKey(
                        name: "FK_TravelPackages_FlightOffer_OutboundFlightId",
                        column: x => x.OutboundFlightId,
                        principalTable: "FlightOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TravelPackages_FlightOffer_ReturnFlightId",
                        column: x => x.ReturnFlightId,
                        principalTable: "FlightOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TravelPackages_Hotel_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlightSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItineraryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlightSegmentId = table.Column<int>(type: "int", nullable: false),
                    DepartureAirport = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalAirport = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CarrierCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlightNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AircraftCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stops = table.Column<int>(type: "int", nullable: false),
                    CabinClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckedBags = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightSegments_Itineraries_ItineraryId",
                        column: x => x.ItineraryId,
                        principalTable: "Itineraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlightSegments_ItineraryId",
                table: "FlightSegments",
                column: "ItineraryId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelOffer_HotelDbId",
                table: "HotelOffer",
                column: "HotelDbId");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_FlightOfferId",
                table: "Itineraries",
                column: "FlightOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelPackages_HotelId",
                table: "TravelPackages",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelPackages_OutboundFlightId",
                table: "TravelPackages",
                column: "OutboundFlightId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelPackages_ReturnFlightId",
                table: "TravelPackages",
                column: "ReturnFlightId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "FlightSegments");

            migrationBuilder.DropTable(
                name: "HotelOffer");

            migrationBuilder.DropTable(
                name: "TravelPackages");

            migrationBuilder.DropTable(
                name: "Itineraries");

            migrationBuilder.DropTable(
                name: "Hotel");

            migrationBuilder.DropTable(
                name: "FlightOffer");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");
        }
    }
}
