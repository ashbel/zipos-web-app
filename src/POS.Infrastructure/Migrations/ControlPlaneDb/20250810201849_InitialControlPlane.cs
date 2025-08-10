using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations.ControlPlaneDb
{
    /// <inheritdoc />
    public partial class InitialControlPlane : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant_connection_strings",
                columns: table => new
                {
                    OrganizationId = table.Column<string>(type: "text", nullable: false),
                    ConnectionString = table.Column<string>(type: "text", nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Npgsql"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_connection_strings", x => x.OrganizationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_connection_strings_OrganizationId",
                table: "tenant_connection_strings",
                column: "OrganizationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_connection_strings");
        }
    }
}
