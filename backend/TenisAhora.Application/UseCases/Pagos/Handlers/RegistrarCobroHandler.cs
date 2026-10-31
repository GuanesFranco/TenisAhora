using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Pagos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Pagos.Commands;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Pagos.Handlers;

/// <summary>
/// Caso de Uso: Registrar Cobro de Alquiler de Cancha, Clases o Entrenamientos [RF-65, RD-06, RD-07, RD-08]
/// </summary>
public class RegistrarCobroHandler
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly IStockReservasRepository _stockRepository;
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly IReciboRepository _reciboRepository;

    public RegistrarCobroHandler(
        IPagoRepository pagoRepository,
        IUsuarioRepository usuarioRepository,
        IReservaRepository reservaRepository,
        IStockReservasRepository stockRepository,
        IMercadoPagoService mercadoPagoService,
        IReciboRepository reciboRepository)
    {
        _pagoRepository = pagoRepository;
        _usuarioRepository = usuarioRepository;
        _reservaRepository = reservaRepository;
        _stockRepository = stockRepository;
        _mercadoPagoService = mercadoPagoService;
        _reciboRepository = reciboRepository;
    }

    public async Task<CobroResponseDto> Handle(RegistrarCobroCommand command, CancellationToken ct = default)
    {
        // 1. Validar Socio
        var persona = await _usuarioRepository.ObtenerPorDniAsync(command.SocioDni);
        if (persona is null || !persona.Activo)
        {
            throw new DomainException($"No existe un usuario activo con DNI {command.SocioDni}.");
        }

        // 2. Validar Reserva y calcular 50% Seña / 50% Saldo
        if (command.EsSena50Porciento && command.EsSaldoFinalPartido)
        {
            throw new DomainException("Un cobro no puede ser seña y saldo final simultáneamente.");
        }

        Reserva? reserva = null;
        if (command.ReservaId.HasValue)
        {
            reserva = await _reservaRepository.ObtenerPorIdAsync(command.ReservaId.Value, ct);
            if (reserva is null)
            {
                throw new DomainException("La reserva indicada no existe.");
            }
        }

        float montoBase = command.ImporteBase > 0 ? command.ImporteBase : (reserva?.importe ?? 0f);
        float porcentajePago = 100f;

        if (command.EsSena50Porciento)
        {
            montoBase = montoBase * 0.5f;
            porcentajePago = 50f;
        }
        else if (command.EsSaldoFinalPartido)
        {
            if (reserva is null)
            {
                throw new DomainException("Debe indicar una reserva para cobrar el saldo final del partido.");
            }

            var pagosReserva = await _pagoRepository.ObtenerPorReservaIdAsync(reserva.Id, ct);
            var tieneSenaValidada = pagosReserva.Any(p => p.estado == EstadoPago.Validado && p.porcentaje_pago == 50f);
            if (!tieneSenaValidada)
            {
                throw new DomainException("No se registra el pago validado del 50% de seña para esta reserva.");
            }

            montoBase = montoBase * 0.5f;
            porcentajePago = 50f;
        }

        // 3. Descuentos y Recargos
        if (command.DescuentoId.HasValue)
        {
            var descuento = await _pagoRepository.ObtenerDescuentoPorIdAsync(command.DescuentoId.Value, ct);
            if (descuento is null)
            {
                throw new DomainException("El descuento indicado no existe.");
            }

            montoBase = montoBase - (montoBase * (descuento.porcentaje / 100f));
        }

        if (command.RecargoCancelacion < 0)
        {
            throw new DomainException("El recargo por cancelación no puede ser negativo.");
        }

        float importeFinal = montoBase + command.RecargoCancelacion;
        if (importeFinal <= 0)
        {
            throw new DomainException("El importe total a cobrar debe ser mayor a cero.");
        }

        // 4. Procesamiento por Medio de Pago
        if (command.TipoPago == TipoDePago.TarjetaDebito || command.TipoPago == TipoDePago.TarjetaCredito)
        {
            if (string.IsNullOrWhiteSpace(command.TokenTarjetaMp) || string.IsNullOrWhiteSpace(command.PaymentMethodIdMp))
            {
                throw new DomainException("Se requieren TokenTarjetaMp y PaymentMethodIdMp para procesar pagos con tarjeta.");
            }

            if (command.TipoPago == TipoDePago.TarjetaDebito && command.Cuotas != 1)
            {
                throw new DomainException("Los pagos con tarjeta de débito solo admiten 1 cuota.");
            }

            var resultadoMp = await _mercadoPagoService.ProcesarPagoTarjetaAsync(new ProcesarTarjetaMpRequest(
                (decimal)importeFinal,
                command.TokenTarjetaMp,
                command.PaymentMethodIdMp,
                command.Cuotas,
                persona.Email,
                persona.Dni,
                $"Cobro TenisAhora - DNI {persona.Dni}"
            ), ct);

            if (!resultadoMp.Aprobado)
            {
                throw new DomainException($"El pago con tarjeta fue rechazado por Mercado Pago: {resultadoMp.StatusDetail}");
            }

            var pago = new Pago
            {
                estado = EstadoPago.Validado,
                MercadoPagoPaymentId = resultadoMp.PaymentId,
                importe = importeFinal,
                Fecha_pago = DateTime.UtcNow,
                tipo_pago = command.TipoPago,
                porcentaje_pago = porcentajePago,
                ReservaId = command.ReservaId,
                InscripcionId = command.InscripcionId,
                DescuentoId = command.DescuentoId
            };

            await _pagoRepository.AgregarAsync(pago, ct);

            if (pago.estado == EstadoPago.Validado && pago.Recibo == null)
            {
                var nuevoRecibo = new Recibo
                {
                    PagoId = pago.Id,
                    FechaEmision = DateTime.UtcNow,
                    Anulado = false
                };
                await _reciboRepository.AgregarAsync(nuevoRecibo, ct);
                pago.Recibo = nuevoRecibo;
            }

            if (reserva != null)
            {
                if (command.EsSena50Porciento)
                {
                    reserva.estado = EstadoReserva.Confirmada;
                    await _reservaRepository.ActualizarReservaAsync(reserva, ct);
                }
                else if (command.EsSaldoFinalPartido)
                {
                    foreach (var detalle in reserva.DetallesStockReserva)
                    {
                        await _stockRepository.IncrementarStockAsync(detalle.StockId, detalle.cantidad, ct);
                    }
                }
            }

            return new CobroResponseDto(
                pago.Id,
                persona.Dni,
                pago.importe,
                pago.porcentaje_pago,
                pago.tipo_pago.ToString(),
                pago.estado.ToString(),
                pago.Fecha_pago,
                pago.ReservaId,
                pago.InscripcionId,
                pago.DescuentoId,
                pago.MercadoPagoPaymentId,
                null,
                null,
                null,
                pago.Recibo?.Id
            );
        }
        else if (command.TipoPago == TipoDePago.QR)
        {
            var pago = new Pago
            {
                estado = EstadoPago.Pendiente,
                importe = importeFinal,
                Fecha_pago = DateTime.UtcNow,
                tipo_pago = TipoDePago.QR,
                porcentaje_pago = porcentajePago,
                ReservaId = command.ReservaId,
                InscripcionId = command.InscripcionId,
                DescuentoId = command.DescuentoId
            };

            await _pagoRepository.AgregarAsync(pago, ct);

            var qrResult = await _mercadoPagoService.GenerarQrPagoAsync(new GenerarQrMpRequest(
                (decimal)importeFinal,
                $"Cobro #{pago.Id} - TenisAhora",
                pago.Id.ToString(),
                persona.Email
            ), ct);

            return new CobroResponseDto(
                pago.Id,
                persona.Dni,
                pago.importe,
                pago.porcentaje_pago,
                pago.tipo_pago.ToString(),
                pago.estado.ToString(),
                pago.Fecha_pago,
                pago.ReservaId,
                pago.InscripcionId,
                pago.DescuentoId,
                pago.MercadoPagoPaymentId,
                qrResult.PreferenceId,
                qrResult.UrlPagoQr,
                qrResult.CodigoQrBase64,
                pago.Recibo?.Id
            );
        }

        throw new DomainException($"Medio de pago {command.TipoPago} no soportado.");
    }
}
