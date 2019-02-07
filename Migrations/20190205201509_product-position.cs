using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication2.Migrations
{
    public partial class productposition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPositions_Orders_OrderId",
                table: "ProductPositions");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "ProductPositions",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductPrice",
                table: "ProductPositions",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "ProductPositions",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPositions_Orders_OrderId",
                table: "ProductPositions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPositions_Orders_OrderId",
                table: "ProductPositions");

            migrationBuilder.DropColumn(
                name: "ProductPrice",
                table: "ProductPositions");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "ProductPositions");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "ProductPositions",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPositions_Orders_OrderId",
                table: "ProductPositions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
