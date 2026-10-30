using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Disponibilidad;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Disponibilidad.Commands;
using TenisAhora.Application.UseCases.Disponibilidad.Handlers;
using TenisAhora.Application.UseCases.Disponibilidad.Queries;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;
using Xunit;

namespace TenisAhora.UnitTests;

public class DisponibilidadTests
{
    private readonly IDisponibilidadRepository _disponibilidadRepository = Substitute.For<IDisponibilidadRepository>();
    private readonly ICanchaRepository _canchaRepository = Substitute.For<ICanchaRepository>();

    #region RF-16: Registrar Disponibilidad

    [Fact]
    public async Task CrearDisponibilidad_ConDatosValidos_DebeCrearYRetornarResponseDto()
    {
        // Arrange
        var cancha = new Cancha { Id = 1, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Ladrillo } };
        _canchaRepository.ObtenerPorIdAsync(1).Returns(cancha);
        _disponibilidadRepository.ExisteSolapamientoAsync(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(false);

        _disponibilidadRepository.CrearAsync(Arg.Any<Disponibilidad>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var entidad = callInfo.Arg<Disponibilidad>();
                entidad.Id = 10;
                return Task.CompletedTask;
            });

        var handler = new CrearDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new CrearDisponibilidadCommand(
            1,
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(1).AddHours(2),
            EstadoDisponibilidad.Disponible
        );

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(10, resultado.Id);
        Assert.Equal(1, resultado.CanchaId);
        Assert.Equal("Ladrillo", resultado.TipoCanchaSuperficie);
        Assert.Equal(EstadoDisponibilidad.Disponible, resultado.Estado);
        Assert.Equal("Disponible", resultado.EstadoNombre);

        await _disponibilidadRepository.Received(1).CrearAsync(Arg.Is<Disponibilidad>(d =>
            d.CanchaId == 1 &&
            d.estado == EstadoDisponibilidad.Disponible
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearDisponibilidad_FechaFinMenorIgualInicio_LanzaDomainException()
    {
        // Arrange
        var handler = new CrearDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new CrearDisponibilidadCommand(
            1,
            DateTime.Now.AddDays(1).AddHours(2),
            DateTime.Now.AddDays(1),
            EstadoDisponibilidad.Disponible
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("fecha/hora de fin debe ser posterior", ex.Message);
        await _disponibilidadRepository.DidNotReceive().CrearAsync(Arg.Any<Disponibilidad>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearDisponibilidad_FechaEnPasado_LanzaDomainException()
    {
        // Arrange
        var handler = new CrearDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new CrearDisponibilidadCommand(
            1,
            DateTime.Now.AddHours(-1),
            DateTime.Now.AddHours(1),
            EstadoDisponibilidad.Disponible
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("fecha u hora pasada", ex.Message);
        await _disponibilidadRepository.DidNotReceive().CrearAsync(Arg.Any<Disponibilidad>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearDisponibilidad_CanchaNoExiste_LanzaDomainException()
    {
        // Arrange
        _canchaRepository.ObtenerPorIdAsync(99).Returns((Cancha?)null);

        var handler = new CrearDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new CrearDisponibilidadCommand(
            99,
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(1).AddHours(2),
            EstadoDisponibilidad.Disponible
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("No se encontró una cancha con ID 99", ex.Message);
        await _disponibilidadRepository.DidNotReceive().CrearAsync(Arg.Any<Disponibilidad>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearDisponibilidad_ConSolapamiento_LanzaDomainException()
    {
        // Arrange
        var cancha = new Cancha { Id = 1, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Ladrillo } };
        _canchaRepository.ObtenerPorIdAsync(1).Returns(cancha);
        _disponibilidadRepository.ExisteSolapamientoAsync(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(true);

        var handler = new CrearDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new CrearDisponibilidadCommand(
            1,
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(1).AddHours(2),
            EstadoDisponibilidad.Disponible
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("se solapa con el período solicitado", ex.Message);
        await _disponibilidadRepository.DidNotReceive().CrearAsync(Arg.Any<Disponibilidad>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearDisponibilidad_EstadoMantenimiento_DebeCrearCorrectamente()
    {
        // Arrange
        var cancha = new Cancha { Id = 2, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Cemento } };
        _canchaRepository.ObtenerPorIdAsync(2).Returns(cancha);
        _disponibilidadRepository.ExisteSolapamientoAsync(2, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(false);

        _disponibilidadRepository.CrearAsync(Arg.Any<Disponibilidad>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var entidad = callInfo.Arg<Disponibilidad>();
                entidad.Id = 20;
                return Task.CompletedTask;
            });

        var handler = new CrearDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new CrearDisponibilidadCommand(
            2,
            DateTime.Now.AddDays(5),
            DateTime.Now.AddDays(5).AddHours(4),
            EstadoDisponibilidad.Mantenimiento
        );

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(20, resultado.Id);
        Assert.Equal(EstadoDisponibilidad.Mantenimiento, resultado.Estado);
        Assert.Equal("Mantenimiento", resultado.EstadoNombre);
    }

    #endregion

    #region RF-17: Consultar Disponibilidad

    [Fact]
    public async Task ObtenerPorCancha_CuandoExiste_RetornaListaConFiltros()
    {
        // Arrange
        var cancha = new Cancha { Id = 1, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Ladrillo } };
        var disponibilidades = new List<Disponibilidad>
        {
            new Disponibilidad { Id = 1, CanchaId = 1, Cancha = cancha, FechaHoraInicio = DateTime.Today.AddHours(8), FechaHoraFin = DateTime.Today.AddHours(10), estado = EstadoDisponibilidad.Disponible },
            new Disponibilidad { Id = 2, CanchaId = 1, Cancha = cancha, FechaHoraInicio = DateTime.Today.AddHours(14), FechaHoraFin = DateTime.Today.AddHours(16), estado = EstadoDisponibilidad.Disponible },
            new Disponibilidad { Id = 3, CanchaId = 1, Cancha = cancha, FechaHoraInicio = DateTime.Today.AddDays(1).AddHours(8), FechaHoraFin = DateTime.Today.AddDays(1).AddHours(10), estado = EstadoDisponibilidad.Mantenimiento }
        };

        _disponibilidadRepository.ObtenerPorCanchaYFiltrosAsync(1, null, null, null, Arg.Any<CancellationToken>()).Returns(disponibilidades);

        var handler = new GetDisponibilidadByCanchaHandler(_disponibilidadRepository);
        var query = new GetDisponibilidadByCanchaQuery(1);

        // Act
        var resultado = await handler.Handle(query);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.Count());
        Assert.Contains(resultado, d => d.Id == 1 && d.Estado == EstadoDisponibilidad.Disponible);
        Assert.Contains(resultado, d => d.Id == 2 && d.Estado == EstadoDisponibilidad.Disponible);
        Assert.Contains(resultado, d => d.Id == 3 && d.Estado == EstadoDisponibilidad.Mantenimiento);
        Assert.All(resultado, d => Assert.Equal("Ladrillo", d.TipoCanchaSuperficie));
    }

    [Fact]
    public async Task ObtenerPorCancha_ConFiltroFechaDesde_RetornaSoloPosteriores()
    {
        // Arrange
        var cancha = new Cancha { Id = 1, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Ladrillo } };
        var fechaDesde = DateTime.Today.AddHours(12);
        var disponibilidades = new List<Disponibilidad>
        {
            new Disponibilidad { Id = 2, CanchaId = 1, Cancha = cancha, FechaHoraInicio = DateTime.Today.AddHours(14), FechaHoraFin = DateTime.Today.AddHours(16), estado = EstadoDisponibilidad.Disponible }
        };

        _disponibilidadRepository.ObtenerPorCanchaYFiltrosAsync(1, fechaDesde, null, null, Arg.Any<CancellationToken>()).Returns(disponibilidades);

        var handler = new GetDisponibilidadByCanchaHandler(_disponibilidadRepository);
        var query = new GetDisponibilidadByCanchaQuery(1, fechaDesde);

        // Act
        var resultado = await handler.Handle(query);

        // Assert
        Assert.Single(resultado);
        Assert.Equal(2, resultado.First().Id);
        await _disponibilidadRepository.Received(1).ObtenerPorCanchaYFiltrosAsync(1, fechaDesde, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObtenerPorCancha_ConFiltroEstado_RetornaSoloEseEstado()
    {
        // Arrange
        var cancha = new Cancha { Id = 1, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Ladrillo } };
        var disponibilidades = new List<Disponibilidad>
        {
            new Disponibilidad { Id = 3, CanchaId = 1, Cancha = cancha, FechaHoraInicio = DateTime.Today.AddDays(1).AddHours(8), FechaHoraFin = DateTime.Today.AddDays(1).AddHours(10), estado = EstadoDisponibilidad.Mantenimiento }
        };

        _disponibilidadRepository.ObtenerPorCanchaYFiltrosAsync(1, null, null, EstadoDisponibilidad.Mantenimiento, Arg.Any<CancellationToken>()).Returns(disponibilidades);

        var handler = new GetDisponibilidadByCanchaHandler(_disponibilidadRepository);
        var query = new GetDisponibilidadByCanchaQuery(1, null, null, EstadoDisponibilidad.Mantenimiento);

        // Act
        var resultado = await handler.Handle(query);

        // Assert
        Assert.Single(resultado);
        Assert.Equal(EstadoDisponibilidad.Mantenimiento, resultado.First().Estado);
    }

    [Fact]
    public async Task ObtenerPorId_CuandoExiste_RetornaResponseDto()
    {
        // Arrange
        var cancha = new Cancha { Id = 1, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Pasto } };
        var disponibilidad = new Disponibilidad { Id = 5, CanchaId = 1, Cancha = cancha, FechaHoraInicio = DateTime.Today.AddHours(10), FechaHoraFin = DateTime.Today.AddHours(12), estado = EstadoDisponibilidad.Disponible };

        _disponibilidadRepository.ObtenerPorIdAsync(5, Arg.Any<CancellationToken>()).Returns(disponibilidad);

        var handler = new GetDisponibilidadByIdHandler(_disponibilidadRepository);
        var query = new GetDisponibilidadByIdQuery(5);

        // Act
        var resultado = await handler.Handle(query);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(5, resultado!.Id);
        Assert.Equal(1, resultado.CanchaId);
        Assert.Equal("Pasto", resultado.TipoCanchaSuperficie);
        Assert.Equal(EstadoDisponibilidad.Disponible, resultado.Estado);
    }

    [Fact]
    public async Task ObtenerPorId_CuandoNoExiste_RetornaNull()
    {
        // Arrange
        _disponibilidadRepository.ObtenerPorIdAsync(999, Arg.Any<CancellationToken>()).Returns((Disponibilidad?)null);

        var handler = new GetDisponibilidadByIdHandler(_disponibilidadRepository);
        var query = new GetDisponibilidadByIdQuery(999);

        // Act
        var resultado = await handler.Handle(query);

        // Assert
        Assert.Null(resultado);
    }

    #endregion

    #region RF-18: Modificar Disponibilidad

    [Fact]
    public async Task ModificarDisponibilidad_CuandoExiste_ActualizaDatosYPersiste()
    {
        // Arrange
        var cancha = new Cancha { Id = 1, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Ladrillo } };
        var disponibilidadExistente = new Disponibilidad { Id = 1, CanchaId = 1, Cancha = cancha, FechaHoraInicio = DateTime.Today.AddHours(8), FechaHoraFin = DateTime.Today.AddHours(10), estado = EstadoDisponibilidad.Disponible };

        _disponibilidadRepository.ObtenerPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(disponibilidadExistente);
        _disponibilidadRepository.ExisteSolapamientoAsync(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), 1, Arg.Any<CancellationToken>()).Returns(false);
        _canchaRepository.ObtenerPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(cancha);

        var handler = new ModificarDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new ModificarDisponibilidadCommand(
            DateTime.Today.AddHours(14),
            DateTime.Today.AddHours(16),
            EstadoDisponibilidad.Mantenimiento
        );

        // Act
        var resultado = await handler.Handle(1, command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(1, resultado!.Id);
        Assert.Equal(EstadoDisponibilidad.Mantenimiento, resultado.Estado);
        Assert.Equal("Mantenimiento", resultado.EstadoNombre);
        Assert.Equal(14, resultado.FechaHoraInicio.Hour);
        Assert.Equal(16, resultado.FechaHoraFin.Hour);

        await _disponibilidadRepository.Received(1).ActualizarAsync(Arg.Is<Disponibilidad>(d =>
            d.Id == 1 &&
            d.FechaHoraInicio == command.FechaHoraInicio &&
            d.FechaHoraFin == command.FechaHoraFin &&
            d.estado == EstadoDisponibilidad.Mantenimiento
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ModificarDisponibilidad_CuandoNoExiste_RetornaNull()
    {
        // Arrange
        _disponibilidadRepository.ObtenerPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Disponibilidad?)null);

        var handler = new ModificarDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new ModificarDisponibilidadCommand(
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(1).AddHours(2),
            EstadoDisponibilidad.Disponible
        );

        // Act
        var resultado = await handler.Handle(99, command);

        // Assert
        Assert.Null(resultado);
        await _disponibilidadRepository.DidNotReceive().ActualizarAsync(Arg.Any<Disponibilidad>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ModificarDisponibilidad_FechaFinMenorIgualInicio_LanzaDomainException()
    {
        // Arrange
        var cancha = new Cancha { Id = 1, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Ladrillo } };
        var disponibilidadExistente = new Disponibilidad { Id = 1, CanchaId = 1, Cancha = cancha, FechaHoraInicio = DateTime.Today.AddHours(8), FechaHoraFin = DateTime.Today.AddHours(10), estado = EstadoDisponibilidad.Disponible };

        _disponibilidadRepository.ObtenerPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(disponibilidadExistente);

        var handler = new ModificarDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new ModificarDisponibilidadCommand(
            DateTime.Today.AddHours(16),
            DateTime.Today.AddHours(14),
            EstadoDisponibilidad.Disponible
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(1, command));
        Assert.Contains("fecha/hora de fin debe ser posterior", ex.Message);
        await _disponibilidadRepository.DidNotReceive().ActualizarAsync(Arg.Any<Disponibilidad>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ModificarDisponibilidad_FechaEnPasado_LanzaDomainException()
    {
        // Arrange
        var cancha = new Cancha { Id = 1, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Ladrillo } };
        var disponibilidadExistente = new Disponibilidad { Id = 1, CanchaId = 1, Cancha = cancha, FechaHoraInicio = DateTime.Today.AddHours(8), FechaHoraFin = DateTime.Today.AddHours(10), estado = EstadoDisponibilidad.Disponible };

        _disponibilidadRepository.ObtenerPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(disponibilidadExistente);

        var handler = new ModificarDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new ModificarDisponibilidadCommand(
            DateTime.Now.AddHours(-2),
            DateTime.Now.AddHours(-1),
            EstadoDisponibilidad.Disponible
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(1, command));
        Assert.Contains("fecha u hora pasada", ex.Message);
        await _disponibilidadRepository.DidNotReceive().ActualizarAsync(Arg.Any<Disponibilidad>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ModificarDisponibilidad_ConSolapamientoConOtra_LanzaDomainException()
    {
        // Arrange
        var cancha = new Cancha { Id = 1, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Ladrillo } };
        var disponibilidadExistente = new Disponibilidad { Id = 1, CanchaId = 1, Cancha = cancha, FechaHoraInicio = DateTime.Today.AddHours(8), FechaHoraFin = DateTime.Today.AddHours(10), estado = EstadoDisponibilidad.Disponible };

        _disponibilidadRepository.ObtenerPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(disponibilidadExistente);
        _disponibilidadRepository.ExisteSolapamientoAsync(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), 1, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new ModificarDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new ModificarDisponibilidadCommand(
            DateTime.Today.AddHours(14),
            DateTime.Today.AddHours(16),
            EstadoDisponibilidad.Disponible
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(1, command));
        Assert.Contains("se solapa con el período solicitado", ex.Message);
        await _disponibilidadRepository.DidNotReceive().ActualizarAsync(Arg.Any<Disponibilidad>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ModificarDisponibilidad_SinSolapamiento_PermiteCambiarAEstadoMantenimiento()
    {
        // Arrange
        var cancha = new Cancha { Id = 2, TipoDeCancha = new TipoDeCancha { superficie = Superficie.Cemento } };
        var disponibilidadExistente = new Disponibilidad { Id = 2, CanchaId = 2, Cancha = cancha, FechaHoraInicio = DateTime.Today.AddDays(1).AddHours(8), FechaHoraFin = DateTime.Today.AddDays(1).AddHours(10), estado = EstadoDisponibilidad.Disponible };

        _disponibilidadRepository.ObtenerPorIdAsync(2, Arg.Any<CancellationToken>()).Returns(disponibilidadExistente);
        _disponibilidadRepository.ExisteSolapamientoAsync(2, Arg.Any<DateTime>(), Arg.Any<DateTime>(), 2, Arg.Any<CancellationToken>()).Returns(false);
        _canchaRepository.ObtenerPorIdAsync(2, Arg.Any<CancellationToken>()).Returns(cancha);

        var handler = new ModificarDisponibilidadHandler(_disponibilidadRepository, _canchaRepository);
        var command = new ModificarDisponibilidadCommand(
            DateTime.Today.AddDays(1).AddHours(8),
            DateTime.Today.AddDays(1).AddHours(10),
            EstadoDisponibilidad.Mantenimiento
        );

        // Act
        var resultado = await handler.Handle(2, command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(EstadoDisponibilidad.Mantenimiento, resultado!.Estado);
    }

    #endregion
}