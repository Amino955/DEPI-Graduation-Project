using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TellaStore.Migrations
{
    /// <inheritdoc />
    public partial class AddUsageCountToDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsageCount",
                table: "Discounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsageCount",
                table: "Discounts");
        }
    }
}
