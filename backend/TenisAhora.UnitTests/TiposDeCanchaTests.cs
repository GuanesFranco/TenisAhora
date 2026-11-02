using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.TiposDeCancha.Commands;
using TenisAhora.Application.UseCases.TiposDeCancha.Handlers;
using TenisAhora.Application.UseCases.TiposDeCancha.Queries;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;
using Xunit;

namespace TenisAhora.UnitTests;

public class TiposDeCanchaTests
{
    private readonly ITipoDeCanchaRepository _repository = Substitute.For<ITipoDeCanchaRepository>();

    #region RF-08: Registrar Tipos de Cancha

    [Theory]
    [InlineData(Superficie.Ladrillo, 4, 12000.0, "Polvo de ladrillo profesional")]
    [InlineData(Superficie.Cemento, 2, 9500.0, "Superficie rápida")]
    [InlineData(Superficie.Pasto, 4, 16000.0, "Césped natural")]
    public async Task CrearTipoDeCancha_ConDatosValidos_DebeCrearYRetornarResponseDto(
        Superficie superficie, int capacidad, double precio, string caracteristicas)
    {
        // Arrange
        var handler = new CreateTipoDeCanchaHandler(_repository);
        var command = new CreateTipoDeCanchaCommand
        {
            Superficie = superficie,
            Capacidad = capacidad,
            Precio = precio,
            Caracteristicas = caracteristicas
        };

        _repository.CrearAsync(Arg.Any<TipoDeCancha>())
            .Returns(callInfo =>
            {
                var entidad = callInfo.Arg<TipoDeCancha>();
                entidad.Id = 10;
                return entidad;
            });

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(10, resultado.Id);
        Assert.Equal(superficie, resultado.Superficie);
        Assert.Equal(superficie.ToString(), resultado.SuperficieNombre);
        Assert.Equal(capacidad, resultado.Capacidad);
        Assert.Equal(precio, resultado.Precio);
        Assert.Equal(caracteristicas, resultado.Caracteristicas);

        await _repository.Received(1).CrearAsync(Arg.Is<TipoDeCancha>(t =>
            t.superficie == superficie &&
            t.capacidad == capacidad &&
            t.precio == precio &&
            t.caracteristicas == caracteristicas
        ));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task CrearTipoDeCancha_ConCapacidadInvalida_LanzaDomainException(int capacidadInvalida)
    {
        // Arrange
        var handler = new CreateTipoDeCanchaHandler(_repository);
        var command = new CreateTipoDeCanchaCommand
        {
            Superficie = Superficie.Ladrillo,
            Capacidad = capacidadInvalida,
            Precio = 10000,
            Caracteristicas = "Cancha estándar"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("capacidad de la cancha debe ser mayor a 0", ex.Message);
        await _repository.DidNotReceive().CrearAsync(Arg.Any<TipoDeCancha>());
    }

    [Fact]
    public async Task CrearTipoDeCancha_ConPrecioNegativo_LanzaDomainException()
    {
        // Arrange
        var handler = new CreateTipoDeCanchaHandler(_repository);
        var command = new CreateTipoDeCanchaCommand
        {
            Superficie = Superficie.Cemento,
            Capacidad = 4,
            Precio = -100,
            Caracteristicas = "Cancha estándar"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("precio del tipo de cancha no puede ser negativo", ex.Message);
        await _repository.DidNotReceive().CrearAsync(Arg.Any<TipoDeCancha>());
    }

    #endregion

    #region RF-09: Modificar Tipos de Cancha

    [Fact]
    public async Task ModificarTipoDeCancha_CuandoExiste_ActualizaDatosYPersiste()
    {
        // Arrange
        var tipoExistente = new TipoDeCancha(Superficie.Cemento, 2, 8000, "Cancha vieja")
        {
            Id = 1
        };

        _repository.ObtenerPorIdParaActualizarAsync(1).Returns(tipoExistente);

        var handler = new UpdateTipoDeCanchaHandler(_repository);
        var command = new UpdateTipoDeCanchaCommand
        {
            Superficie = Superficie.Ladrillo,
            Capacidad = 4,
            Precio = 14000,
            Caracteristicas = "Remodelada a polvo de ladrillo con iluminación LED"
        };

        // Act
        var resultado = await handler.Handle(1, command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Id);
        Assert.Equal(Superficie.Ladrillo, resultado.Superficie);
        Assert.Equal(4, resultado.Capacidad);
        Assert.Equal(14000, resultado.Precio);
        Assert.Equal("Remodelada a polvo de ladrillo con iluminación LED", resultado.Caracteristicas);

        await _repository.Received(1).GuardarCambiosAsync();
    }

    [Fact]
    public async Task ModificarTipoDeCancha_CuandoNoExiste_RetornaNull()
    {
        // Arrange
        _repository.ObtenerPorIdParaActualizarAsync(99).Returns((TipoDeCancha?)null);

        var handler = new UpdateTipoDeCanchaHandler(_repository);
        var command = new UpdateTipoDeCanchaCommand
        {
            Superficie = Superficie.Pasto,
            Capacidad = 4,
            Precio = 15000
        };

        // Act
        var resultado = await handler.Handle(99, command);

        // Assert
        Assert.Null(resultado);
        await _repository.DidNotReceive().GuardarCambiosAsync();
    }

    #endregion

    #region RF-10: Consultar Tipos de Cancha

    [Fact]
    public async Task ObtenerPorId_CuandoExiste_RetornaResponseDto()
    {
        // Arrange
        var tipo = new TipoDeCancha(Superficie.Pasto, 4, 18000, "Césped natural para torneos")
        {
            Id = 3
        };

        _repository.ObtenerPorIdAsync(3).Returns(tipo);

        var handler = new GetTipoDeCanchaByIdHandler(_repository);
        var query = new GetTipoDeCanchaByIdQuery(3);

        // Act
        var resultado = await handler.Handle(query);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.Id);
        Assert.Equal(Superficie.Pasto, resultado.Superficie);
        Assert.Equal("Pasto", resultado.SuperficieNombre);
        Assert.Equal(4, resultado.Capacidad);
        Assert.Equal(18000, resultado.Precio);
        Assert.Equal("Césped natural para torneos", resultado.Caracteristicas);
    }

    [Fact]
    public async Task ObtenerPorId_CuandoNoExiste_RetornaNull()
    {
        // Arrange
        _repository.ObtenerPorIdAsync(999).Returns((TipoDeCancha?)null);

        var handler = new GetTipoDeCanchaByIdHandler(_repository);
        var query = new GetTipoDeCanchaByIdQuery(999);

        // Act
        var resultado = await handler.Handle(query);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObtenerTodos_RetornaListaDeTiposDeCancha()
    {
        // Arrange
        var tipos = new List<TipoDeCancha>
        {
            new TipoDeCancha(Superficie.Ladrillo, 4, 12000, "Ladrillo") { Id = 1 },
            new TipoDeCancha(Superficie.Cemento, 2, 9000, "Cemento") { Id = 2 },
            new TipoDeCancha(Superficie.Pasto, 4, 15000, "Pasto") { Id = 3 }
        };

        _repository.ObtenerTodosAsync().Returns(tipos);

        var handler = new GetTiposDeCanchaHandler(_repository);
        var query = new GetTiposDeCanchaQuery();

        // Act
        var resultado = await handler.Handle(query);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.Count);
        Assert.Contains(resultado, t => t.Superficie == Superficie.Ladrillo && t.Capacidad == 4);
        Assert.Contains(resultado, t => t.Superficie == Superficie.Cemento && t.Capacidad == 2);
        Assert.Contains(resultado, t => t.Superficie == Superficie.Pasto && t.Capacidad == 4);
    }

    #endregion

        #region RF-11: Eliminar Tipos de Cancha

        [Fact]
        public async Task EliminarTipoDeCancha_SinCanchasAsociadas_EliminaYRetornaTrue()
        {
            // Arrange
            var tipoExistente = new TipoDeCancha(Superficie.Ladrillo, 4, 12000, "Ladrillo")
            {
                Id = 1
            };

            _repository.ObtenerPorIdAsync(1).Returns(tipoExistente);
            _repository.TieneCanchasAsociadasAsync(1).Returns(false);
            _repository.EliminarAsync(1).Returns(true);

            var handler = new DeleteTipoDeCanchaHandler(_repository);

            // Act
            var resultado = await handler.Handle(1);

            // Assert
            Assert.True(resultado);
            await _repository.Received(1).ObtenerPorIdAsync(1);
            await _repository.Received(1).TieneCanchasAsociadasAsync(1);
            await _repository.Received(1).EliminarAsync(1);
        }

        [Fact]
        public async Task EliminarTipoDeCancha_ConCanchasAsociadas_LanzaDomainException()
        {
            // Arrange
            var tipoExistente = new TipoDeCancha(Superficie.Cemento, 2, 9000, "Cemento")
            {
                Id = 2
            };

            _repository.ObtenerPorIdAsync(2).Returns(tipoExistente);
            _repository.TieneCanchasAsociadasAsync(2).Returns(true);

            var handler = new DeleteTipoDeCanchaHandler(_repository);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(2));
            Assert.Contains("tiene canchas asociadas", ex.Message);

            await _repository.Received(1).ObtenerPorIdAsync(2);
            await _repository.Received(1).TieneCanchasAsociadasAsync(2);
            await _repository.DidNotReceive().EliminarAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task EliminarTipoDeCancha_NoExistente_RetornaFalse()
        {
            // Arrange
            _repository.ObtenerPorIdAsync(999).Returns((TipoDeCancha?)null);

            var handler = new DeleteTipoDeCanchaHandler(_repository);

            // Act
            var resultado = await handler.Handle(999);

            // Assert
            Assert.False(resultado);
            await _repository.Received(1).ObtenerPorIdAsync(999);
            await _repository.DidNotReceive().TieneCanchasAsociadasAsync(Arg.Any<int>());
            await _repository.DidNotReceive().EliminarAsync(Arg.Any<int>());
        }

        #endregion
    }

