using TenisAhora.Application.DTOs.Usuarios;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Usuarios.ObtenerFichaImpresion;

public record ObtenerFichaImpresionQuery(int Dni);

public class ObtenerFichaImpresionQueryHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObtenerFichaImpresionQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<FichaUsuarioDto> Handle(ObtenerFichaImpresionQuery query)
    {
        var persona = await _usuarioRepository.ObtenerPorDniAsync(query.Dni);
        if (persona is null)
        {
            throw new DomainException($"No se encontró ningún usuario con DNI {query.Dni} para generar la ficha de impresión.");
        }

        var hoy = DateTime.Today;
        var edad = hoy.Year - persona.FechaDeNacimiento.Year;
        if (persona.FechaDeNacimiento.Date > hoy.AddYears(-edad)) edad--;

        string? estadoMembresia = persona is Socio socio ? socio.estadoMembresia.ToString() : null;

        return new FichaUsuarioDto(
            persona.Id,
            $"{persona.Apellido}, {persona.Nombre}",
            persona.Dni,
            persona.Email,
            persona.Telefono,
            persona.Direccion,
            persona.CodigoPostal,
            persona.FechaDeNacimiento,
            edad,
            persona.Rol.ToString(),
            estadoMembresia,
            DateTime.Now
        );
    }
}
