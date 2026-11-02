using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenisAhora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarClasesAActividades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Actividades",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Actividades",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoDeClase",
                table: "Actividades",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "TipoDeClase",
                table: "Actividades");
        }
    }
}
