
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly AppDbContext _context;

        public ReporteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]?> GenerarAsync(
            string tipoReporte,
            int administradorId)
        {
            var administradorExiste = await _context.Administradores
                .AnyAsync(a => a.Id == administradorId);

            if (!administradorExiste ||
                string.IsNullOrWhiteSpace(tipoReporte))
            {
                return null;
            }

            var fechaGeneracion = DateTime.Now;
            string titulo;
            string contenido;
            byte[] pdf;

            QuestPDF.Settings.License = LicenseType.Community;

            if (tipoReporte.Equals(
                "Canchas",
                StringComparison.OrdinalIgnoreCase))
            {
                var canchas = await _context.Canchas
                    .Include(c => c.TipoDeCancha)
                    .OrderBy(c => c.Id)
                    .ToListAsync();

                titulo = "Reporte de Canchas";

                contenido = string.Join(
                    Environment.NewLine,
                    canchas.Select(c =>
                        $"Cancha {c.Id} - Superficie: " +
                        $"{c.TipoDeCancha.superficie} - " +
                        $"Capacidad: {c.TipoDeCancha.capacidad}"));

                if (canchas.Count == 0)
                    contenido = "No hay canchas registradas.";

                pdf = Document.Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Margin(40);

                        page.Header()
                            .Text(titulo)
                            .FontSize(20)
                            .Bold();

                        page.Content().Column(column =>
                        {
                            column.Spacing(10);

                            column.Item().Text(
                                $"Fecha de generación: " +
                                $"{fechaGeneracion:dd/MM/yyyy HH:mm}");

                            column.Item().Text(
                                $"Cantidad de canchas: {canchas.Count}");

                            column.Item().Text("Detalle de canchas:").Bold();

                            foreach (var cancha in canchas)
                            {
                                column.Item().Text(
                                    $"Cancha {cancha.Id} | " +
                                    $"Superficie: {cancha.TipoDeCancha.superficie} | " +
                                    $"Capacidad: {cancha.TipoDeCancha.capacidad}");
                            }

                            if (canchas.Count == 0)
                                column.Item().Text(
                                    "No hay canchas registradas.");
                        });

                        page.Footer().AlignCenter().Text("TenisAhora");
                    });
                }).GeneratePdf();
            }
            else if (tipoReporte.Equals(
                "Ingresos",
                StringComparison.OrdinalIgnoreCase))
            {
                var pagos = await _context.Pagos
                    .OrderBy(p => p.Fecha_pago)
                    .ThenBy(p => p.Id)
                    .ToListAsync();

                var pagosValidados = pagos
                    .Where(p => p.estado == EstadoPago.Validado)
                    .ToList();

                float totalIngresos = pagosValidados.Sum(p => p.importe);

                titulo = "Reporte de Ingresos";

                contenido = string.Join(
                    Environment.NewLine,
                    pagos.Select(p =>
                        $"Pago {p.Id} - " +
                        $"Fecha: {p.Fecha_pago:dd/MM/yyyy HH:mm} - " +
                        $"Importe: {p.importe:C2} - " +
                        $"Medio: {p.tipo_pago} - " +
                        $"Estado: {p.estado}"));

                if (pagos.Count == 0)
                    contenido = "No hay pagos registrados.";

                pdf = Document.Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Margin(40);

                        page.Header()
                            .Text(titulo)
                            .FontSize(20)
                            .Bold();

                        page.Content().Column(column =>
                        {
                            column.Spacing(10);

                            column.Item().Text(
                                $"Fecha de generación: " +
                                $"{fechaGeneracion:dd/MM/yyyy HH:mm}");

                            column.Item().Text($"Cantidad de pagos: {pagos.Count}");
                            column.Item().Text($"Pagos validados: {pagosValidados.Count}");

                            column.Item()
                                .Text($"Total de ingresos validados: {totalIngresos:C2}")
                                .Bold();

                            column.Item().Text("Detalle de pagos:").Bold();

                            foreach (var pago in pagos)
                            {
                                column.Item().Text(
                                    $"Pago {pago.Id} | " +
                                    $"Fecha: {pago.Fecha_pago:dd/MM/yyyy HH:mm} | " +
                                    $"Importe: {pago.importe:C2} | " +
                                    $"Medio: {pago.tipo_pago} | " +
                                    $"Estado: {pago.estado}");
                            }

                            if (pagos.Count == 0)
                                column.Item().Text("No hay pagos registrados.");
                        });

                        page.Footer().AlignCenter().Text("TenisAhora");
                    });
                }).GeneratePdf();
            }



            else if (tipoReporte.Equals(
                "Reservas",
                StringComparison.OrdinalIgnoreCase))
            {
                var reservas = await _context.Reservas
                    .Include(r => r.Cancha)
                        .ThenInclude(c => c.TipoDeCancha)
                    .Include(r => r.Socios)
                    .OrderBy(r => r.fecha_reserva)
                    .ThenBy(r => r.HoraInicio)
                    .ThenBy(r => r.Id)
                    .ToListAsync();

                titulo = "Reporte de Reservas";

                contenido = string.Join(
                    Environment.NewLine,
                    reservas.Select(r =>
                        $"Reserva {r.Id} - " +
                        $"Fecha: {r.fecha_reserva:dd/MM/yyyy} - " +
                        $"Horario: {r.HoraInicio:HH:mm} a {r.HoraFin:HH:mm} - " +
                        $"Estado: {r.estado} - " +
                        $"Importe: {r.importe:C2} - " +
                        $"Cancha: {r.CanchaId}" +
                        Environment.NewLine +
                        "Socios involucrados: " +
                        (r.Socios.Count > 0
                            ? string.Join(
                                " | ",
                                r.Socios.Select(s =>
                                    $"{s.Nombre} {s.Apellido}, " +
                                    $"DNI: {s.Dni}, " +
                                    $"Teléfono: {s.Telefono}, " +
                                    $"Email: {s.Email}, " +
                                    $"Membresía: {s.estadoMembresia}"))
                            : "Sin socios asociados")));

                if (reservas.Count == 0)
                    contenido = "No hay reservas registradas.";

                pdf = Document.Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Margin(40);

                        page.Header()
                            .Text(titulo)
                            .FontSize(20)
                            .Bold();

                        page.Content().Column(column =>
                        {
                            column.Spacing(10);

                            column.Item().Text(
                                $"Fecha de generación: " +
                                $"{fechaGeneracion:dd/MM/yyyy HH:mm}");

                            column.Item().Text(
                                $"Cantidad de reservas: {reservas.Count}");

                            column.Item().Text("Detalle de reservas:").Bold();

                            foreach (var reserva in reservas)
                            {
                                column.Item()
                                    .Text($"Reserva {reserva.Id}")
                                    .FontSize(14)
                                    .Bold();

                                column.Item().Text(
                                    $"Fecha: {reserva.fecha_reserva:dd/MM/yyyy}");

                                column.Item().Text(
                                    $"Horario: {reserva.HoraInicio:HH:mm} a " +
                                    $"{reserva.HoraFin:HH:mm}");

                                column.Item().Text($"Estado: {reserva.estado}");

                                column.Item().Text(
                                    $"Importe: {reserva.importe:C2}");

                                column.Item().Text(
                                    $"Cancha: {reserva.CanchaId}");

                                column.Item()
                                    .Text("Socios involucrados:")
                                    .Bold();

                                if (reserva.Socios.Count > 0)
                                {
                                    foreach (var socio in reserva.Socios)
                                    {
                                        column.Item().Text(
                                            $"Nombre completo: " +
                                            $"{socio.Nombre} {socio.Apellido}");

                                        column.Item().Text(
                                            $"DNI: {socio.Dni}");

                                        column.Item().Text(
                                            $"Teléfono: {socio.Telefono}");

                                        column.Item().Text(
                                            $"Email: {socio.Email}");

                                        column.Item().Text(
                                            $"Estado de membresía: " +
                                            $"{socio.estadoMembresia}");

                                        column.Item().PaddingBottom(6);
                                    }
                                }
                                else
                                {
                                    column.Item().Text(
                                        "No hay socios asociados a esta reserva.");
                                }

                                column.Item()
                                    .PaddingVertical(5)
                                    .LineHorizontal(1);
                            }

                            if (reservas.Count == 0)
                            {
                                column.Item().Text(
                                    "No hay reservas registradas.");
                            }
                        });

                        page.Footer()
                            .AlignCenter()
                            .Text("TenisAhora");
                    });
                }).GeneratePdf();
            }








            else
            {
                return null;
            }

            var reporte = new Reporte
            {
                FechaGeneracion = fechaGeneracion,
                tipo_reporte = titulo.Replace("Reporte de ", ""),
                contenido_detalle = contenido,
                AdministradorId = administradorId
            };

            _context.Reportes.Add(reporte);
            await _context.SaveChangesAsync();

            return pdf;
        }
    }
}
