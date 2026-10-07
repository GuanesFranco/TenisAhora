using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(Persona persona);
}
