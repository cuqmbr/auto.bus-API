using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Server.Migrations
{
    public partial class Decouple_RouteAddress_and_RouteAddressDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostToNextCity",
                table: "RouteAddresses");

            migrationBuilder.DropColumn(
                name: "TimeSpanToNextCity",
                table: "RouteAddresses");

            migrationBuilder.DropColumn(
                name: "WaitTimeSpan",
                table: "RouteAddresses");

            migrationBuilder.AddColumn<int>(
                name: "RouteAddressDetailsId",
                table: "VehicleEnrollments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RouteAddressDetailsId",
                table: "RouteAddresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RouteAddressDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleEnrollmentId = table.Column<int>(type: "integer", nullable: false),
                    RouteAddressId = table.Column<int>(type: "integer", nullable: false),
                    TimeSpanToNextCity = table.Column<TimeSpan>(type: "interval", nullable: false),
                    WaitTimeSpan = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CostToNextCity = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteAddressDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteAddressDetails_RouteAddresses_RouteAddressId",
                        column: x => x.RouteAddressId,
                        principalTable: "RouteAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteAddressDetails_VehicleEnrollments_VehicleEnrollmentId",
                        column: x => x.VehicleEnrollmentId,
                        principalTable: "VehicleEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RouteAddressDetails_RouteAddressId",
                table: "RouteAddressDetails",
                column: "RouteAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteAddressDetails_VehicleEnrollmentId",
                table: "RouteAddressDetails",
                column: "VehicleEnrollmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteAddressDetails");

            migrationBuilder.DropColumn(
                name: "RouteAddressDetailsId",
                table: "VehicleEnrollments");

            migrationBuilder.DropColumn(
                name: "RouteAddressDetailsId",
                table: "RouteAddresses");

            migrationBuilder.AddColumn<double>(
                name: "CostToNextCity",
                table: "RouteAddresses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeSpanToNextCity",
                table: "RouteAddresses",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WaitTimeSpan",
                table: "RouteAddresses",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
