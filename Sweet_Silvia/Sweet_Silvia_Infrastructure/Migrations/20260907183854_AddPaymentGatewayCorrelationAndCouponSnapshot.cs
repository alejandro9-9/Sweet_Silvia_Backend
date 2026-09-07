using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sweet_Silvia_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentGatewayCorrelationAndCouponSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GatewayTransactionId",
                table: "Payments",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CouponAppliedQuantity",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GatewayTransactionId",
                table: "Payments",
                column: "GatewayTransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_GatewayTransactionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "GatewayTransactionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CouponAppliedQuantity",
                table: "Orders");
        }
    }
}
