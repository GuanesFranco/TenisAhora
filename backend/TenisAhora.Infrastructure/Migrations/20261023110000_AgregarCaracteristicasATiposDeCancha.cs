using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenisAhora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCaracteristicasATiposDeCancha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "caracteristicas",
                table: "TiposDeCancha",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "caracteristicas",
                table: "TiposDeCancha");
        }
    }
}

