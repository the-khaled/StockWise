using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockWise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removeproductWarehouseId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Products",
                type: "int",
                nullable: true);
        }
    }
}
