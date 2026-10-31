using System;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Disponibilidad;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Disponibilidad.Commands;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;
using DisponibilidadEntity = TenisAhora.Domain.Entities.Disponibilidad;

namespace TenisAhora.Application.UseCases.Disponibilidad.Handlers;

public class CrearDisponibilidadHandler : ICrearDisponibilidadHandler
{
    private readonly IDisponibilidadRepository _repository;
    private readonly ICanchaRepository _canchaRepository;

    public CrearDisponibilidadHandler(
        IDisponibilidadRepository repository,
        ICanchaRepository canchaRepository)
    {
        _repository = repository;
        _canchaRepository = canchaRepository;
    }

    public async Task<DisponibilidadResponseDto> Handle(CrearDisponibilidadCommand command, CancellationToken cancellationToken = default)
    {
        // Validaciones de negocio
        if (command.FechaHoraFin <= command.FechaHoraInicio)
        {
            throw new DomainException("La fecha/hora de fin debe ser posterior a la de inicio.");
        }

        // No permitir disponibilidades en el pasado
        if (command.FechaHoraInicio < DateTime.Now)
        {
            throw new DomainException("No se puede registrar disponibilidad en una fecha u hora pasada.");
        }

        // Verificar que la cancha existe
        var cancha = await _canchaRepository.ObtenerPorIdAsync(command.CanchaId);
        if (cancha is null)
        {
            throw new DomainException($"No se encontró una cancha con ID {command.CanchaId}.");
        }

        // Verificar solapamiento con disponibilidades existentes de la misma cancha
        var haySolapamiento = await _repository.ExisteSolapamientoAsync(
            command.CanchaId, command.FechaHoraInicio, command.FechaHoraFin, cancellationToken);

        if (haySolapamiento)
        {
            throw new DomainException("Ya existe una disponibilidad registrada que se solapa con el período solicitado para esta cancha.");
        }

        var disponibilidad = new DisponibilidadEntity
        {
            CanchaId = command.CanchaId,
            FechaHoraInicio = command.FechaHoraInicio,
            FechaHoraFin = command.FechaHoraFin,
            estado = command.Estado
        };

        await _repository.CrearAsync(disponibilidad, cancellationToken);

        return new DisponibilidadResponseDto(
            disponibilidad.Id,
            disponibilidad.CanchaId,
            cancha.TipoDeCancha?.superficie.ToString() ?? string.Empty,
            disponibilidad.FechaHoraInicio,
            disponibilidad.FechaHoraFin,
            disponibilidad.estado,
            disponibilidad.estado.ToString()
        );
    }
}