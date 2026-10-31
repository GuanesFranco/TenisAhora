using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Canchas;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Canchas.Commands;
using TenisAhora.Application.UseCases.Canchas.Handlers;
using TenisAhora.Application.UseCases.Canchas.Queries;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using Xunit;

namespace TenisAhora.UnitTests;

public class CanchasTests
{
    private readonly ICanchaRepository _repository = Substitute.For<ICanchaRepository>();

    [Fact]
    public async Task CrearCancha_ConFotoUrlPersonalizada_GuardaEnDbYRetornaDto()
    {
        // Arrange
        var tipoLadrillo = new TipoDeCancha(Superficie.Ladrillo, 4, 12000.0) { Id = 1 };
        var fotoUrl = "https://tenisahora.com/fotos/pista-central.jpg";
        var handler = new CreateCanchaHandler(_repository);
        var command = new CreateCanchaCommand
        {
            TipoDeCanchaId = 1,
            FotoUrl = fotoUrl
        };

        var canchaCreada = new Cancha
        {
            Id = 5,
            TipoDeCanchaId = 1,
            TipoDeCancha = tipoLadrillo,
            FotoUrl = fotoUrl
        };

        _repository.CrearAsync(1, fotoUrl).Returns(canchaCreada);

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(5, resultado.Id);
        Assert.Equal(1, resultado.TipoDeCanchaId);
        Assert.Equal("Ladrillo", resultado.Superficie);
        Assert.Equal(4, resultado.Capacidad);
        Assert.Equal(12000.0, resultado.Precio);
        Assert.Equal(fotoUrl, resultado.FotoUrl);

        await _repository.Received(1).CrearAsync(1, fotoUrl);
    }

    [Fact]
    public async Task CrearCancha_SinFotoUrl_RetornaFotoPorDefectoDeSuperficie()
    {
        // Arrange
        var tipoCemento = new TipoDeCancha(Superficie.Cemento, 2, 9500.0) { Id = 2 };
        var fotoPorDefecto = "/courts/cemento.jpg?v=3";
        var handler = new CreateCanchaHandler(_repository);
        var command = new CreateCanchaCommand
        {
            TipoDeCanchaId = 2,
            FotoUrl = null
        };

        var canchaCreada = new Cancha
        {
            Id = 6,
            TipoDeCanchaId = 2,
            TipoDeCancha = tipoCemento,
            FotoUrl = fotoPorDefecto
        };

        _repository.CrearAsync(2, null).Returns(canchaCreada);

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(6, resultado.Id);
        Assert.Equal("/courts/cemento.jpg?v=3", resultado.FotoUrl);

        await _repository.Received(1).CrearAsync(2, null);
    }

    [Fact]
    public async Task ActualizarCancha_ConNuevaFotoYTipo_ActualizaYRetornaDto()
    {
        // Arrange
        var tipoPasto = new TipoDeCancha(Superficie.Pasto, 4, 16000.0) { Id = 3 };
        var nuevaFotoUrl = "/courts/pasto.jpg?v=5";
        var handler = new UpdateCanchaHandler(_repository);
        var command = new UpdateCanchaCommand
        {
            Id = 2,
            TipoDeCanchaId = 3,
            FotoUrl = nuevaFotoUrl
        };

        var canchaActualizada = new Cancha
        {
            Id = 2,
            TipoDeCanchaId = 3,
            TipoDeCancha = tipoPasto,
            FotoUrl = nuevaFotoUrl
        };

        _repository.ActualizarAsync(2, 3, nuevaFotoUrl).Returns(canchaActualizada);

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Id);
        Assert.Equal(3, resultado.TipoDeCanchaId);
        Assert.Equal("Pasto", resultado.Superficie);
        Assert.Equal(nuevaFotoUrl, resultado.FotoUrl);

        await _repository.Received(1).ActualizarAsync(2, 3, nuevaFotoUrl);
    }

    [Fact]
    public async Task EliminarCancha_ExistenteSinReservas_EliminaYRetornaTrue()
    {
        // Arrange
        var handler = new DeleteCanchaHandler(_repository);
        var command = new DeleteCanchaCommand { Id = 4 };

        _repository.EliminarAsync(4).Returns(true);

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.True(resultado);
        await _repository.Received(1).EliminarAsync(4);
    }

    [Fact]
    public async Task EliminarCancha_ConReservasAsociadas_LanzaExcepcion()
    {
        // Arrange
        var handler = new DeleteCanchaHandler(_repository);
        var command = new DeleteCanchaCommand { Id = 1 };

        _repository.EliminarAsync(1)
            .Returns(Task.FromException<bool>(new InvalidOperationException("No se puede eliminar una cancha que tiene reservas o partidos asociados.")));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command));
        Assert.Contains("reservas o partidos", ex.Message);
    }

    [Fact]
    public async Task ObtenerTodas_RetornaListaDeCanchasConFotoUrl()
    {
        // Arrange
        var tipoLadrillo = new TipoDeCancha(Superficie.Ladrillo, 4, 12000.0) { Id = 1 };
        var tipoCemento = new TipoDeCancha(Superficie.Cemento, 2, 9500.0) { Id = 2 };

        var lista = new List<Cancha>
        {
            new Cancha { Id = 1, TipoDeCanchaId = 1, TipoDeCancha = tipoLadrillo, FotoUrl = "/courts/ladrillo.jpg?v=3" },
            new Cancha { Id = 2, TipoDeCanchaId = 2, TipoDeCancha = tipoCemento, FotoUrl = "/courts/cemento.jpg?v=3" }
        };

        _repository.ObtenerTodasAsync().Returns(lista);
        var handler = new GetAllCanchasHandler(_repository);

        // Act
        var resultado = await handler.Handle(new GetAllCanchasQuery());

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        Assert.Equal("/courts/ladrillo.jpg?v=3", resultado[0].FotoUrl);
        Assert.Equal("/courts/cemento.jpg?v=3", resultado[1].FotoUrl);
    }
}
