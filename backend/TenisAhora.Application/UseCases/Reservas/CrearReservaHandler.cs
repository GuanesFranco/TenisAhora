using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Reservas;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Reservas;

/// <summary>
/// Caso de Uso: Registrar Reserva de Cancha
/// [RF - 24] El sistema debe permitir registrar reservas de canchas.
/// [RD - 10] Una cancha puede ser reservada para dos o cuatro personas.
/// [RD - 11] Es obligatorio registrar los datos de todos los jugadores que intervienen en el partido por seguro.
/// [RD - 14] El alquiler requiere equipamiento deportivo (red, pelotas y raquetas), impidiendo el alquiler si no hay stock.
/// [RD - 15] Cada vez que se alquila una cancha se debe descontar del stock.
/// </summary>
public class CrearReservaHandler
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IStockReservasRepository _stockRepository;

    public CrearReservaHandler(
        IReservaRepository reservaRepository,
        IUsuarioRepository usuarioRepository,
        IStockReservasRepository stockRepository)
    {
        _reservaRepository = reservaRepository;
        _usuarioRepository = usuarioRepository;
        _stockRepository = stockRepository;
    }

    public async Task<ReservaResponseDto> Handle(CrearReservaCommand command, CancellationToken cancellationToken = default)
    {
        // ----------------------------------------------------------------------------------
        // [RF - 24] Regla de Horario: El sistema debe registrar el horario y validar su coherencia
        // ----------------------------------------------------------------------------------
        if (command.HoraFin <= command.HoraInicio)
        {
            throw new DomainException("La hora de finalización no puede ser anterior o igual a la de inicio.");
        }

        // [RF - 24] El sistema no debe permitir registrar una reserva cuya duración supere las 2 horas.
        var duracionHoras = (command.HoraFin - command.HoraInicio).TotalHours;
        if (duracionHoras > 2)
        {
            throw new DomainException("La reserva no puede superar las 2 horas de duración.");
        }

        // Validación de fecha no retroactiva
        if (command.HoraInicio < DateTime.Now)
        {
            throw new DomainException("No se puede realizar una reserva en una fecha u hora pasada.");
        }

        // [RF - 24] El sistema no debe permitir registrar reservas con más de 30 días calendario de antelación.
        if (command.fecha_reserva > DateTime.Now.AddDays(30))
        {
            throw new DomainException("No se puede realizar una reserva con más de 30 días calendario de antelación.");
        }

        // ----------------------------------------------------------------------------------
        // [RF - 24] Identificar la cancha a reservar y calcular el importe según su precio por hora
        // ----------------------------------------------------------------------------------
        var cancha = await _reservaRepository.ObtenerCanchaPorIdAsync(command.CanchaId, cancellationToken);
        if (cancha is null)
        {
            throw new DomainException($"No se encontró la cancha con ID {command.CanchaId}.");
        }

        var precioPorHora = cancha.TipoDeCancha?.precio ?? 0;
        if (precioPorHora <= 0)
        {
            throw new DomainException("La cancha seleccionada no posee un precio por hora configurado válido.");
        }

        float importeCalculado = (float)(duracionHoras * precioPorHora);

        // ----------------------------------------------------------------------------------
        // [RF - 24] Registrar los datos de las personas que participarán en la reserva.
        // Regla de Negocio (RD-10 / RD-11): Modalidad tenis singles (2) o dobles (4 jugadores)
        // Obligatorio registrar todos los datos de los jugadores para el seguro deportivo
        // ----------------------------------------------------------------------------------
        if (command.JugadoresDni == null || (command.JugadoresDni.Count != 2 && command.JugadoresDni.Count != 4))
        {
            throw new DomainException("La reserva debe contar con exactamente 2 o 4 jugadores registrados por normativa de juego y seguro.");
        }

        if (command.JugadoresDni.Distinct().Count() != command.JugadoresDni.Count)
        {
            throw new DomainException("La nómina de jugadores contiene DNIs duplicados.");
        }

        if (!command.JugadoresDni.Contains(command.SocioDni))
        {
            throw new DomainException("El socio titular que realiza la reserva debe figurar en la lista de jugadores.");
        }

        // Validar titularidad y habilitación del socio titular
        var socioPrincipal = await _usuarioRepository.ObtenerPorDniAsync(command.SocioDni);
        if (socioPrincipal is null || socioPrincipal is not Socio socioTitular)
        {
            throw new DomainException($"No se encontró un socio registrado con el DNI {command.SocioDni}.");
        }

        if (!socioTitular.Activo || socioTitular.estadoMembresia != EstadoMembresia.Activa)
        {
            throw new DomainException($"El socio titular con DNI {command.SocioDni} no está habilitado o su membresía no se encuentra activa.");
        }

        // Cargar participantes socios vinculados a la reserva e incorporar para el seguro
        var listaSocios = new List<Socio>();
        foreach (var dni in command.JugadoresDni)
        {
            var persona = await _usuarioRepository.ObtenerPorDniAsync(dni);
            if (persona is Socio s)
            {
                listaSocios.Add(s);
            }
            else
            {
                var detalle = command.JugadoresDetalle?.FirstOrDefault(j => j.Dni == dni);
                var nuevoSocio = new Socio
                {
                    Dni = dni,
                    Nombre = !string.IsNullOrWhiteSpace(detalle?.Nombre) ? detalle.Nombre : $"Jugador",
                    Apellido = !string.IsNullOrWhiteSpace(detalle?.Apellido) ? detalle.Apellido : $"{dni}",
                    Telefono = !string.IsNullOrWhiteSpace(detalle?.Telefono) ? detalle.Telefono : "1100000000",
                    Direccion = !string.IsNullOrWhiteSpace(detalle?.CoberturaMedica) ? $"Seguro: {detalle.CoberturaMedica}" : "Seguro Complejo Deportivo",
                    CodigoPostal = 1888,
                    Email = $"jugador.{dni}@tenisahora.com",
                    PasswordHash = "JUGADOR_PARTICIPANTE_HASH",
                    Rol = Rol.Socio,
                    estadoMembresia = EstadoMembresia.Activa,
                    Activo = true
                };
                await _usuarioRepository.AgregarAsync(nuevoSocio, cancellationToken);
                listaSocios.Add(nuevoSocio);
            }
        }

        if (!listaSocios.Any(s => s.Dni == socioTitular.Dni))
        {
            listaSocios.Add(socioTitular);
        }

        // ----------------------------------------------------------------------------------
        // [RF - 24] El sistema no debe permitir realizar una reserva cuando no exista disponibilidad de la cancha.
        // ----------------------------------------------------------------------------------
        var haySuperposicion = await _reservaRepository.ExisteSuperposicionAsync(
            command.CanchaId, command.HoraInicio, command.HoraFin, cancellationToken);

        if (haySuperposicion)
        {
            throw new DomainException("La cancha seleccionada no se encuentra disponible en la franja horaria solicitada.");
        }

        // ----------------------------------------------------------------------------------
        // [RF - 24 / RD-14 / RD-15] Asignación y control estricto de equipamiento deportivo:
        // Todo alquiler requiere: 1 Red de tenis, 1 Tubo de pelotas y raquetas (2 para singles, 4 para dobles).
        // No se puede alquilar una cancha si no hay stock de algo.
        // ----------------------------------------------------------------------------------
        var equipamientosAProcesar = new List<EquipamientoSolicitadoDto>();

        if (command.EquipamientosSolicitados != null && command.EquipamientosSolicitados.Any())
        {
            equipamientosAProcesar.AddRange(command.EquipamientosSolicitados);
        }
        else
        {
            // Determinar automáticamente el equipamiento obligatorio reglamentario
            var todosLosStocks = await _stockRepository.ListarTodosAsync(cancellationToken);
            var stockPelotas = todosLosStocks.FirstOrDefault(s => s.tipo_elemento.Contains("Pelota", StringComparison.OrdinalIgnoreCase)) ?? todosLosStocks.FirstOrDefault(s => s.Id == 1);
            var stockRaquetas = todosLosStocks.FirstOrDefault(s => s.tipo_elemento.Contains("Raqueta", StringComparison.OrdinalIgnoreCase)) ?? todosLosStocks.FirstOrDefault(s => s.Id == 2);
            var stockRedes = todosLosStocks.FirstOrDefault(s => s.tipo_elemento.Contains("Red", StringComparison.OrdinalIgnoreCase)) ?? todosLosStocks.FirstOrDefault(s => s.Id == 3);

            if (stockPelotas != null)
                equipamientosAProcesar.Add(new EquipamientoSolicitadoDto(stockPelotas.Id, 1));
            if (stockRaquetas != null)
                equipamientosAProcesar.Add(new EquipamientoSolicitadoDto(stockRaquetas.Id, command.JugadoresDni.Count));
            if (stockRedes != null)
                equipamientosAProcesar.Add(new EquipamientoSolicitadoDto(stockRedes.Id, 1));
        }

        // Validación estricta: No se puede alquilar una cancha si no hay stock de algún elemento (RD-14)
        foreach (var item in equipamientosAProcesar)
        {
            var tieneStock = await _stockRepository.ValidarStockDisponibleAsync(
                item.EquipamientoId, item.Cantidad, cancellationToken);

            if (!tieneStock)
            {
                var stockItem = await _stockRepository.ObtenerPorIdAsync(item.EquipamientoId, cancellationToken);
                var nombreItem = stockItem?.tipo_elemento ?? $"ID {item.EquipamientoId}";
                throw new DomainException($"No se puede realizar la reserva: No hay stock suficiente de '{nombreItem}'. Cantidad requerida: {item.Cantidad}.");
            }
        }

        // ----------------------------------------------------------------------------------
        // [RF - 24] El sistema debe asignar un ID automático a la reserva (manejado por Identity en BD).
        // [RF - 24] El sistema no debe confirmar la reserva si no se encuentra registrado y validado el pago del 50%.
        // Por ende, la reserva nace en estado 'Pendiente' hasta que se concrete el pago de la seña.
        // ----------------------------------------------------------------------------------
        var reserva = new Reserva
        {
            CanchaId = command.CanchaId,
            fecha_reserva = command.fecha_reserva,
            HoraInicio = command.HoraInicio,
            HoraFin = command.HoraFin,
            importe = importeCalculado,
            estado = EstadoReserva.Pendiente, // Estado inicial Pendiente (RD-09)
            Socios = listaSocios
        };

        // Descontar equipamiento asignado del inventario general (RD-15)
        foreach (var item in equipamientosAProcesar)
        {
            reserva.DetallesStockReserva.Add(new Detalle_stock_reserva
            {
                StockId = item.EquipamientoId,
                cantidad = item.Cantidad
            });

            await _stockRepository.DescontarStockAsync(item.EquipamientoId, item.Cantidad, cancellationToken);
        }

        await _reservaRepository.AgregarReservaAsync(reserva, cancellationToken);

        // [RF - 24] Cálculo de la seña obligatoria del 50% requerida para posterior confirmación
        decimal montoSeña = (decimal)reserva.importe * 0.5m;

        var equipamientosResponse = reserva.DetallesStockReserva.Select(d => new DetalleEquipamientoResponseDto(
            d.StockId,
            d.Stock?.tipo_elemento ?? string.Empty,
            d.cantidad
        )).ToList();

        var jugadoresDetalleResponse = listaSocios.Select(s => new JugadorDetalleDto(
            s.Dni,
            s.Nombre,
            s.Apellido,
            s.Telefono,
            s.Direccion?.StartsWith("Seguro: ") == true ? s.Direccion.Substring(8) : "Póliza Complejo Deportivo"
        )).ToList();

        return new ReservaResponseDto(
            reserva.Id,
            reserva.CanchaId,
            reserva.fecha_reserva,
            reserva.HoraInicio,
            reserva.HoraFin,
            command.SocioDni,
            command.JugadoresDni,
            reserva.importe,
            (float)montoSeña,
            reserva.estado.ToString(),
            true,
            equipamientosResponse,
            jugadoresDetalleResponse
        );
    }
}
