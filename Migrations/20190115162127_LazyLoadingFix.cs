using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication2.Migrations
{
    public partial class LazyLoadingFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BathPlacePositions_Orders_OrderId",
                table: "BathPlacePositions");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "BathPlacePositions",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_BathPlacePositions_Orders_OrderId",
                table: "BathPlacePositions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BathPlacePositions_Orders_OrderId",
                table: "BathPlacePositions");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "BathPlacePositions",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BathPlacePositions_Orders_OrderId",
                table: "BathPlacePositions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
