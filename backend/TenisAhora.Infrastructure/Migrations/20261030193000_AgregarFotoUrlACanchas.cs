using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenisAhora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFotoUrlACanchas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "Canchas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "Canchas");
        }
    }
}
