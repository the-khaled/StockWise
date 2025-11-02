using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockWise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addimg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Imag",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Imag",
                table: "Products");
        }
    }
}
