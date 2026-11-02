using System;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Recibos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Recibos.Commands;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Recibos.Handlers;

/// <summary>
/// Caso de Uso: Generar Recibo [RF-69]
/// </summary>
public class GenerarReciboHandler
{
    private readonly IReciboRepository _reciboRepository;
    private readonly IPagoRepository _pagoRepository;

    public GenerarReciboHandler(IReciboRepository reciboRepository, IPagoRepository pagoRepository)
    {
        _reciboRepository = reciboRepository;
        _pagoRepository = pagoRepository;
    }

    public async Task<ReciboResponseDto> Handle(GenerarReciboCommand command, CancellationToken ct = default)
    {
        var pago = await _pagoRepository.ObtenerPorIdAsync(command.NumeroCobro, ct);
        if (pago is null)
        {
            throw new DomainException($"No existe un cobro con el número {command.NumeroCobro}.");
        }

        if (pago.estado != EstadoPago.Validado)
        {
            throw new DomainException("Solo se pueden generar recibos para cobros en estado Validado.");
        }

        var reciboExistente = await _reciboRepository.ObtenerPorPagoIdAsync(pago.Id, ct);
        if (reciboExistente != null)
        {
            return ReciboMapper.MapearADto(reciboExistente);
        }

        var recibo = new Recibo
        {
            PagoId = pago.Id,
            Pago = pago,
            FechaEmision = DateTime.UtcNow,
            Anulado = false
        };

        await _reciboRepository.AgregarAsync(recibo, ct);

        return ReciboMapper.MapearADto(recibo);
    }
}
