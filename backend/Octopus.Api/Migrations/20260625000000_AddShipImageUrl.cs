using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Octopus.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddShipImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Ships",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Ships");
        }
    }
}
