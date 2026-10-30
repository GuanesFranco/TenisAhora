using System;
using System.ComponentModel.DataAnnotations;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Usuarios.CrearUsuario;

public record CrearUsuarioCommand(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    string Nombre,

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
    string Apellido,

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [Range(1000000, 99999999, ErrorMessage = "El DNI debe tener un formato válido (entre 7 y 8 dígitos)")]
    int Dni,

    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    string Email,

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    string Telefono,

    [Required(ErrorMessage = "La dirección es obligatoria")]
    string Direccion,

    int CodigoPostal,

    DateTime? FechaDeNacimiento = null,

    string? Password = null,

    Rol Rol = Rol.Socio,

    string? TituloHabilitante = null,
    string? EnteEmisor = null,
    int? Antiguedad = null,
    string? Especialidad = null
);
