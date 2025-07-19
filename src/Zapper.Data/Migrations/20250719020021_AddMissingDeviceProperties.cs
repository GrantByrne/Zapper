using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingDeviceProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceIdentifier",
                table: "Devices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaired",
                table: "Devices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PairingId",
                table: "Devices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PairingKey",
                table: "Devices",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProtocolVersion",
                table: "Devices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPairing",
                table: "Devices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Devices",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceIdentifier",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "IsPaired",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "PairingId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "PairingKey",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "ProtocolVersion",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RequiresPairing",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Devices");
        }
    }
}
