using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Sweet_Silvia_Infrastructure;

#nullable disable

namespace Sweet_Silvia_Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261002100000_AddUserAvatar")]
public partial class AddUserAvatar : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AvatarId",
            table: "Users",
            type: "character varying(16)",
            maxLength: 16,
            nullable: false,
            defaultValue: "avatar_01");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AvatarId",
            table: "Users");
    }
}
