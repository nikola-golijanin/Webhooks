using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Webhooks.Api.Migrations
{
    /// <inheritdoc />
    public partial class Add_Roles_and_Permissions_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_role_users_roles_RoleId",
                table: "role_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_role_users",
                table: "role_users");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "role_users");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "role_users",
                newName: "RolesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_role_users",
                table: "role_users",
                columns: new[] { "RolesId", "UsersId" });

            migrationBuilder.AddForeignKey(
                name: "FK_role_users_roles_RolesId",
                table: "role_users",
                column: "RolesId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_role_users_roles_RolesId",
                table: "role_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_role_users",
                table: "role_users");

            migrationBuilder.RenameColumn(
                name: "RolesId",
                table: "role_users",
                newName: "UserId");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "role_users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_role_users",
                table: "role_users",
                columns: new[] { "RoleId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_role_users_roles_RoleId",
                table: "role_users",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
