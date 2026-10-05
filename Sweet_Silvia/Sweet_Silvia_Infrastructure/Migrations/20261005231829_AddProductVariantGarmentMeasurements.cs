using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sweet_Silvia_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariantGarmentMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BackLengthCm",
                table: "ProductVariants",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ChestCircumferenceCm",
                table: "ProductVariants",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FrontLengthCm",
                table: "ProductVariants",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HipCircumferenceCm",
                table: "ProductVariants",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SleeveLengthCm",
                table: "ProductVariants",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaistCircumferenceCm",
                table: "ProductVariants",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackLengthCm",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ChestCircumferenceCm",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "FrontLengthCm",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "HipCircumferenceCm",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "SleeveLengthCm",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "WaistCircumferenceCm",
                table: "ProductVariants");
        }
    }
}
