using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bikya.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRequestUserAndProcessedAndShippingFeePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "ShippingInfos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(12)",
                oldMaxLength: 12);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "ExchangeRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcessedBy",
                table: "ExchangeRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ExchangeRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRequests_UserId",
                table: "ExchangeRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRequests_AspNetUsers_UserId",
                table: "ExchangeRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRequests_AspNetUsers_UserId",
                table: "ExchangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeRequests_UserId",
                table: "ExchangeRequests");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "ExchangeRequests");

            migrationBuilder.DropColumn(
                name: "ProcessedBy",
                table: "ExchangeRequests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExchangeRequests");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "ShippingInfos",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
