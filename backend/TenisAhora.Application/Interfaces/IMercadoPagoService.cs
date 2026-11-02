using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Pagos;

namespace TenisAhora.Application.Interfaces;

public interface IMercadoPagoService
{
    Task<ResultadoPagoMpDto> ProcesarPagoTarjetaAsync(ProcesarTarjetaMpRequest request, CancellationToken ct = default);
    Task<ResultadoQrMpDto> GenerarQrPagoAsync(GenerarQrMpRequest request, CancellationToken ct = default);
    Task<ResultadoPagoMpDto> ConsultarPagoAsync(long paymentId, CancellationToken ct = default);
    Task AnularOReembolsarPagoAsync(long paymentId, CancellationToken ct = default);
}
