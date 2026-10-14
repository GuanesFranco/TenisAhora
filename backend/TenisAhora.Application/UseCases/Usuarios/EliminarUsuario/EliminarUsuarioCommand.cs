using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Usuarios.EliminarUsuario;

public record EliminarUsuarioCommand(int Dni);

public class EliminarUsuarioCommandHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public EliminarUsuarioCommandHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task Handle(EliminarUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        var persona = await _usuarioRepository.ObtenerPorDniAsync(command.Dni);
        if (persona is null)
        {
            throw new DomainException($"No se encontró ningún usuario con DNI {command.Dni} para dar de baja.");
        }

        // Baja lógica según lineamientos de seguridad e integridad del modelo
        persona.Activo = false;
        await _usuarioRepository.ActualizarAsync(persona, cancellationToken);
    }
}
