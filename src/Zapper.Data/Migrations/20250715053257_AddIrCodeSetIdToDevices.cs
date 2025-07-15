using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIrCodeSetIdToDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    ConnectionType = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    MacAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Port = table.Column<int>(type: "INTEGER", nullable: true),
                    AuthToken = table.Column<string>(type: "TEXT", nullable: true),
                    NetworkAddress = table.Column<string>(type: "TEXT", nullable: true),
                    AuthenticationToken = table.Column<string>(type: "TEXT", nullable: true),
                    UseSecureConnection = table.Column<bool>(type: "INTEGER", nullable: false),
                    BluetoothAddress = table.Column<string>(type: "TEXT", nullable: true),
                    SupportsMouseInput = table.Column<bool>(type: "INTEGER", nullable: false),
                    SupportsKeyboardInput = table.Column<bool>(type: "INTEGER", nullable: false),
                    IrCodeSet = table.Column<string>(type: "TEXT", nullable: true),
                    IrCodeSetId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsOnline = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalIrCodeCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CacheKey = table.Column<string>(type: "TEXT", nullable: false),
                    CachedData = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalIrCodeCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IrCodeSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Brand = table.Column<string>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceType = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    DownloadCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrCodeSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrimaryDevice = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityDevices_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityDevices_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    IrCode = table.Column<string>(type: "TEXT", nullable: true),
                    NetworkPayload = table.Column<string>(type: "TEXT", nullable: true),
                    HttpMethod = table.Column<string>(type: "TEXT", nullable: true),
                    HttpEndpoint = table.Column<string>(type: "TEXT", nullable: true),
                    DelayMs = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRepeatable = table.Column<bool>(type: "INTEGER", nullable: false),
                    MouseDeltaX = table.Column<int>(type: "INTEGER", nullable: true),
                    MouseDeltaY = table.Column<int>(type: "INTEGER", nullable: true),
                    KeyboardText = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceCommands_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IrCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Brand = table.Column<string>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceType = table.Column<string>(type: "TEXT", nullable: false),
                    CommandName = table.Column<string>(type: "TEXT", nullable: false),
                    Protocol = table.Column<string>(type: "TEXT", nullable: false),
                    HexCode = table.Column<string>(type: "TEXT", nullable: false),
                    Frequency = table.Column<int>(type: "INTEGER", nullable: false),
                    RawData = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IRCodeSetId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IrCodes_IrCodeSets_IRCodeSetId",
                        column: x => x.IRCodeSetId,
                        principalTable: "IrCodeSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivitySteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceCommandId = table.Column<int>(type: "INTEGER", nullable: false),
                    StepOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    DelayBeforeMs = table.Column<int>(type: "INTEGER", nullable: false),
                    DelayAfterMs = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitySteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivitySteps_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivitySteps_DeviceCommands_DeviceCommandId",
                        column: x => x.DeviceCommandId,
                        principalTable: "DeviceCommands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Name",
                table: "Activities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SortOrder",
                table: "Activities",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityDevices_ActivityId_DeviceId",
                table: "ActivityDevices",
                columns: new[] { "ActivityId", "DeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityDevices_DeviceId",
                table: "ActivityDevices",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySteps_ActivityId_StepOrder",
                table: "ActivitySteps",
                columns: new[] { "ActivityId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySteps_DeviceCommandId",
                table: "ActivitySteps",
                column: "DeviceCommandId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_DeviceId_Name",
                table: "DeviceCommands",
                columns: new[] { "DeviceId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_IpAddress_Port",
                table: "Devices",
                columns: new[] { "IpAddress", "Port" });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Name",
                table: "Devices",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalIrCodeCache_CacheKey",
                table: "ExternalIrCodeCache",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalIrCodeCache_ExpiresAt",
                table: "ExternalIrCodeCache",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_IrCodes_Brand_Model_CommandName",
                table: "IrCodes",
                columns: new[] { "Brand", "Model", "CommandName" });

            migrationBuilder.CreateIndex(
                name: "IX_IrCodes_IRCodeSetId",
                table: "IrCodes",
                column: "IRCodeSetId");

            migrationBuilder.CreateIndex(
                name: "IX_IrCodeSets_Brand_Model",
                table: "IrCodeSets",
                columns: new[] { "Brand", "Model" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityDevices");

            migrationBuilder.DropTable(
                name: "ActivitySteps");

            migrationBuilder.DropTable(
                name: "ExternalIrCodeCache");

            migrationBuilder.DropTable(
                name: "IrCodes");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "DeviceCommands");

            migrationBuilder.DropTable(
                name: "IrCodeSets");

            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
