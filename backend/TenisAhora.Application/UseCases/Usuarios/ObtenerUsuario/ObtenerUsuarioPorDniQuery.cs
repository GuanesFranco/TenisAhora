using TenisAhora.Application.DTOs.Usuarios;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Usuarios.ObtenerUsuario;

public record ObtenerUsuarioPorDniQuery(int Dni);

public class ObtenerUsuarioPorDniQueryHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObtenerUsuarioPorDniQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<UsuarioResponseDto> Handle(ObtenerUsuarioPorDniQuery query)
    {
        var persona = await _usuarioRepository.ObtenerPorDniAsync(query.Dni);
        if (persona is null)
        {
            throw new DomainException($"No se encontró ningún usuario con DNI {query.Dni}.");
        }

        EstadoMembresia? membresia = persona is Socio socio ? socio.estadoMembresia : null;

        return new UsuarioResponseDto(
            persona.Id,
            persona.Nombre,
            persona.Apellido,
            persona.Dni,
            persona.Email,
            persona.Telefono,
            persona.Direccion,
            persona.CodigoPostal,
            persona.FechaDeNacimiento,
            persona.Rol,
            persona.Activo,
            membresia
        );
    }
}
