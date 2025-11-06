using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Webhooks.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_EventType",
                schema: "webhooks",
                table: "subscriptions",
                column: "EventType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_subscriptions_EventType",
                schema: "webhooks",
                table: "subscriptions");
        }
    }
}
