using System;
using System.ComponentModel.DataAnnotations;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Usuarios.CrearUsuario;

public record CrearUsuarioCommand(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
    string Nombre,

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios")]
    string Apellido,

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [Range(1000000, 99999999, ErrorMessage = "El DNI debe tener un formato válido (entre 7 y 8 dígitos)")]
    int Dni,

    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    string Email,

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(@"^[0-9]{8,15}$", ErrorMessage = "El teléfono debe contener entre 8 y 15 dígitos numéricos.")]
    string Telefono,

    [Required(ErrorMessage = "La dirección es obligatoria")]
    string Direccion,

    int CodigoPostal,

    DateTime? FechaDeNacimiento = null,

    string? Password = null,

    Rol Rol = Rol.Socio,

    string? TituloHabilitante = null,
    string? EnteEmisor = null,
    [Range(0, 60, ErrorMessage = "La antigüedad debe estar entre 0 y 60 años")]
    int? Antiguedad = null,
    string? Especialidad = null
);
