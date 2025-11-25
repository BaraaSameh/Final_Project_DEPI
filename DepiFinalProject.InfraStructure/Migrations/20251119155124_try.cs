using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepiFinalProject.Migrations
{
    /// <inheritdoc />
    public partial class @try : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlashSaleProducts_Users_userid",
                table: "FlashSaleProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_FlashSales_Users_userid",
                table: "FlashSales");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_UserID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_FlashSaleProducts_userid",
                table: "FlashSaleProducts");

            migrationBuilder.DropColumn(
                name: "userid",
                table: "FlashSaleProducts");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Products",
                newName: "userid");

            migrationBuilder.RenameIndex(
                name: "IX_Products_UserID",
                table: "Products",
                newName: "IX_Products_userid");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "FlashSales",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_FlashSales_userid",
                table: "FlashSales",
                newName: "IX_FlashSales_UserID");

            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "userid",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconPublicId",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductImage",
                columns: table => new
                {
                    ImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImage", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_ProductImage_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImage_ProductId",
                table: "ProductImage",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSales_Users_UserID",
                table: "FlashSales",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_userid",
                table: "Products",
                column: "userid",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlashSales_Users_UserID",
                table: "FlashSales");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_userid",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductImage");

            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IconPublicId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "Products",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Products_userid",
                table: "Products",
                newName: "IX_Products_UserID");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "FlashSales",
                newName: "userid");

            migrationBuilder.RenameIndex(
                name: "IX_FlashSales_UserID",
                table: "FlashSales",
                newName: "IX_FlashSales_userid");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "userid",
                table: "FlashSaleProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FlashSaleProducts_userid",
                table: "FlashSaleProducts",
                column: "userid");

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSaleProducts_Users_userid",
                table: "FlashSaleProducts",
                column: "userid",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlashSales_Users_userid",
                table: "FlashSales",
                column: "userid",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_UserID",
                table: "Products",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }
    }
}
