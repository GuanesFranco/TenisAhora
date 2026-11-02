using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using TenisAhora.Application.DTOs.Pagos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Pagos.Commands;
using TenisAhora.Application.UseCases.Pagos.Handlers;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;
using Xunit;

namespace TenisAhora.UnitTests;

public class CobrosTests
{
    private readonly IPagoRepository _pagoRepo = Substitute.For<IPagoRepository>();
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IReservaRepository _reservaRepo = Substitute.For<IReservaRepository>();
    private readonly IStockReservasRepository _stockRepo = Substitute.For<IStockReservasRepository>();
    private readonly IMercadoPagoService _mercadoPagoService = Substitute.For<IMercadoPagoService>();
    private readonly IReciboRepository _reciboRepo = Substitute.For<IReciboRepository>();

    private Socio CrearSocioActivo(int dni = 40123456)
    {
        return new Socio
        {
            Id = 1,
            Dni = dni,
            Nombre = "Juan",
            Apellido = "Perez",
            Email = "juan.perez@tenisahora.com",
            Activo = true
        };
    }

    [Fact]
    public async Task RegistrarCobro_TarjetaDebitoConMasDeUnaCuota_LanzaDomainException()
    {
        // Arrange
        var socio = CrearSocioActivo(40123456);
        _usuarioRepo.ObtenerPorDniAsync(40123456).Returns(socio);

        var handler = new RegistrarCobroHandler(
            _pagoRepo, _usuarioRepo, _reservaRepo, _stockRepo, _mercadoPagoService, _reciboRepo);

        var command = new RegistrarCobroCommand(
            SocioDni: 40123456,
            ImporteBase: 5000f,
            TipoPago: TipoDePago.TarjetaDebito,
            TokenTarjetaMp: "tok_deb_123",
            PaymentMethodIdMp: "debvisa",
            Cuotas: 3);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("tarjeta de débito solo admiten 1 cuota", ex.Message);
    }

    [Fact]
    public async Task RegistrarCobro_TarjetaCreditoAprobada_ValidaCobroConfirmaReservaYEmiteRecibo()
    {
        // Arrange
        var socio = CrearSocioActivo(40123456);
        _usuarioRepo.ObtenerPorDniAsync(40123456).Returns(socio);

        var reserva = new Reserva
        {
            Id = 10,
            importe = 10000f,
            estado = EstadoReserva.Pendiente
        };
        _reservaRepo.ObtenerPorIdAsync(10, Arg.Any<CancellationToken>()).Returns(reserva);

        _mercadoPagoService.ProcesarPagoTarjetaAsync(Arg.Any<ProcesarTarjetaMpRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ResultadoPagoMpDto(999111L, "approved", "accredited", true));

        var handler = new RegistrarCobroHandler(
            _pagoRepo, _usuarioRepo, _reservaRepo, _stockRepo, _mercadoPagoService, _reciboRepo);

        var command = new RegistrarCobroCommand(
            SocioDni: 40123456,
            ImporteBase: 0f,
            TipoPago: TipoDePago.TarjetaCredito,
            EsSena50Porciento: true,
            ReservaId: 10,
            TokenTarjetaMp: "tok_cred_123",
            PaymentMethodIdMp: "visa",
            Cuotas: 1);

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(5000f, resultado.Importe);
        Assert.Equal(EstadoReserva.Confirmada, reserva.estado);
        await _reservaRepo.Received(1).ActualizarReservaAsync(reserva, Arg.Any<CancellationToken>());
        await _reciboRepo.Received(1).AgregarAsync(Arg.Any<Recibo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegistrarCobro_PagoConQr_GeneraQrEnMercadoPagoYDejaEstadoPendiente()
    {
        // Arrange
        var socio = CrearSocioActivo(40123456);
        _usuarioRepo.ObtenerPorDniAsync(40123456).Returns(socio);

        _mercadoPagoService.GenerarQrPagoAsync(Arg.Any<GenerarQrMpRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ResultadoQrMpDto("pref_123", "https://mp/qr", "base64_qr"));

        var handler = new RegistrarCobroHandler(
            _pagoRepo, _usuarioRepo, _reservaRepo, _stockRepo, _mercadoPagoService, _reciboRepo);

        var command = new RegistrarCobroCommand(
            SocioDni: 40123456,
            ImporteBase: 3000f,
            TipoPago: TipoDePago.QR);

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(EstadoPago.Pendiente.ToString(), resultado.Estado);
        Assert.Equal("base64_qr", resultado.CodigoQrBase64);
        Assert.Equal("https://mp/qr", resultado.UrlPagoQr);
    }

    [Fact]
    public async Task ConfirmarCobroQr_ConPagoAprobadoEnMp_ValidaCobroConfirmaReservaYEmiteRecibo()
    {
        // Arrange
        var reserva = new Reserva
        {
            Id = 5,
            estado = EstadoReserva.Pendiente
        };

        var pago = new Pago
        {
            Id = 20,
            tipo_pago = TipoDePago.QR,
            estado = EstadoPago.Pendiente,
            porcentaje_pago = 50f,
            ReservaId = 5,
            Reserva = reserva
        };

        _pagoRepo.ObtenerPorIdAsync(20, Arg.Any<CancellationToken>()).Returns(pago);
        _reciboRepo.ObtenerPorPagoIdAsync(20, Arg.Any<CancellationToken>()).Returns((Recibo?)null);
        _mercadoPagoService.ConsultarPagoAsync(888111L, Arg.Any<CancellationToken>())
            .Returns(new ResultadoPagoMpDto(888111L, "approved", "accredited", true));

        var handler = new ConfirmarCobroQrHandler(
            _pagoRepo, _reservaRepo, _stockRepo, _mercadoPagoService, _reciboRepo);

        var command = new ConfirmarCobroQrCommand(20, 888111L);

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(EstadoPago.Validado, pago.estado);
        Assert.Equal(EstadoReserva.Confirmada, reserva.estado);
        await _reciboRepo.Received(1).AgregarAsync(Arg.Any<Recibo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegistrarCobro_SaldoFinalSinSenaPrevia_LanzaDomainException()
    {
        // Arrange
        var socio = CrearSocioActivo(40123456);
        _usuarioRepo.ObtenerPorDniAsync(40123456).Returns(socio);

        var reserva = new Reserva { Id = 12, importe = 8000f };
        _reservaRepo.ObtenerPorIdAsync(12, Arg.Any<CancellationToken>()).Returns(reserva);
        _pagoRepo.ObtenerPorReservaIdAsync(12, Arg.Any<CancellationToken>()).Returns(new List<Pago>());

        var handler = new RegistrarCobroHandler(
            _pagoRepo, _usuarioRepo, _reservaRepo, _stockRepo, _mercadoPagoService, _reciboRepo);

        var command = new RegistrarCobroCommand(
            SocioDni: 40123456,
            ImporteBase: 0f,
            TipoPago: TipoDePago.TarjetaCredito,
            EsSaldoFinalPartido: true,
            ReservaId: 12,
            TokenTarjetaMp: "tok_cred_123",
            PaymentMethodIdMp: "visa");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("No se registra el pago validado del 50% de seña", ex.Message);
    }

    [Fact]
    public async Task RegistrarCobro_SaldoFinalConSenaPrevia_RestituyeStockDeEquipamiento()
    {
        // Arrange
        var socio = CrearSocioActivo(40123456);
        _usuarioRepo.ObtenerPorDniAsync(40123456).Returns(socio);

        var reserva = new Reserva
        {
            Id = 15,
            importe = 10000f,
            DetallesStockReserva = new List<Detalle_stock_reserva>
            {
                new Detalle_stock_reserva { StockId = 1, cantidad = 2 }
            }
        };
        _reservaRepo.ObtenerPorIdAsync(15, Arg.Any<CancellationToken>()).Returns(reserva);

        var pagosPrevios = new List<Pago>
        {
            new Pago { estado = EstadoPago.Validado, porcentaje_pago = 50f }
        };
        _pagoRepo.ObtenerPorReservaIdAsync(15, Arg.Any<CancellationToken>()).Returns(pagosPrevios);

        _mercadoPagoService.ProcesarPagoTarjetaAsync(Arg.Any<ProcesarTarjetaMpRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ResultadoPagoMpDto(111222L, "approved", "accredited", true));

        var handler = new RegistrarCobroHandler(
            _pagoRepo, _usuarioRepo, _reservaRepo, _stockRepo, _mercadoPagoService, _reciboRepo);

        var command = new RegistrarCobroCommand(
            SocioDni: 40123456,
            ImporteBase: 0f,
            TipoPago: TipoDePago.TarjetaCredito,
            EsSaldoFinalPartido: true,
            ReservaId: 15,
            TokenTarjetaMp: "tok_cred_123",
            PaymentMethodIdMp: "visa");

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        await _stockRepo.Received(1).IncrementarStockAsync(1, 2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnularCobro_SinMotivo_LanzaDomainException()
    {
        // Arrange
        var handler = new AnularCobroHandler(_pagoRepo, _mercadoPagoService);
        var command = new AnularCobroCommand(1, "");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("obligatorio registrar el motivo de la anulación", ex.Message);
    }

    [Fact]
    public async Task AnularCobro_ConPaymentIdDeMercadoPago_ReembolsaEnMpYAnulaCobroYRecibo()
    {
        // Arrange
        var recibo = new Recibo { Id = 30, Anulado = false };
        var pago = new Pago
        {
            Id = 10,
            estado = EstadoPago.Validado,
            MercadoPagoPaymentId = 555666L,
            Recibo = recibo
        };
        _pagoRepo.ObtenerPorIdAsync(10, Arg.Any<CancellationToken>()).Returns(pago);

        var handler = new AnularCobroHandler(_pagoRepo, _mercadoPagoService);
        var command = new AnularCobroCommand(10, "Cancelación por mal tiempo");

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        await _mercadoPagoService.Received(1).AnularOReembolsarPagoAsync(555666L, Arg.Any<CancellationToken>());
        Assert.Equal(EstadoPago.Anulado, pago.estado);
        Assert.True(recibo.Anulado);
    }

    [Fact]
    public async Task ProcesarWebhookMp_PagoNoAprobado_RetornaFalseSinLanzarExcepcion()
    {
        // Arrange
        _mercadoPagoService.ConsultarPagoAsync(12345L, Arg.Any<CancellationToken>())
            .Returns(new ResultadoPagoMpDto(12345L, "pending", "pending_review", false, "10"));

        var confirmarQrHandler = new ConfirmarCobroQrHandler(
            _pagoRepo, _reservaRepo, _stockRepo, _mercadoPagoService, _reciboRepo);
        var webhookHandler = new ProcesarWebhookMpHandler(_mercadoPagoService, confirmarQrHandler);

        // Act
        var resultado = await webhookHandler.Handle(12345L);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task ProcesarWebhookMp_ExternalReferenceNoNumerico_RetornaFalse()
    {
        // Arrange
        _mercadoPagoService.ConsultarPagoAsync(12345L, Arg.Any<CancellationToken>())
            .Returns(new ResultadoPagoMpDto(12345L, "approved", "accredited", true, "referencia_invalida"));

        var confirmarQrHandler = new ConfirmarCobroQrHandler(
            _pagoRepo, _reservaRepo, _stockRepo, _mercadoPagoService, _reciboRepo);
        var webhookHandler = new ProcesarWebhookMpHandler(_mercadoPagoService, confirmarQrHandler);

        // Act
        var resultado = await webhookHandler.Handle(12345L);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task ProcesarWebhookMp_PagoAprobadoConNumeroCobroValido_ConfirmaCobroYRetornaTrue()
    {
        // Arrange
        var pago = new Pago
        {
            Id = 5,
            tipo_pago = TipoDePago.QR,
            estado = EstadoPago.Pendiente,
            porcentaje_pago = 50f
        };
        _pagoRepo.ObtenerPorIdAsync(5, Arg.Any<CancellationToken>()).Returns(pago);

        _mercadoPagoService.ConsultarPagoAsync(999888L, Arg.Any<CancellationToken>())
            .Returns(new ResultadoPagoMpDto(999888L, "approved", "accredited", true, "5"));

        var confirmarQrHandler = new ConfirmarCobroQrHandler(
            _pagoRepo, _reservaRepo, _stockRepo, _mercadoPagoService, _reciboRepo);
        var webhookHandler = new ProcesarWebhookMpHandler(_mercadoPagoService, confirmarQrHandler);

        // Act
        var resultado = await webhookHandler.Handle(999888L);

        // Assert
        Assert.True(resultado);
        Assert.Equal(EstadoPago.Validado, pago.estado);
        Assert.Equal(999888L, pago.MercadoPagoPaymentId);
        await _reciboRepo.Received(1).AgregarAsync(Arg.Any<Recibo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcesarWebhookMp_CobroYaValidadoPreviamente_RetornaTruePorIdempotencia()
    {
        // Arrange
        var pago = new Pago
        {
            Id = 5,
            tipo_pago = TipoDePago.QR,
            estado = EstadoPago.Validado, // Ya validado
            MercadoPagoPaymentId = 999888L
        };
        _pagoRepo.ObtenerPorIdAsync(5, Arg.Any<CancellationToken>()).Returns(pago);

        _mercadoPagoService.ConsultarPagoAsync(999888L, Arg.Any<CancellationToken>())
            .Returns(new ResultadoPagoMpDto(999888L, "approved", "accredited", true, "5"));

        var confirmarQrHandler = new ConfirmarCobroQrHandler(
            _pagoRepo, _reservaRepo, _stockRepo, _mercadoPagoService, _reciboRepo);
        var webhookHandler = new ProcesarWebhookMpHandler(_mercadoPagoService, confirmarQrHandler);

        // Act
        var resultado = await webhookHandler.Handle(999888L);

        // Assert
        Assert.True(resultado);
    }
}
