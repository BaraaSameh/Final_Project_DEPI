using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepiFinalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddIsCancelledToReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Returns",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Returns");
        }
    }
}
