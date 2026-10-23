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
/// </summary>
public class CrearReservaHandler
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IStockRepository _stockRepository;

    public CrearReservaHandler(
        IReservaRepository reservaRepository,
        IUsuarioRepository usuarioRepository,
        IStockRepository stockRepository)
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
        if ((command.HoraFin - command.HoraInicio).TotalHours > 2)
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
        // [RF - 24] Registrar los datos de las personas que participarán en la reserva.
        // Regla de Negocio (RD-10 / RD-11): Modalidad tenis singles (2) o dobles (4 jugadores)
        // ----------------------------------------------------------------------------------
        if (command.JugadoresDni == null || (command.JugadoresDni.Count != 2 && command.JugadoresDni.Count != 4))
        {
            throw new DomainException("La reserva debe contar con exactamente 2 o 4 jugadores registrados.");
        }

        if (command.JugadoresDni.Distinct().Count() != command.JugadoresDni.Count)
        {
            throw new DomainException("La nómina de jugadores contiene DNIs duplicados.");
        }

        if (!command.JugadoresDni.Contains(command.SocioDni))
        {
            throw new DomainException("El socio titular que realiza la reserva debe figurar en la lista de jugadores.");
        }

        // Validar titularidad y habilitación del socio
        var socioPrincipal = await _usuarioRepository.ObtenerPorDniAsync(command.SocioDni);
        if (socioPrincipal is null || socioPrincipal is not Socio socioTitular)
        {
            throw new DomainException($"No se encontró un socio registrado con el DNI {command.SocioDni}.");
        }

        if (!socioTitular.Activo || socioTitular.estadoMembresia != EstadoMembresia.Activa)
        {
            throw new DomainException($"El socio titular con DNI {command.SocioDni} no está habilitado o su membresía no se encuentra activa.");
        }

        // Cargar participantes socios vinculados a la reserva
        var listaSocios = new List<Socio>();
        foreach (var dni in command.JugadoresDni)
        {
            var persona = await _usuarioRepository.ObtenerPorDniAsync(dni);
            if (persona is Socio s)
            {
                listaSocios.Add(s);
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
        // [RF - 24] El sistema debe permitir agregar equipamiento según el stock disponible.
        // [RF - 24] El sistema no debe permitir realizar una reserva cuando no exista el equipamiento requerido.
        // ----------------------------------------------------------------------------------
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
            importe = command.ImporteTotal,
            estado = EstadoReserva.Pendiente, // Estado inicial Pendiente (RD-09)
            Socios = listaSocios
        };

        // Descontar equipamiento asignado del inventario general (RD-15)
        if (command.EquipamientosSolicitados != null && command.EquipamientosSolicitados.Any())
        {
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

        await _reservaRepository.AgregarReservaAsync(reserva, cancellationToken);

        // [RF - 24] Cálculo de la seña obligatoria del 50% requerida para posterior confirmación
        decimal montoSeña = (decimal)reserva.importe * 0.5m;

        var equipamientosResponse = reserva.DetallesStockReserva.Select(d => new DetalleEquipamientoResponseDto(
            d.StockId,
            d.Stock?.tipo_elemento ?? string.Empty,
            d.cantidad
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
            equipamientosResponse
        );
    }
}