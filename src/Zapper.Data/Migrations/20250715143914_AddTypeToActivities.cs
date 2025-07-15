using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeToActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Activities");
        }
    }
}
