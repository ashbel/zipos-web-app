using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SchemaBasedMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Organizations_OrganizationId",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Organizations_OrganizationId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_OrganizationId_Name",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Organizations",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Branches_OrganizationId_Code",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "UserBranches");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Branches");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "UserRoles",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "UserBranches",
                newName: "UserBranches",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Roles",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                newName: "RolePermissions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "permissions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Organizations",
                newName: "organizations",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Branches",
                newName: "Branches",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_Name",
                schema: "public",
                table: "permissions",
                newName: "IX_permissions_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_Module",
                schema: "public",
                table: "permissions",
                newName: "IX_permissions_Module");

            migrationBuilder.RenameIndex(
                name: "IX_Organizations_Name",
                schema: "public",
                table: "organizations",
                newName: "IX_organizations_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Organizations_ContactEmail",
                schema: "public",
                table: "organizations",
                newName: "IX_organizations_ContactEmail");

            migrationBuilder.AddPrimaryKey(
                name: "PK_permissions",
                schema: "public",
                table: "permissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_organizations",
                schema: "public",
                table: "organizations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "public",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                schema: "public",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code",
                schema: "public",
                table: "Branches",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_permissions_PermissionId",
                schema: "public",
                table: "RolePermissions",
                column: "PermissionId",
                principalSchema: "public",
                principalTable: "permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_permissions_PermissionId",
                schema: "public",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                schema: "public",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Name",
                schema: "public",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_permissions",
                schema: "public",
                table: "permissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_organizations",
                schema: "public",
                table: "organizations");

            migrationBuilder.DropIndex(
                name: "IX_Branches_Code",
                schema: "public",
                table: "Branches");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "public",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "public",
                newName: "UserRoles");

            migrationBuilder.RenameTable(
                name: "UserBranches",
                schema: "public",
                newName: "UserBranches");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "public",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                schema: "public",
                newName: "RolePermissions");

            migrationBuilder.RenameTable(
                name: "permissions",
                schema: "public",
                newName: "Permissions");

            migrationBuilder.RenameTable(
                name: "organizations",
                schema: "public",
                newName: "Organizations");

            migrationBuilder.RenameTable(
                name: "Branches",
                schema: "public",
                newName: "Branches");

            migrationBuilder.RenameIndex(
                name: "IX_permissions_Name",
                table: "Permissions",
                newName: "IX_Permissions_Name");

            migrationBuilder.RenameIndex(
                name: "IX_permissions_Module",
                table: "Permissions",
                newName: "IX_Permissions_Module");

            migrationBuilder.RenameIndex(
                name: "IX_organizations_Name",
                table: "Organizations",
                newName: "IX_Organizations_Name");

            migrationBuilder.RenameIndex(
                name: "IX_organizations_ContactEmail",
                table: "Organizations",
                newName: "IX_Organizations_ContactEmail");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "UserRoles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "UserBranches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "Roles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "RolePermissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "Branches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permissions",
                table: "Permissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Organizations",
                table: "Organizations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId_Email",
                table: "Users",
                columns: new[] { "OrganizationId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_OrganizationId_Name",
                table: "Roles",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_OrganizationId_Code",
                table: "Branches",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Organizations_OrganizationId",
                table: "Branches",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Organizations_OrganizationId",
                table: "Roles",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
