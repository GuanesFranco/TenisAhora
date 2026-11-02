using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Recibos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Recibos.Queries;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Recibos.Handlers;

/// <summary>
/// Caso de Uso: Imprimir Comprobante de Recibo [RF-72]
/// </summary>
public class ImprimirReciboHandler
{
    private readonly IReciboRepository _reciboRepository;

    public ImprimirReciboHandler(IReciboRepository reciboRepository)
    {
        _reciboRepository = reciboRepository;
    }

    public async Task<ImpresionReciboDto> Handle(ImprimirReciboQuery query, CancellationToken ct = default)
    {
        var recibo = await _reciboRepository.ObtenerPorIdAsync(query.Id, ct);
        if (recibo is null)
        {
            throw new DomainException($"No se encontró el recibo con ID {query.Id}.");
        }

        var socio = recibo.Pago.Reserva?.Socios?.FirstOrDefault();
        string? nombreSocio = socio != null ? $"{socio.Nombre} {socio.Apellido}".Trim() : null;

        string concepto;
        if (recibo.Pago.ReservaId.HasValue)
        {
            concepto = "Reserva / Alquiler de Cancha";
        }
        else if (recibo.Pago.InscripcionId.HasValue)
        {
            concepto = "Inscripción a Actividad / Competencia";
        }
        else
        {
            concepto = "Cobro General";
        }

        return new ImpresionReciboDto(
            recibo.Id,
            recibo.PagoId,
            "Complejo El Deporte Saludable - Sistema Tenis Ahora",
            recibo.FechaEmision,
            socio?.Dni,
            nombreSocio,
            concepto,
            recibo.Pago.importe,
            recibo.Pago.porcentaje_pago,
            recibo.Pago.tipo_pago.ToString(),
            recibo.Anulado ? "Anulado" : "Válido"
        );
    }
}
