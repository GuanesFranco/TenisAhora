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
/// Caso de Uso: Modificar Reserva de Cancha
/// [RF - 26] El sistema debe permitir modificar reservas de canchas.
/// ● El sistema debe permitir identificar la reserva mediante su ID.
/// ● El sistema debe permitir modificar los datos de la reserva.
/// ● El sistema debe permitir guardar las modificaciones realizadas.
/// </summary>
public class ModificarReservaHandler
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IStockRepository _stockRepository;

    public ModificarReservaHandler(
        IReservaRepository reservaRepository,
        IUsuarioRepository usuarioRepository,
        IStockRepository stockRepository)
    {
        _reservaRepository = reservaRepository;
        _usuarioRepository = usuarioRepository;
        _stockRepository = stockRepository;
    }

    public async Task<ReservaResponseDto> Handle(int reservaId, ModificarReservaCommand command, CancellationToken cancellationToken = default)
    {
        // 1. [RF - 26] Identificar la reserva mediante su ID
        var reserva = await _reservaRepository.ObtenerPorIdAsync(reservaId, cancellationToken);
        if (reserva is null)
        {
            throw new DomainException($"No se encontró la reserva con ID {reservaId}.");
        }

        // No se pueden modificar reservas canceladas ni que ya hayan finalizado
        if (reserva.estado == EstadoReserva.Cancelada)
        {
            throw new DomainException("No se puede modificar una reserva cancelada.");
        }

        if (reserva.HoraFin <= DateTime.Now)
        {
            throw new DomainException("No se puede modificar una reserva cuyo horario ya ha finalizado.");
        }

        // 2. [RF - 26] Validaciones de horario y negocio
        if (command.HoraFin <= command.HoraInicio)
        {
            throw new DomainException("La hora de finalización no puede ser anterior o igual a la de inicio.");
        }

        if ((command.HoraFin - command.HoraInicio).TotalHours > 2)
        {
            throw new DomainException("La reserva modificada no puede superar las 2 horas de duración.");
        }

        if (command.HoraInicio < DateTime.Now)
        {
            throw new DomainException("No se puede reprogramar una reserva para una fecha u hora pasada.");
        }

        if (command.fecha_reserva > DateTime.Now.AddDays(30))
        {
            throw new DomainException("No se puede reprogramar con más de 30 días calendario de antelación.");
        }

        // 3. [RF - 26] Validar nómina de jugadores
        if (command.JugadoresDni == null || (command.JugadoresDni.Count != 2 && command.JugadoresDni.Count != 4))
        {
            throw new DomainException("La reserva debe contar con exactamente 2 o 4 jugadores registrados.");
        }

        if (command.JugadoresDni.Distinct().Count() != command.JugadoresDni.Count)
        {
            throw new DomainException("La nómina de jugadores contiene DNIs duplicados.");
        }

        // 4. [RF - 26] Validar disponibilidad de cancha excluyendo la reserva actual
        var haySuperposicion = await _reservaRepository.ExisteSuperposicionAsync(
            command.CanchaId, command.HoraInicio, command.HoraFin, excluirReservaId: reserva.Id, cancellationToken);

        if (haySuperposicion)
        {
            throw new DomainException("La cancha seleccionada no está disponible en el nuevo rango horario solicitado.");
        }

        // 5. [RF - 26] Gestión de equipamiento y stock
        // Restituir el equipamiento asignado anteriormente
        if (reserva.DetallesStockReserva != null && reserva.DetallesStockReserva.Any())
        {
            foreach (var detalle in reserva.DetallesStockReserva)
            {
                await _stockRepository.IncrementarStockAsync(detalle.StockId, detalle.cantidad, cancellationToken);
            }
            reserva.DetallesStockReserva.Clear();
        }

        // Validar y descontar el nuevo equipamiento solicitado
        if (command.EquipamientosSolicitados != null && command.EquipamientosSolicitados.Any())
        {
            foreach (var item in command.EquipamientosSolicitados)
            {
                var tieneStock = await _stockRepository.ValidarStockDisponibleAsync(
                    item.EquipamientoId, item.Cantidad, cancellationToken);

                if (!tieneStock)
                {
                    throw new DomainException($"No hay stock disponible suficiente para el equipamiento con ID {item.EquipamientoId}.");
                }
            }

            reserva.DetallesStockReserva ??= new List<Detalle_stock_reserva>();
            foreach (var item in command.EquipamientosSolicitados)
            {
                reserva.DetallesStockReserva.Add(new Detalle_stock_reserva
                {
                    StockId = item.EquipamientoId,
                    cantidad = item.Cantidad
                });

                await _stockRepository.DescontarStockAsync(item.EquipamientoId, item.Cantidad, cancellationToken);
            }
        }

        // 6. [RF - 26] Actualizar socios participantes
        var nuevaListaSocios = new List<Socio>();
        foreach (var dni in command.JugadoresDni)
        {
            var persona = await _usuarioRepository.ObtenerPorDniAsync(dni);
            if (persona is Socio s)
            {
                nuevaListaSocios.Add(s);
            }
        }
        reserva.Socios = nuevaListaSocios;

        // 7. [RF - 26] Modificar los datos de la reserva y guardar
        reserva.CanchaId = command.CanchaId;
        reserva.fecha_reserva = command.fecha_reserva;
        reserva.HoraInicio = command.HoraInicio;
        reserva.HoraFin = command.HoraFin;
        reserva.importe = command.ImporteTotal;

        await _reservaRepository.ActualizarReservaAsync(reserva, cancellationToken);

        decimal montoSeña = (decimal)reserva.importe * 0.5m;
        var titularDni = reserva.Socios.FirstOrDefault()?.Dni ?? 0;

        var equipamientosResponse = reserva.DetallesStockReserva?.Select(d => new DetalleEquipamientoResponseDto(
            d.StockId,
            d.Stock?.tipo_elemento ?? string.Empty,
            d.cantidad
        )).ToList() ?? new List<DetalleEquipamientoResponseDto>();

        return new ReservaResponseDto(
            reserva.Id,
            reserva.CanchaId,
            reserva.fecha_reserva,
            reserva.HoraInicio,
            reserva.HoraFin,
            titularDni,
            command.JugadoresDni,
            reserva.importe,
            (float)montoSeña,
            reserva.estado.ToString(),
            true,
            equipamientosResponse
        );
    }
}
