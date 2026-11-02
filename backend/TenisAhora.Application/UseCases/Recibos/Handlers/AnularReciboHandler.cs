using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Recibos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Recibos.Commands;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Recibos.Handlers;

/// <summary>
/// Caso de Uso: Anular Recibo Emitido [RF-71]
/// </summary>
public class AnularReciboHandler
{
    private readonly IReciboRepository _reciboRepository;

    public AnularReciboHandler(IReciboRepository reciboRepository)
    {
        _reciboRepository = reciboRepository;
    }

    public async Task<ReciboResponseDto> Handle(AnularReciboCommand command, CancellationToken ct = default)
    {
        var recibo = await _reciboRepository.ObtenerPorIdAsync(command.Id, ct);
        if (recibo is null)
        {
            throw new DomainException($"No se encontró el recibo con ID {command.Id}.");
        }

        if (recibo.Anulado)
        {
            throw new DomainException("El recibo seleccionado ya se encuentra anulado.");
        }

        recibo.Anulado = true;
        await _reciboRepository.ActualizarAsync(recibo, ct);

        return ReciboMapper.MapearADto(recibo);
    }
}
