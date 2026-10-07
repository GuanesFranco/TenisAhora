namespace TenisAhora.Application.DTOs.Auth;

public record AuthResponse(
    string Token,
    int UsuarioId,
    string Nombre,
    string Apellido,
    string Email,
    string Rol
);
