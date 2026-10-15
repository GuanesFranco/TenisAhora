using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Torneos;

public class InscribirTorneoHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public InscribirTorneoHandler(
        ICompetenciaRepository competenciaRepository,
        IUsuarioRepository usuarioRepository)
    {
        _competenciaRepository = competenciaRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<InscriptoDto> Handle(int torneoId, InscribirTorneoCommand command, CancellationToken cancellationToken = default)
    {
        var torneo = await _competenciaRepository.ObtenerTorneoPorIdAsync(torneoId);
        if (torneo is null || !torneo.Activo)
        {
            throw new DomainException($"El torneo con ID {torneoId} no existe o no se encuentra activo.");
        }

        var persona = await _usuarioRepository.ObtenerPorDniAsync(command.SocioDni);
        if (persona is null)
        {
            throw new DomainException($"No se encontró al usuario con DNI {command.SocioDni}.");
        }

        if (persona is not Socio socio)
        {
            throw new DomainException($"El usuario con DNI {command.SocioDni} no es un Socio habilitado para competir.");
        }

        var yaInscripto = await _competenciaRepository.EstaInscriptoAsync(torneoId, socio.Id);
        if (yaInscripto)
        {
            throw new DomainException($"El socio {socio.Nombre} {socio.Apellido} ya se encuentra inscripto en este torneo.");
        }

        var numeroParticipante = torneo.Inscripciones.Count + 1;

        var inscripcion = new Inscripcion_competencia
        {
            CompetenciaId = torneo.Id,
            SocioId = socio.Id,
            Fecha = DateTime.Now,
            NumeroParticipante = numeroParticipante,
            estadoInscripcion = EstadoInscripcion.Confirmada
        };

        await _competenciaRepository.InscribirCompetenciaAsync(inscripcion, cancellationToken);

        return new InscriptoDto(
            inscripcion.Id,
            socio.Id,
            $"{socio.Nombre} {socio.Apellido}",
            socio.Dni,
            numeroParticipante,
            inscripcion.Fecha
        );
    }
}
