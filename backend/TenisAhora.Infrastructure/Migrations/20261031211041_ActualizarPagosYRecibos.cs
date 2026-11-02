using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenisAhora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarPagosYRecibos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recibos_Pagos_PagoId",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "importe",
                table: "Recibos");

            migrationBuilder.AddColumn<bool>(
                name: "Anulado",
                table: "Recibos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "MercadoPagoPaymentId",
                table: "Pagos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Recibos_Pagos_PagoId",
                table: "Recibos",
                column: "PagoId",
                principalTable: "Pagos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recibos_Pagos_PagoId",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "Anulado",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "MercadoPagoPaymentId",
                table: "Pagos");

            migrationBuilder.AddColumn<float>(
                name: "importe",
                table: "Recibos",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddForeignKey(
                name: "FK_Recibos_Pagos_PagoId",
                table: "Recibos",
                column: "PagoId",
                principalTable: "Pagos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
