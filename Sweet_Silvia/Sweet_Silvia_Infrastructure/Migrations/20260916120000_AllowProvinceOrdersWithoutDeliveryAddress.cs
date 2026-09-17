using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Sweet_Silvia_Infrastructure;

#nullable disable

namespace Sweet_Silvia_Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260916120000_AllowProvinceOrdersWithoutDeliveryAddress")]
public partial class AllowProvinceOrdersWithoutDeliveryAddress : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "AddressId",
            table: "Orders",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE \"Orders\" SET \"AddressId\" = (SELECT \"Id\" FROM \"Addresses\" WHERE \"Addresses\".\"UserId\" = \"Orders\".\"UserId\" AND \"Addresses\".\"IsActive\" = TRUE ORDER BY \"Addresses\".\"IsDefault\" DESC, \"Addresses\".\"CreatedAt\" LIMIT 1) WHERE \"AddressId\" IS NULL;");
        migrationBuilder.AlterColumn<Guid>(
            name: "AddressId",
            table: "Orders",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);
    }
}
