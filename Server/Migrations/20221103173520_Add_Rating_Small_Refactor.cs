using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    public partial class Add_Rating_Small_Refactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntendedDepartureTimeOnlyUtc",
                table: "Routes");

            migrationBuilder.AddColumn<bool>(
                name: "HasBelts",
                table: "Vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasClimateControl",
                table: "Vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasOutlet",
                table: "Vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasStewardess",
                table: "Vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasTV",
                table: "Vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasWC",
                table: "Vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasWiFi",
                table: "Vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CancelationComment",
                table: "VehicleEnrollments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DepartureTimeOnlyUtc",
                table: "VehicleEnrollments",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                table: "VehicleEnrollments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    VehicleEnrollmentId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => new { x.UserId, x.VehicleEnrollmentId });
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_VehicleEnrollments_VehicleEnrollmentId",
                        column: x => x.VehicleEnrollmentId,
                        principalTable: "VehicleEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_VehicleEnrollmentId",
                table: "Reviews",
                column: "VehicleEnrollmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropColumn(
                name: "HasBelts",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasClimateControl",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasOutlet",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasStewardess",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasTV",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasWC",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HasWiFi",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CancelationComment",
                table: "VehicleEnrollments");

            migrationBuilder.DropColumn(
                name: "DepartureTimeOnlyUtc",
                table: "VehicleEnrollments");

            migrationBuilder.DropColumn(
                name: "IsCanceled",
                table: "VehicleEnrollments");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "IntendedDepartureTimeOnlyUtc",
                table: "Routes",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }
    }
}
