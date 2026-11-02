using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Pagos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Pagos.Queries;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Pagos.Handlers;

/// <summary>
/// Caso de Uso: Imprimir Comprobante de Cobro [RF-68]
/// </summary>
public class ImprimirCobroHandler
{
    private readonly IPagoRepository _pagoRepository;

    public ImprimirCobroHandler(IPagoRepository pagoRepository)
    {
        _pagoRepository = pagoRepository;
    }

    public async Task<ImpresionCobroDto> Handle(ImprimirCobroQuery query, CancellationToken ct = default)
    {
        var pago = await _pagoRepository.ObtenerPorIdAsync(query.NumeroCobro, ct);
        if (pago is null)
        {
            throw new DomainException($"No se encontró el cobro con número {query.NumeroCobro}.");
        }

        string concepto;
        if (pago.ReservaId.HasValue)
        {
            concepto = "Reserva / Alquiler de Cancha";
        }
        else if (pago.InscripcionId.HasValue)
        {
            concepto = "Inscripción a Actividad / Competencia";
        }
        else
        {
            concepto = "Cobro General";
        }

        return new ImpresionCobroDto(
            pago.Id,
            "Complejo El Deporte Saludable - Sistema Tenis Ahora",
            pago.Fecha_pago,
            concepto,
            pago.importe,
            pago.porcentaje_pago,
            pago.tipo_pago.ToString(),
            pago.estado.ToString(),
            pago.ReservaId,
            pago.InscripcionId,
            pago.MercadoPagoPaymentId,
            pago.Recibo?.Id
        );
    }
}
