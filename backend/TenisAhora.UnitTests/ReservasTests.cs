using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using TenisAhora.Application.DTOs.Reservas;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Reservas;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;
using Xunit;

namespace TenisAhora.UnitTests;

public class ReservasTests
{
    private readonly IReservaRepository _reservaRepo = Substitute.For<IReservaRepository>();
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IStockReservasRepository _stockRepo = Substitute.For<IStockReservasRepository>();

    private Socio CrearSocioMock(int dni, bool activo = true, EstadoMembresia membresia = EstadoMembresia.Activa)
    {
        return new Socio
        {
            Id = 1,
            Dni = dni,
            Nombre = "Melany",
            Apellido = "Godoy",
            Activo = activo,
            estadoMembresia = membresia
        };
    }

    private Cancha CrearCanchaMock(int id = 1, double precioPorHora = 2000.0)
    {
        return new Cancha
        {
            Id = id,
            TipoDeCanchaId = 1,
            TipoDeCancha = new TipoDeCancha
            {
                Id = 1,
                superficie = Superficie.Ladrillo,
                precio = precioPorHora
            }
        };
    }

    [Fact]
    public async Task CrearReserva_ConHoraFinMenorOIgualAInicio_LanzaDomainException()
    {
        // Arrange
        var handler = new CrearReservaHandler(_reservaRepo, _usuarioRepo, _stockRepo);
        var ahora = DateTime.Now.AddDays(1);
        var command = new CrearReservaCommand(
            CanchaId: 1,
            fecha_reserva: ahora.Date,
            HoraInicio: ahora.Date.AddHours(14),
            HoraFin: ahora.Date.AddHours(13), // Inválido
            SocioDni: 40123456,
            JugadoresDni: new List<int> { 40123456, 40123457 }
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("finalización no puede ser anterior", ex.Message);
    }

    [Fact]
    public async Task CrearReserva_ConDuracionMayorADosHoras_LanzaDomainException()
    {
        // Arrange
        var handler = new CrearReservaHandler(_reservaRepo, _usuarioRepo, _stockRepo);
        var ahora = DateTime.Now.AddDays(1);
        var command = new CrearReservaCommand(
            CanchaId: 1,
            fecha_reserva: ahora.Date,
            HoraInicio: ahora.Date.AddHours(10),
            HoraFin: ahora.Date.AddHours(13), // 3 horas -> Excede límite
            SocioDni: 40123456,
            JugadoresDni: new List<int> { 40123456, 40123457 }
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("2 horas de duración", ex.Message);
    }

    [Fact]
    public async Task CrearReserva_ConTresJugadores_LanzaDomainException()
    {
        // Arrange (regla RD-10 / RD-11: exactamente 2 o 4)
        _reservaRepo.ObtenerCanchaPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(CrearCanchaMock(1, 2000.0));
        var handler = new CrearReservaHandler(_reservaRepo, _usuarioRepo, _stockRepo);
        var ahora = DateTime.Now.AddDays(1);
        var command = new CrearReservaCommand(
            CanchaId: 1,
            fecha_reserva: ahora.Date,
            HoraInicio: ahora.Date.AddHours(10),
            HoraFin: ahora.Date.AddHours(11),
            SocioDni: 40123456,
            JugadoresDni: new List<int> { 40123456, 40123457, 40123458 } // 3 jugadores
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("2 o 4 jugadores", ex.Message);
    }

    [Fact]
    public async Task CrearReserva_ConCanchaSuperpuesta_LanzaDomainException()
    {
        // Arrange
        var socio1 = CrearSocioMock(40123456);
        var socio2 = CrearSocioMock(40123457);
        _usuarioRepo.ObtenerPorDniAsync(40123456).Returns(socio1);
        _usuarioRepo.ObtenerPorDniAsync(40123457).Returns(socio2);
        _reservaRepo.ObtenerCanchaPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(CrearCanchaMock(1, 2000.0));
        _reservaRepo.ExisteSuperposicionAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new CrearReservaHandler(_reservaRepo, _usuarioRepo, _stockRepo);
        var ahora = DateTime.Now.AddDays(1);
        var command = new CrearReservaCommand(
            CanchaId: 1,
            fecha_reserva: ahora.Date,
            HoraInicio: ahora.Date.AddHours(10),
            HoraFin: ahora.Date.AddHours(11),
            SocioDni: 40123456,
            JugadoresDni: new List<int> { 40123456, 40123457 }
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("no se encuentra disponible", ex.Message);
    }

    [Fact]
    public async Task CrearReserva_Exitosa_CalculaMontoSenaCincuentaPorCiento()
    {
        // Arrange
        var socio1 = CrearSocioMock(40123456);
        var socio2 = CrearSocioMock(40123457);
        _usuarioRepo.ObtenerPorDniAsync(40123456).Returns(socio1);
        _usuarioRepo.ObtenerPorDniAsync(40123457).Returns(socio2);
        _reservaRepo.ObtenerCanchaPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(CrearCanchaMock(1, 2000.0));
        _reservaRepo.ExisteSuperposicionAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _stockRepo.ValidarStockDisponibleAsync(1, 2, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new CrearReservaHandler(_reservaRepo, _usuarioRepo, _stockRepo);
        var ahora = DateTime.Now.AddDays(2);
        var command = new CrearReservaCommand(
            CanchaId: 1,
            fecha_reserva: ahora.Date,
            HoraInicio: ahora.Date.AddHours(15),
            HoraFin: ahora.Date.AddHours(16), // 1 hora -> 1 * 2000 = 2000
            SocioDni: 40123456,
            JugadoresDni: new List<int> { 40123456, 40123457 },
            EquipamientosSolicitados: new List<EquipamientoSolicitadoDto>
            {
                new EquipamientoSolicitadoDto(EquipamientoId: 1, Cantidad: 2)
            }
        );

        // Act
        var respuesta = await handler.Handle(command);

        // Assert
        Assert.NotNull(respuesta);
        Assert.Equal(2000f, respuesta.ImporteTotal); // 1 hora * $2000
        Assert.Equal(1000f, respuesta.MontoSena); // 50% de 2000 (RD-06)
        Assert.Equal("Pendiente", respuesta.Estado); // RD-09
        await _stockRepo.Received(1).DescontarStockAsync(1, 2, Arg.Any<CancellationToken>());
        await _reservaRepo.Received(1).AgregarReservaAsync(Arg.Any<Reserva>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelarReserva_ConMasDeSeisHoras_AplicaReembolsoYRestituyeStock()
    {
        // Arrange
        var reserva = new Reserva
        {
            Id = 10,
            HoraInicio = DateTime.Now.AddHours(12), // Más de 6 horas
            HoraFin = DateTime.Now.AddHours(13),
            estado = EstadoReserva.Pendiente,
            DetallesStockReserva = new List<Detalle_stock_reserva>
            {
                new Detalle_stock_reserva { StockId = 2, cantidad = 3 }
            }
        };

        _reservaRepo.ObtenerPorIdAsync(10, Arg.Any<CancellationToken>()).Returns(reserva);
        var handler = new CancelarReservaHandler(_reservaRepo, _stockRepo);

        // Act
        var resultado = await handler.Handle(10);

        // Assert
        Assert.True(resultado.AplicaReembolso);
        Assert.Contains("reembolso correspondiente", resultado.Mensaje);
        Assert.Equal(EstadoReserva.Cancelada.ToString(), resultado.Estado);
        await _stockRepo.Received(1).IncrementarStockAsync(2, 3, Arg.Any<CancellationToken>());
        await _reservaRepo.Received(1).ActualizarReservaAsync(reserva, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelarReserva_ConMenosDeSeisHoras_RetieneSena()
    {
        // Arrange
        var reserva = new Reserva
        {
            Id = 11,
            HoraInicio = DateTime.Now.AddHours(2), // Menos de 6 horas
            HoraFin = DateTime.Now.AddHours(3),
            estado = EstadoReserva.Pendiente,
            DetallesStockReserva = new List<Detalle_stock_reserva>()
        };

        _reservaRepo.ObtenerPorIdAsync(11, Arg.Any<CancellationToken>()).Returns(reserva);
        var handler = new CancelarReservaHandler(_reservaRepo, _stockRepo);

        // Act
        var resultado = await handler.Handle(11);

        // Assert
        Assert.False(resultado.AplicaReembolso);
        Assert.Contains("retendrá la seña", resultado.Mensaje);
        Assert.Equal(EstadoReserva.Cancelada.ToString(), resultado.Estado);
    }

    [Fact]
    public async Task ConfirmarReserva_SinPagoSuficiente_LanzaDomainException()
    {
        // Arrange
        var reserva = new Reserva
        {
            Id = 12,
            importe = 2000f,
            estado = EstadoReserva.Pendiente,
            Pagos = new List<Pago>() // Sin pagos
        };

        _reservaRepo.ObtenerPorIdAsync(12, Arg.Any<CancellationToken>()).Returns(reserva);
        var handler = new ConfirmarReservaHandler(_reservaRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(12));
        Assert.Contains("No se encuentra registrado y validado el pago del 50%", ex.Message);
    }

    [Fact]
    public async Task ConfirmarReserva_ConPagoValidadoDeAlMenosCincuentaPorCiento_ConfirmaReserva()
    {
        // Arrange
        var reserva = new Reserva
        {
            Id = 13,
            importe = 2000f,
            estado = EstadoReserva.Pendiente,
            Socios = new List<Socio> { CrearSocioMock(40123456) },
            Pagos = new List<Pago>
            {
                new Pago
                {
                    importe = 1000f, // 50% de 2000
                    estado = EstadoPago.Validado
                }
            }
        };

        _reservaRepo.ObtenerPorIdAsync(13, Arg.Any<CancellationToken>()).Returns(reserva);
        var handler = new ConfirmarReservaHandler(_reservaRepo);

        // Act
        var respuesta = await handler.Handle(13);

        // Assert
        Assert.NotNull(respuesta);
        Assert.Equal(EstadoReserva.Confirmada.ToString(), respuesta.Estado);
        Assert.Equal(EstadoReserva.Confirmada, reserva.estado);
        await _reservaRepo.Received(1).ActualizarReservaAsync(reserva, Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task CrearReserva_SinStockDeEquipamiento_LanzaDomainException()
    {
        // Arrange
        var socio1 = CrearSocioMock(40123456);
        var socio2 = CrearSocioMock(40123457);
        _usuarioRepo.ObtenerPorDniAsync(40123456).Returns(socio1);
        _usuarioRepo.ObtenerPorDniAsync(40123457).Returns(socio2);
        _reservaRepo.ObtenerCanchaPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(CrearCanchaMock(1, 2000.0));
        _reservaRepo.ExisteSuperposicionAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Mock sin stock de red (Id: 3)
        _stockRepo.ListarTodosAsync(Arg.Any<CancellationToken>()).Returns(new List<Stock>
        {
            new Stock { Id = 1, tipo_elemento = "Pelotas de tenis", cantidad_disponible = 10 },
            new Stock { Id = 2, tipo_elemento = "Raquetas", cantidad_disponible = 10 },
            new Stock { Id = 3, tipo_elemento = "Redes", cantidad_disponible = 0 }
        });
        _stockRepo.ValidarStockDisponibleAsync(1, 1, Arg.Any<CancellationToken>()).Returns(true);
        _stockRepo.ValidarStockDisponibleAsync(2, 2, Arg.Any<CancellationToken>()).Returns(true);
        _stockRepo.ValidarStockDisponibleAsync(3, 1, Arg.Any<CancellationToken>()).Returns(false); // Sin stock
        _stockRepo.ObtenerPorIdAsync(3, Arg.Any<CancellationToken>()).Returns(new Stock { Id = 3, tipo_elemento = "Redes" });

        var handler = new CrearReservaHandler(_reservaRepo, _usuarioRepo, _stockRepo);
        var ahora = DateTime.Now.AddDays(2);
        var command = new CrearReservaCommand(
            CanchaId: 1,
            fecha_reserva: ahora.Date,
            HoraInicio: ahora.Date.AddHours(14),
            HoraFin: ahora.Date.AddHours(15),
            SocioDni: 40123456,
            JugadoresDni: new List<int> { 40123456, 40123457 }
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains("No hay stock suficiente", ex.Message);
    }

    [Fact]
    public async Task CrearReserva_ConCuatroJugadores_DescuentaCuatroRaquetasTuboYRed()
    {
        // Arrange
        var socio1 = CrearSocioMock(40123456);
        var socio2 = CrearSocioMock(40123457);
        var socio3 = CrearSocioMock(40123458);
        var socio4 = CrearSocioMock(40123459);
        _usuarioRepo.ObtenerPorDniAsync(40123456).Returns(socio1);
        _usuarioRepo.ObtenerPorDniAsync(40123457).Returns(socio2);
        _usuarioRepo.ObtenerPorDniAsync(40123458).Returns(socio3);
        _usuarioRepo.ObtenerPorDniAsync(40123459).Returns(socio4);
        _reservaRepo.ObtenerCanchaPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(CrearCanchaMock(1, 2000.0));
        _reservaRepo.ExisteSuperposicionAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _stockRepo.ListarTodosAsync(Arg.Any<CancellationToken>()).Returns(new List<Stock>
        {
            new Stock { Id = 1, tipo_elemento = "Pelotas de tenis", cantidad_disponible = 20 },
            new Stock { Id = 2, tipo_elemento = "Raquetas", cantidad_disponible = 10 },
            new Stock { Id = 3, tipo_elemento = "Redes", cantidad_disponible = 5 }
        });
        _stockRepo.ValidarStockDisponibleAsync(1, 1, Arg.Any<CancellationToken>()).Returns(true);
        _stockRepo.ValidarStockDisponibleAsync(2, 4, Arg.Any<CancellationToken>()).Returns(true); // 4 raquetas para 4 jugadores
        _stockRepo.ValidarStockDisponibleAsync(3, 1, Arg.Any<CancellationToken>()).Returns(true); // 1 red

        var handler = new CrearReservaHandler(_reservaRepo, _usuarioRepo, _stockRepo);
        var ahora = DateTime.Now.AddDays(2);
        var command = new CrearReservaCommand(
            CanchaId: 1,
            fecha_reserva: ahora.Date,
            HoraInicio: ahora.Date.AddHours(14),
            HoraFin: ahora.Date.AddHours(15),
            SocioDni: 40123456,
            JugadoresDni: new List<int> { 40123456, 40123457, 40123458, 40123459 }
        );

        // Act
        var res = await handler.Handle(command);

        // Assert
        Assert.NotNull(res);
        Assert.Equal(4, res.JugadoresDni.Count);
        // Debe descontar: 1 tubo de pelotas, 4 raquetas, 1 red
        await _stockRepo.Received(1).DescontarStockAsync(1, 1, Arg.Any<CancellationToken>());
        await _stockRepo.Received(1).DescontarStockAsync(2, 4, Arg.Any<CancellationToken>());
        await _stockRepo.Received(1).DescontarStockAsync(3, 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FinalizarReserva_ReintegraEquipamientoAlStock()
    {
        // Arrange
        var reserva = new Reserva
        {
            Id = 20,
            estado = EstadoReserva.Confirmada,
            HoraInicio = DateTime.Now.AddHours(-2),
            HoraFin = DateTime.Now.AddHours(-1),
            importe = 3000f,
            DetallesStockReserva = new List<Detalle_stock_reserva>
            {
                new Detalle_stock_reserva { StockId = 1, cantidad = 1 }, // 1 tubo
                new Detalle_stock_reserva { StockId = 2, cantidad = 2 }, // 2 raquetas
                new Detalle_stock_reserva { StockId = 3, cantidad = 1 }  // 1 red
            }
        };

        _reservaRepo.ObtenerPorIdAsync(20, Arg.Any<CancellationToken>()).Returns(reserva);
        var handler = new FinalizarReservaHandler(_reservaRepo, _stockRepo);

        // Act
        var resultado = await handler.Handle(20);

        // Assert
        Assert.Equal(EstadoReserva.Finalizada.ToString(), resultado.Estado);
        await _stockRepo.Received(1).IncrementarStockAsync(1, 1, Arg.Any<CancellationToken>());
        await _stockRepo.Received(1).IncrementarStockAsync(2, 2, Arg.Any<CancellationToken>());
        await _stockRepo.Received(1).IncrementarStockAsync(3, 1, Arg.Any<CancellationToken>());
        await _reservaRepo.Received(1).ActualizarReservaAsync(reserva, Arg.Any<CancellationToken>());
    }
}
