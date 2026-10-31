using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using TenisAhora.Application.DTOs.Stock;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Stock.Commands;
using TenisAhora.Application.UseCases.Stock.Handlers;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;
using Xunit;

namespace TenisAhora.UnitTests;

public class StockTests
{
    private readonly IStockRepository _stockRepo = Substitute.For<IStockRepository>();
    private readonly IReservaRepository _reservaRepo = Substitute.For<IReservaRepository>();

    [Fact]
    public async Task RegistrarStock_ConCantidadNegativa_LanzaDomainException()
    {
        // Arrange
        var handler = new RegistrarStockHandler(_stockRepo);
        var command = new RegistrarStockCommand("Tubos de pelotas", -5);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("cantidad disponible no puede ser negativa", ex.Message);
    }

    [Fact]
    public async Task RegistrarStock_ConDatosValidos_PersisteYRetornaDto()
    {
        // Arrange
        var handler = new RegistrarStockHandler(_stockRepo);
        var command = new RegistrarStockCommand("Raqueta Wilson Pro", 15);

        _stockRepo.AgregarAsync(Arg.Any<Stock>(), Arg.Any<CancellationToken>())
            .Returns(new Stock
            {
                Id = 1,
                tipo_elemento = "Raqueta Wilson Pro",
                cantidad_disponible = 15
            });

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Id);
        Assert.Equal("Raqueta Wilson Pro", resultado.TipoElemento);
        Assert.Equal(15, resultado.CantidadDisponible);
        await _stockRepo.Received(1).AgregarAsync(
            Arg.Is<Stock>(s => s.tipo_elemento == "Raqueta Wilson Pro" && s.cantidad_disponible == 15),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ModificarStock_ConIdExistente_ActualizaCantidadCorrectamente()
    {
        // Arrange
        var handler = new ModificarStockHandler(_stockRepo);
        var stock = new Stock
        {
            Id = 2,
            tipo_elemento = "Red de tenis",
            cantidad_disponible = 4
        };

        _stockRepo.ObtenerPorIdAsync(2, Arg.Any<CancellationToken>()).Returns(stock);

        var command = new ModificarStockCommand(2, 10);

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Id);
        Assert.Equal(10, resultado.CantidadDisponible);
        Assert.Equal(10, stock.cantidad_disponible);
        await _stockRepo.Received(1).ActualizarAsync(stock, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EliminarStock_ConReservasAsociadas_LanzaDomainException()
    {
        // Arrange
        var handler = new EliminarStockHandler(_stockRepo);
        var stock = new Stock
        {
            Id = 3,
            tipo_elemento = "Pelotas Head",
            cantidad_disponible = 20
        };

        _stockRepo.ObtenerPorIdAsync(3, Arg.Any<CancellationToken>()).Returns(stock);
        _stockRepo.TieneReservasAsociadasAsync(3, Arg.Any<CancellationToken>()).Returns(true);

        var command = new EliminarStockCommand(3);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("asociado a reservas registradas", ex.Message);
        await _stockRepo.DidNotReceive().EliminarAsync(Arg.Any<Stock>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FinalizarAlquilerStock_ConReservaConfirmada_IncrementaStockDeCadaElemento()
    {
        // Arrange
        var handler = new FinalizarAlquilerStockHandler(_reservaRepo, _stockRepo);
        var reserva = new Reserva
        {
            Id = 10,
            estado = EstadoReserva.Confirmada,
            DetallesStockReserva = new List<Detalle_stock_reserva>
            {
                new Detalle_stock_reserva { StockId = 1, cantidad = 2 },
                new Detalle_stock_reserva { StockId = 4, cantidad = 3 }
            }
        };

        _reservaRepo.ObtenerPorIdAsync(10, Arg.Any<CancellationToken>()).Returns(reserva);

        var command = new FinalizarAlquilerStockCommand(10);

        // Act
        await handler.Handle(command);

        // Assert
        await _stockRepo.Received(1).IncrementarStockAsync(1, 2, Arg.Any<CancellationToken>());
        await _stockRepo.Received(1).IncrementarStockAsync(4, 3, Arg.Any<CancellationToken>());
    }
}
