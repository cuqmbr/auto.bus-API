using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    public partial class Change_Refresh_Token_Property_Names : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Expires",
                table: "RefreshToken",
                newName: "ExpiryDateTime");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "RefreshToken",
                newName: "CreationDateTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpiryDateTime",
                table: "RefreshToken",
                newName: "Expires");

            migrationBuilder.RenameColumn(
                name: "CreationDateTime",
                table: "RefreshToken",
                newName: "Created");
        }
    }
}
