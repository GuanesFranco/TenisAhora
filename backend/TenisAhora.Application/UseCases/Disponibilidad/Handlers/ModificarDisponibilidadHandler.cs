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

public class ModificarDisponibilidadHandler : IModificarDisponibilidadHandler
{
    private readonly IDisponibilidadRepository _repository;
    private readonly ICanchaRepository _canchaRepository;

    public ModificarDisponibilidadHandler(
        IDisponibilidadRepository repository,
        ICanchaRepository canchaRepository)
    {
        _repository = repository;
        _canchaRepository = canchaRepository;
    }

    public async Task<DisponibilidadResponseDto?> Handle(int id, ModificarDisponibilidadCommand command, CancellationToken cancellationToken = default)
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

        // Buscar la disponibilidad existente
        var disponibilidadExistente = await _repository.ObtenerPorIdAsync(id, cancellationToken);
        if (disponibilidadExistente is null)
        {
            return null;
        }

        // Verificar solapamiento con OTRAS disponibilidades de la misma cancha (excluyendo la actual)
        var haySolapamiento = await _repository.ExisteSolapamientoAsync(
            disponibilidadExistente.CanchaId, command.FechaHoraInicio, command.FechaHoraFin, id, cancellationToken);

        if (haySolapamiento)
        {
            throw new DomainException("Ya existe otra disponibilidad registrada que se solapa con el período solicitado para esta cancha.");
        }

        // Actualizar la disponibilidad
        disponibilidadExistente.FechaHoraInicio = command.FechaHoraInicio;
        disponibilidadExistente.FechaHoraFin = command.FechaHoraFin;
        disponibilidadExistente.estado = command.Estado;

        await _repository.ActualizarAsync(disponibilidadExistente, cancellationToken);

        // Obtener la cancha para el response
        var cancha = await _canchaRepository.ObtenerPorIdAsync(disponibilidadExistente.CanchaId, cancellationToken);

        return new DisponibilidadResponseDto(
            disponibilidadExistente.Id,
            disponibilidadExistente.CanchaId,
            cancha?.TipoDeCancha?.superficie.ToString() ?? string.Empty,
            disponibilidadExistente.FechaHoraInicio,
            disponibilidadExistente.FechaHoraFin,
            disponibilidadExistente.estado,
            disponibilidadExistente.estado.ToString()
        );
    }
}