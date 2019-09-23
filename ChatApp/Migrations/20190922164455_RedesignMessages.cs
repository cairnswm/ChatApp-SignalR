using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatApp.Migrations
{
    public partial class RedesignMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDeviceMessages");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "UserDevice",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceID",
                table: "UserDevice",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeen",
                table: "UserDevice",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "UserDevice",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ChatID = table.Column<int>(nullable: false),
                    FromUserName = table.Column<string>(nullable: true),
                    Text = table.Column<string>(nullable: true),
                    Sent = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDevice_DeviceID",
                table: "UserDevice",
                column: "DeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevice_UserID",
                table: "UserDevice",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevice_UserName",
                table: "UserDevice",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatID_Sent",
                table: "Messages",
                columns: new[] { "ChatID", "Sent" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_UserDevice_DeviceID",
                table: "UserDevice");

            migrationBuilder.DropIndex(
                name: "IX_UserDevice_UserID",
                table: "UserDevice");

            migrationBuilder.DropIndex(
                name: "IX_UserDevice_UserName",
                table: "UserDevice");

            migrationBuilder.DropColumn(
                name: "LastSeen",
                table: "UserDevice");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "UserDevice");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "UserDevice",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceID",
                table: "UserDevice",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "UserDeviceMessages",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DeviceID = table.Column<int>(nullable: false),
                    FromUserName = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDeviceMessages", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDeviceMessages_UserName_DeviceID",
                table: "UserDeviceMessages",
                columns: new[] { "UserName", "DeviceID" });
        }
    }
}
