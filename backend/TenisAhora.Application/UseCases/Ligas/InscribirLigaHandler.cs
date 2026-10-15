using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Ligas;

public class InscribirLigaHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public InscribirLigaHandler(
        ICompetenciaRepository competenciaRepository,
        IUsuarioRepository usuarioRepository)
    {
        _competenciaRepository = competenciaRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<InscriptoDto> Handle(int ligaId, InscribirLigaCommand command, CancellationToken cancellationToken = default)
    {
        var liga = await _competenciaRepository.ObtenerLigaPorIdAsync(ligaId);
        if (liga is null || !liga.Activo)
        {
            throw new DomainException($"La liga con ID {ligaId} no existe o no se encuentra activa.");
        }

        var persona = await _usuarioRepository.ObtenerPorDniAsync(command.SocioDni);
        if (persona is null)
        {
            throw new DomainException($"No se encontró al usuario con DNI {command.SocioDni}.");
        }

        if (persona is not Socio socio)
        {
            throw new DomainException($"El usuario con DNI {command.SocioDni} no es un Socio habilitado para competir en la liga.");
        }

        var yaInscripto = await _competenciaRepository.EstaInscriptoAsync(ligaId, socio.Id);
        if (yaInscripto)
        {
            throw new DomainException($"El socio {socio.Nombre} {socio.Apellido} ya se encuentra inscripto en esta liga.");
        }

        var numeroParticipante = liga.Inscripciones.Count + 1;

        var inscripcion = new Inscripcion_competencia
        {
            CompetenciaId = liga.Id,
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
