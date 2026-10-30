using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenisAhora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEntrenamientosAActividades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Entrenamiento_Nombre",
                table: "Actividades",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "objetivoEntrenamiento",
                table: "Actividades",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Entrenamiento_Nombre",
                table: "Actividades");

            migrationBuilder.DropColumn(
                name: "objetivoEntrenamiento",
                table: "Actividades");
        }
    }
}
