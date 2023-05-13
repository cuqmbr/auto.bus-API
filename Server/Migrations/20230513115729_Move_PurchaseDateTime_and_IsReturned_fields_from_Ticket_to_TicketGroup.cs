using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    public partial class Move_PurchaseDateTime_and_IsReturned_fields_from_Ticket_to_TicketGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMissed",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PurchaseDateTimeUtc",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "CancelationComment",
                table: "VehicleEnrollments",
                newName: "CancellationComment");

            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "TicketGroups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDateTimeUtc",
                table: "TicketGroups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "TicketGroups");

            migrationBuilder.DropColumn(
                name: "PurchaseDateTimeUtc",
                table: "TicketGroups");

            migrationBuilder.RenameColumn(
                name: "CancellationComment",
                table: "VehicleEnrollments",
                newName: "CancelationComment");

            migrationBuilder.AddColumn<bool>(
                name: "IsMissed",
                table: "Tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "Tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDateTimeUtc",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
