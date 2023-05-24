using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    public partial class Remove_IsCancelled_field_from_VehicleEnrollment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelayTimeSpan",
                table: "VehicleEnrollments");

            migrationBuilder.DropColumn(
                name: "IsCanceled",
                table: "VehicleEnrollments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "DelayTimeSpan",
                table: "VehicleEnrollments",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                table: "VehicleEnrollments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
