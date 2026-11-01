using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Usuarios.ActivarUsuario;

public record ActivarUsuarioCommand(int Dni);

public class ActivarUsuarioCommandHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ActivarUsuarioCommandHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task Handle(ActivarUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        var persona = await _usuarioRepository.ObtenerPorDniAsync(command.Dni);
        if (persona is null)
        {
            throw new DomainException($"No se encontró ningún usuario con DNI {command.Dni} para reactivar.");
        }

        persona.Activo = true;
        if (persona is Socio socio)
        {
            socio.estadoMembresia = EstadoMembresia.Activa;
        }

        await _usuarioRepository.ActualizarAsync(persona, cancellationToken);
    }
}
