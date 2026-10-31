using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Recibos.Commands;
using TenisAhora.Application.UseCases.Recibos.Handlers;
using TenisAhora.Application.UseCases.Recibos.Queries;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;
using Xunit;

namespace TenisAhora.UnitTests;

public class RecibosTests
{
    private readonly IReciboRepository _reciboRepo = Substitute.For<IReciboRepository>();
    private readonly IPagoRepository _pagoRepo = Substitute.For<IPagoRepository>();

    private Socio CrearSocio(int dni = 40123456, string nombre = "Juan", string apellido = "Perez")
    {
        return new Socio
        {
            Id = 1,
            Dni = dni,
            Nombre = nombre,
            Apellido = apellido,
            Activo = true
        };
    }

    private Pago CrearPago(
        int id = 1,
        EstadoPago estado = EstadoPago.Validado,
        TipoDePago tipo = TipoDePago.TarjetaDebito,
        float importe = 15000f,
        Socio? socio = null,
        bool esReserva = true)
    {
        var pago = new Pago
        {
            Id = id,
            importe = importe,
            Fecha_pago = DateTime.UtcNow,
            tipo_pago = tipo,
            estado = estado,
            porcentaje_pago = 100f
        };

        if (esReserva)
        {
            var s = socio ?? CrearSocio();
            pago.ReservaId = 10;
            pago.Reserva = new Reserva
            {
                Id = 10,
                importe = importe,
                Socios = new List<Socio> { s }
            };
        }
        else if (socio != null)
        {
            pago.InscripcionId = 20;
            pago.Inscripcion = new Inscripcion
            {
                Id = 20,
                SocioId = socio.Id,
                Socio = socio
            };
        }

        return pago;
    }

    [Fact]
    public async Task GenerarRecibo_ConPagoInexistente_LanzaDomainException()
    {
        // Arrange
        _pagoRepo.ObtenerPorIdAsync(999, Arg.Any<CancellationToken>()).Returns((Pago?)null);
        var handler = new GenerarReciboHandler(_reciboRepo, _pagoRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new GenerarReciboCommand(999)));
        Assert.Contains("No existe un cobro con el número 999", ex.Message);
    }

    [Fact]
    public async Task GenerarRecibo_ConPagoNoValidado_LanzaDomainException()
    {
        // Arrange
        var pago = CrearPago(estado: EstadoPago.Pendiente);
        _pagoRepo.ObtenerPorIdAsync(pago.Id, Arg.Any<CancellationToken>()).Returns(pago);
        var handler = new GenerarReciboHandler(_reciboRepo, _pagoRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new GenerarReciboCommand(pago.Id)));
        Assert.Contains("Solo se pueden generar recibos para cobros en estado Validado", ex.Message);
    }

    [Fact]
    public async Task GenerarRecibo_CuandoYaExisteRecibo_RetornaReciboExistenteSinDuplicar()
    {
        // Arrange
        var pago = CrearPago(id: 5, estado: EstadoPago.Validado, importe: 12000f);
        _pagoRepo.ObtenerPorIdAsync(pago.Id, Arg.Any<CancellationToken>()).Returns(pago);

        var reciboExistente = new Recibo { Id = 88, PagoId = pago.Id, Pago = pago, FechaEmision = DateTime.UtcNow, Anulado = false };
        _reciboRepo.ObtenerPorPagoIdAsync(pago.Id, Arg.Any<CancellationToken>()).Returns(reciboExistente);

        var handler = new GenerarReciboHandler(_reciboRepo, _pagoRepo);

        // Act
        var resultado = await handler.Handle(new GenerarReciboCommand(pago.Id));

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(88, resultado.Id);
        Assert.Equal(pago.Id, resultado.NumeroCobro);
        await _reciboRepo.DidNotReceive().AgregarAsync(Arg.Any<Recibo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerarRecibo_DeCobroValidado_CreaReciboCorrectamente()
    {
        // Arrange
        var socio = CrearSocio(41222333, "Martin", "Palermo");
        var pago = CrearPago(id: 15, estado: EstadoPago.Validado, tipo: TipoDePago.TarjetaCredito, importe: 20000f, socio: socio);
        _pagoRepo.ObtenerPorIdAsync(pago.Id, Arg.Any<CancellationToken>()).Returns(pago);
        _reciboRepo.ObtenerPorPagoIdAsync(pago.Id, Arg.Any<CancellationToken>()).Returns((Recibo?)null);

        _reciboRepo.AgregarAsync(Arg.Do<Recibo>(r => { r.Id = 100; }), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Recibo>());

        var handler = new GenerarReciboHandler(_reciboRepo, _pagoRepo);

        // Act
        var resultado = await handler.Handle(new GenerarReciboCommand(pago.Id));

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(pago.Id, resultado.NumeroCobro);
        Assert.False(resultado.Anulado);
        Assert.Equal(20000f, resultado.ImporteTotal);
        Assert.Equal(TipoDePago.TarjetaCredito.ToString(), resultado.MedioDePago);
        await _reciboRepo.Received(1).AgregarAsync(Arg.Any<Recibo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObtenerReciboPorId_Inexistente_LanzaDomainException()
    {
        // Arrange
        _reciboRepo.ObtenerPorIdAsync(55, Arg.Any<CancellationToken>()).Returns((Recibo?)null);
        var handler = new ObtenerReciboPorIdHandler(_reciboRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new ObtenerReciboPorIdQuery(55)));
        Assert.Contains("No se encontró el recibo con ID 55", ex.Message);
    }

    [Fact]
    public async Task ObtenerReciboPorId_Existente_RetornaDto()
    {
        // Arrange
        var pago = CrearPago(id: 7);
        var recibo = new Recibo { Id = 25, PagoId = pago.Id, Pago = pago, FechaEmision = DateTime.UtcNow, Anulado = false };
        _reciboRepo.ObtenerPorIdAsync(25, Arg.Any<CancellationToken>()).Returns(recibo);

        var handler = new ObtenerReciboPorIdHandler(_reciboRepo);

        // Act
        var resultado = await handler.Handle(new ObtenerReciboPorIdQuery(25));

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(25, resultado.Id);
        Assert.Equal(pago.Id, resultado.NumeroCobro);
        Assert.False(resultado.Anulado);
    }

    [Fact]
    public async Task ListarRecibos_RetornaTodosLosRecibos()
    {
        // Arrange
        var pago1 = CrearPago(id: 1);
        var pago2 = CrearPago(id: 2);
        var recibos = new List<Recibo>
        {
            new Recibo { Id = 1, PagoId = 1, Pago = pago1, FechaEmision = DateTime.UtcNow, Anulado = false },
            new Recibo { Id = 2, PagoId = 2, Pago = pago2, FechaEmision = DateTime.UtcNow, Anulado = true }
        };
        _reciboRepo.ListarTodosAsync(Arg.Any<CancellationToken>()).Returns(recibos);

        var handler = new ListarRecibosHandler(_reciboRepo);

        // Act
        var resultado = await handler.Handle(new ListarRecibosQuery());

        // Assert
        Assert.Equal(2, resultado.Count);
        Assert.Equal(1, resultado[0].Id);
        Assert.False(resultado[0].Anulado);
        Assert.Equal(2, resultado[1].Id);
        Assert.True(resultado[1].Anulado);
    }

    [Fact]
    public async Task AnularRecibo_Inexistente_LanzaDomainException()
    {
        // Arrange
        _reciboRepo.ObtenerPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Recibo?)null);
        var handler = new AnularReciboHandler(_reciboRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new AnularReciboCommand(99)));
        Assert.Contains("No se encontró el recibo con ID 99", ex.Message);
    }

    [Fact]
    public async Task AnularRecibo_YaAnulado_LanzaDomainException()
    {
        // Arrange
        var pago = CrearPago();
        var recibo = new Recibo { Id = 40, PagoId = pago.Id, Pago = pago, FechaEmision = DateTime.UtcNow, Anulado = true };
        _reciboRepo.ObtenerPorIdAsync(40, Arg.Any<CancellationToken>()).Returns(recibo);

        var handler = new AnularReciboHandler(_reciboRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new AnularReciboCommand(40)));
        Assert.Contains("El recibo seleccionado ya se encuentra anulado", ex.Message);
    }

    [Fact]
    public async Task AnularRecibo_Exitoso_MarcaAnuladoYPersiste()
    {
        // Arrange
        var pago = CrearPago();
        var recibo = new Recibo { Id = 40, PagoId = pago.Id, Pago = pago, FechaEmision = DateTime.UtcNow, Anulado = false };
        _reciboRepo.ObtenerPorIdAsync(40, Arg.Any<CancellationToken>()).Returns(recibo);

        var handler = new AnularReciboHandler(_reciboRepo);

        // Act
        var resultado = await handler.Handle(new AnularReciboCommand(40));

        // Assert
        Assert.True(resultado.Anulado);
        Assert.True(recibo.Anulado);
        await _reciboRepo.Received(1).ActualizarAsync(recibo, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImprimirRecibo_Inexistente_LanzaDomainException()
    {
        // Arrange
        _reciboRepo.ObtenerPorIdAsync(77, Arg.Any<CancellationToken>()).Returns((Recibo?)null);
        var handler = new ImprimirReciboHandler(_reciboRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new ImprimirReciboQuery(77)));
        Assert.Contains("No se encontró el recibo con ID 77", ex.Message);
    }

    [Fact]
    public async Task ImprimirRecibo_ConReserva_GeneraDetalleCorrecto()
    {
        // Arrange
        var socio = CrearSocio(33444555, "Guillermo", "Vilas");
        var pago = CrearPago(id: 10, estado: EstadoPago.Validado, tipo: TipoDePago.QR, importe: 12000f, socio: socio, esReserva: true);
        var recibo = new Recibo { Id = 50, PagoId = 10, Pago = pago, FechaEmision = new DateTime(2026, 10, 31, 15, 0, 0), Anulado = false };
        _reciboRepo.ObtenerPorIdAsync(50, Arg.Any<CancellationToken>()).Returns(recibo);

        var handler = new ImprimirReciboHandler(_reciboRepo);

        // Act
        var comprobante = await handler.Handle(new ImprimirReciboQuery(50));

        // Assert
        Assert.NotNull(comprobante);
        Assert.Equal(50, comprobante.ReciboId);
        Assert.Equal(10, comprobante.NumeroCobro);
        Assert.Equal("Complejo El Deporte Saludable - Sistema Tenis Ahora", comprobante.Establecimiento);
        Assert.Equal("Reserva / Alquiler de Cancha", comprobante.Concepto);
        Assert.Equal(12000f, comprobante.ImporteTotal);
        Assert.Equal(TipoDePago.QR.ToString(), comprobante.MedioDePago);
        Assert.Equal("Válido", comprobante.EstadoRecibo);
        Assert.Equal(33444555, comprobante.SocioDni);
        Assert.Equal("Guillermo Vilas", comprobante.NombreSocio);
    }
}
