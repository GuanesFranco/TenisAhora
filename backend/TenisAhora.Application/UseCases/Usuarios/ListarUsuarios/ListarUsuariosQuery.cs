using TenisAhora.Application.DTOs.Usuarios;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Usuarios.ListarUsuarios;

public record ListarUsuariosQuery(bool SoloActivos = true);

public class ListarUsuariosQueryHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ListarUsuariosQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<List<UsuarioResponseDto>> Handle(ListarUsuariosQuery query)
    {
        var personas = await _usuarioRepository.ListarTodosAsync(query.SoloActivos);

        return personas.Select(persona =>
        {
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
        }).ToList();
    }
}
