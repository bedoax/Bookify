using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIndexIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_tags_name",
                table: "tags");

            migrationBuilder.CreateIndex(
                name: "UQ_tags_name",
                table: "tags",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_tags_name",
                table: "tags");

            migrationBuilder.CreateIndex(
                name: "UQ_tags_name",
                table: "tags",
                column: "Name",
                unique: true);
        }
    }
}
