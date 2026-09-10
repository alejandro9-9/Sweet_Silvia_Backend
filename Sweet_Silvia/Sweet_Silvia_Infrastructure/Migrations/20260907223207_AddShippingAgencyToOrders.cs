using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sweet_Silvia_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingAgencyToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ShippingAgencyId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingAgencyId",
                table: "Orders",
                column: "ShippingAgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OlvaAgencies_ShippingAgencyId",
                table: "Orders",
                column: "ShippingAgencyId",
                principalTable: "OlvaAgencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OlvaAgencies_ShippingAgencyId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingAgencyId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAgencyId",
                table: "Orders");
        }
    }
}
