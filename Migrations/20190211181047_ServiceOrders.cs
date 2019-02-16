using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication2.Migrations
{
    public partial class ServiceOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ServicePositions_OrderId",
                table: "ServicePositions",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePositions_Orders_OrderId",
                table: "ServicePositions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicePositions_Orders_OrderId",
                table: "ServicePositions");

            migrationBuilder.DropIndex(
                name: "IX_ServicePositions_OrderId",
                table: "ServicePositions");
        }
    }
}
