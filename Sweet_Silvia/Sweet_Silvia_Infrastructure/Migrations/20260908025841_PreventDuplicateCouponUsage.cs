using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Sweet_Silvia_Infrastructure;

#nullable disable

namespace Sweet_Silvia_Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260908025841_PreventDuplicateCouponUsage")]
public partial class PreventDuplicateCouponUsage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_CouponUsages_CouponId_OrderId",
            table: "CouponUsages",
            columns: new[] { "CouponId", "OrderId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_CouponUsages_CouponId_OrderId",
            table: "CouponUsages");
    }
}
