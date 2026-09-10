using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sweet_Silvia_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantShippingDimensionsAndCouponGuard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingHeightCm",
                table: "ProductVariants",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 4m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingLengthCm",
                table: "ProductVariants",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 30m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingWeightKg",
                table: "ProductVariants",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0.3m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingWidthCm",
                table: "ProductVariants",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 22m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingHeightCm",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ShippingLengthCm",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ShippingWeightKg",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ShippingWidthCm",
                table: "ProductVariants");
        }
    }
}
