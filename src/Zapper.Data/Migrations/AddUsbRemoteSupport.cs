using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsbRemoteSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsbRemotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    VendorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Manufacturer = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    InterceptSystemButtons = table.Column<bool>(type: "INTEGER", nullable: false),
                    LongPressTimeoutMs = table.Column<int>(type: "INTEGER", nullable: false),
                    RepeatDelayMs = table.Column<int>(type: "INTEGER", nullable: false),
                    RepeatRateMs = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsbRemotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsbRemoteButtons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RemoteId = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyCode = table.Column<byte>(type: "INTEGER", nullable: false),
                    ButtonName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsSystemButton = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowInterception = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsbRemoteButtons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsbRemoteButtons_UsbRemotes_RemoteId",
                        column: x => x.RemoteId,
                        principalTable: "UsbRemotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsbRemoteButtonMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ButtonId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceCommandId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsbRemoteButtonMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsbRemoteButtonMappings_DeviceCommands_DeviceCommandId",
                        column: x => x.DeviceCommandId,
                        principalTable: "DeviceCommands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsbRemoteButtonMappings_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsbRemoteButtonMappings_UsbRemoteButtons_ButtonId",
                        column: x => x.ButtonId,
                        principalTable: "UsbRemoteButtons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsbRemoteButtonMappings_ButtonId_DeviceId_EventType",
                table: "UsbRemoteButtonMappings",
                columns: ["ButtonId", "DeviceId", "EventType"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsbRemoteButtonMappings_DeviceCommandId",
                table: "UsbRemoteButtonMappings",
                column: "DeviceCommandId");

            migrationBuilder.CreateIndex(
                name: "IX_UsbRemoteButtonMappings_DeviceId",
                table: "UsbRemoteButtonMappings",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_UsbRemoteButtons_RemoteId_KeyCode",
                table: "UsbRemoteButtons",
                columns: ["RemoteId", "KeyCode"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsbRemotes_DeviceId",
                table: "UsbRemotes",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsbRemotes_VendorId_ProductId_SerialNumber",
                table: "UsbRemotes",
                columns: ["VendorId", "ProductId", "SerialNumber"],
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsbRemoteButtonMappings");

            migrationBuilder.DropTable(
                name: "UsbRemoteButtons");

            migrationBuilder.DropTable(
                name: "UsbRemotes");
        }
    }
}
