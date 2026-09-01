using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sweet_Silvia_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOlvaAgencyCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OlvaAgencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Department = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Province = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    District = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Phone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ScheduleJson = table.Column<string>(type: "jsonb", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OlvaAgencies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OlvaAgencies_Code",
                table: "OlvaAgencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OlvaAgencies_Department_Province_District_IsActive",
                table: "OlvaAgencies",
                columns: new[] { "Department", "Province", "District", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OlvaAgencies");
        }
    }
}
