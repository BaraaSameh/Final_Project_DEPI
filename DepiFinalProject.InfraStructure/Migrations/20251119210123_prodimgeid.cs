using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepiFinalProject.Migrations
{
    /// <inheritdoc />
    public partial class prodimgeid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imagepublicid",
                table: "ProductImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imagepublicid",
                table: "ProductImages");
        }
    }
}
