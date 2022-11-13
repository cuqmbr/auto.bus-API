using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    public partial class Merge_departure_DateOnly_and_TimeOnly_into_DateTime_in_VehicleEnrollment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartureDateOnly",
                table: "VehicleEnrollments");

            migrationBuilder.DropColumn(
                name: "DepartureTimeOnlyUtc",
                table: "VehicleEnrollments");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "DelayTimeSpan",
                table: "VehicleEnrollments",
                type: "interval",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "interval");

            migrationBuilder.AlterColumn<string>(
                name: "CancelationComment",
                table: "VehicleEnrollments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDateTimeUtc",
                table: "VehicleEnrollments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartureDateTimeUtc",
                table: "VehicleEnrollments");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "DelayTimeSpan",
                table: "VehicleEnrollments",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "interval",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CancelationComment",
                table: "VehicleEnrollments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DepartureDateOnly",
                table: "VehicleEnrollments",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DepartureTimeOnlyUtc",
                table: "VehicleEnrollments",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
