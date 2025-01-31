using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Webhooks.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenamedOneColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReponseStatusCode",
                schema: "webhooks",
                table: "delivery_attempts",
                newName: "ResponseStatusCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResponseStatusCode",
                schema: "webhooks",
                table: "delivery_attempts",
                newName: "ReponseStatusCode");
        }
    }
}
