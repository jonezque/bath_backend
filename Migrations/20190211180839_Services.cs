using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication2.Migrations
{
    public partial class Services : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BathPlacePositions_BathPlaces_BathPlaceId",
                table: "BathPlacePositions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPositions_Products_ProductId",
                table: "ProductPositions");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductPositions",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BathPlaceId",
                table: "BathPlacePositions",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Masters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastName = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Masters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicePositions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MasterId = table.Column<int>(nullable: false),
                    OrderId = table.Column<int>(nullable: false),
                    ServiceId = table.Column<int>(nullable: false),
                    ServiceCost = table.Column<decimal>(nullable: false),
                    AddonsCost = table.Column<decimal>(nullable: false),
                    TotalCost = table.Column<decimal>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePositions_Masters_MasterId",
                        column: x => x.MasterId,
                        principalTable: "Masters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicePositions_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicePositions_MasterId",
                table: "ServicePositions",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePositions_ServiceId",
                table: "ServicePositions",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_BathPlacePositions_BathPlaces_BathPlaceId",
                table: "BathPlacePositions",
                column: "BathPlaceId",
                principalTable: "BathPlaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPositions_Products_ProductId",
                table: "ProductPositions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BathPlacePositions_BathPlaces_BathPlaceId",
                table: "BathPlacePositions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPositions_Products_ProductId",
                table: "ProductPositions");

            migrationBuilder.DropTable(
                name: "ServicePositions");

            migrationBuilder.DropTable(
                name: "Masters");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductPositions",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "BathPlaceId",
                table: "BathPlacePositions",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_BathPlacePositions_BathPlaces_BathPlaceId",
                table: "BathPlacePositions",
                column: "BathPlaceId",
                principalTable: "BathPlaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPositions_Products_ProductId",
                table: "ProductPositions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
