using Microsoft.EntityFrameworkCore;
using TenisAhora.Infrastructure.Persistence;
using Xunit;

namespace TenisAhora.UnitTests;

public class AppDbContextTests
{
    [Fact]
    public void AppDbContext_ConfiguracionDelModelo_EsValida()
    {
        // 1. Arrange (Preparar)
        // Le indicamos que use el proveedor de SqlServer con una cadena ficticia
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=fake_server;Database=fake_db;Trusted_Connection=True;")
            .Options;

        using var context = new AppDbContext(options);

        // 2. Act (Actuar)
        // Al acceder a 'context.Model', EF Core se ve obligado a ejecutar OnModelCreating y validar todo
        var modelo = context.Model;

        // 3. Assert (Verificar)
        // Si llegó a esta línea sin lanzar ninguna excepción, el modelo es válido
        Assert.NotNull(modelo);
    }
}