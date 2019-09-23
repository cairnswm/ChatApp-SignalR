using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatApp.Migrations
{
    public partial class C : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConnectionID",
                table: "UserSessions",
                newName: "SessionID");

            migrationBuilder.AddColumn<string>(
                name: "DeviceID",
                table: "UserSessions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceID",
                table: "UserSessions");

            migrationBuilder.RenameColumn(
                name: "SessionID",
                table: "UserSessions",
                newName: "ConnectionID");
        }
    }
}
