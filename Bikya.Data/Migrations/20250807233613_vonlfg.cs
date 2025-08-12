using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bikya.Data.Migrations
{
    /// <inheritdoc />
    public partial class vonlfg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "ShippingInfos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShippingMethod",
                table: "ShippingInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSwapOrder",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ExchangeRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "ExchangeRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "ExchangeRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderForOfferedProductId",
                table: "ExchangeRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderForRequestedProductId",
                table: "ExchangeRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAt",
                table: "ExchangeRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusMessage",
                table: "ExchangeRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExchangeStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExchangeRequestId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeStatusHistories_ExchangeRequests_ExchangeRequestId",
                        column: x => x.ExchangeRequestId,
                        principalTable: "ExchangeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRequests_OrderForOfferedProductId",
                table: "ExchangeRequests",
                column: "OrderForOfferedProductId",
                unique: true,
                filter: "[OrderForOfferedProductId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRequests_OrderForRequestedProductId",
                table: "ExchangeRequests",
                column: "OrderForRequestedProductId",
                unique: true,
                filter: "[OrderForRequestedProductId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeStatusHistories_ExchangeRequestId",
                table: "ExchangeStatusHistories",
                column: "ExchangeRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRequests_Orders_OrderForOfferedProductId",
                table: "ExchangeRequests",
                column: "OrderForOfferedProductId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRequests_Orders_OrderForRequestedProductId",
                table: "ExchangeRequests",
                column: "OrderForRequestedProductId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRequests_Orders_OrderForOfferedProductId",
                table: "ExchangeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRequests_Orders_OrderForRequestedProductId",
                table: "ExchangeRequests");

            migrationBuilder.DropTable(
                name: "ExchangeStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeRequests_OrderForOfferedProductId",
                table: "ExchangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeRequests_OrderForRequestedProductId",
                table: "ExchangeRequests");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "ShippingInfos");

            migrationBuilder.DropColumn(
                name: "ShippingMethod",
                table: "ShippingInfos");

            migrationBuilder.DropColumn(
                name: "IsSwapOrder",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "ExchangeRequests");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "ExchangeRequests");

            migrationBuilder.DropColumn(
                name: "OrderForOfferedProductId",
                table: "ExchangeRequests");

            migrationBuilder.DropColumn(
                name: "OrderForRequestedProductId",
                table: "ExchangeRequests");

            migrationBuilder.DropColumn(
                name: "RespondedAt",
                table: "ExchangeRequests");

            migrationBuilder.DropColumn(
                name: "StatusMessage",
                table: "ExchangeRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "ExchangeRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
