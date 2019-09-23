using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatApp.Migrations
{
    public partial class Cha : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDeviceMessages_UserID_DeviceID",
                table: "UserDeviceMessages");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "UserDeviceMessages");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "UserDevice");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "UserDeviceMessages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "UserDevice",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDeviceMessages_UserName_DeviceID",
                table: "UserDeviceMessages",
                columns: new[] { "UserName", "DeviceID" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDeviceMessages_UserName_DeviceID",
                table: "UserDeviceMessages");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "UserDeviceMessages");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "UserDevice");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "UserDeviceMessages",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "UserDevice",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserDeviceMessages_UserID_DeviceID",
                table: "UserDeviceMessages",
                columns: new[] { "UserID", "DeviceID" });
        }
    }
}
