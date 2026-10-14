using NSubstitute;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Usuarios.CrearUsuario;
using TenisAhora.Application.UseCases.Usuarios.EliminarUsuario;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.UnitTests;

public class UsuariosTests
{
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    [Fact]
    public async Task CrearUsuario_ConDniDuplicado_LanzaDomainException_YNoPersiste()
    {
        // Arrange: el DNI ya existe en la base de datos
        var dniExistente = 43249050;
        _usuarioRepository.ExisteDniAsync(dniExistente).Returns(true);

        var handler = new CrearUsuarioCommandHandler(_usuarioRepository, _passwordHasher);
        var command = new CrearUsuarioCommand(
            Nombre: "Franco",
            Apellido: "Guanes",
            Dni: dniExistente,
            Email: "franco@test.com",
            Telefono: "1122334455",
            Direccion: "Calle Falsa 123",
            CodigoPostal: 1888,
            FechaDeNacimiento: new DateTime(2000, 1, 1),
            Password: null,
            Rol: Rol.Socio
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains(dniExistente.ToString(), ex.Message);

        // Verificamos que NUNCA se haya agregado al repositorio
        await _usuarioRepository.DidNotReceive().AgregarAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CrearUsuario_ConDatosValidos_GuardaUsuarioYRetornaDto()
    {
        // Arrange: el DNI no existe
        var nuevoDni = 40112233;
        _usuarioRepository.ExisteDniAsync(nuevoDni).Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed_secret_password");

        var handler = new CrearUsuarioCommandHandler(_usuarioRepository, _passwordHasher);
        var command = new CrearUsuarioCommand(
            Nombre: "Carlos",
            Apellido: "Alcaraz",
            Dni: nuevoDni,
            Email: "carlos@tennis.com",
            Telefono: "1199887766",
            Direccion: "Avenida Siempre Viva 742",
            CodigoPostal: 1800,
            FechaDeNacimiento: new DateTime(2003, 5, 5),
            Password: "PasswordSegura123!",
            Rol: Rol.Socio
        );

        // Act
        var resultado = await handler.Handle(command);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Carlos", resultado.Nombre);
        Assert.Equal(nuevoDni, resultado.Dni);
        Assert.True(resultado.Activo);
        Assert.Equal(EstadoMembresia.Activa, resultado.EstadoMembresia);

        // Verificamos que se haya invocado AgregarAsync en el repositorio
        await _usuarioRepository.Received(1).AgregarAsync(
            Arg.Is<Persona>(p => p.Dni == nuevoDni && p.Activo),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task EliminarUsuario_UsuarioExistente_AplicaBajaLogica()
    {
        // Arrange
        var dni = 43249050;
        var usuario = new Socio { Id = 10, Dni = dni, Nombre = "Franco", Activo = true };
        _usuarioRepository.ObtenerPorDniAsync(dni).Returns(usuario);

        var handler = new EliminarUsuarioCommandHandler(_usuarioRepository);

        // Act
        await handler.Handle(new EliminarUsuarioCommand(dni));

        // Assert
        Assert.False(usuario.Activo); // Se aplicó la baja lógica
        await _usuarioRepository.Received(1).ActualizarAsync(usuario, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegistrarSocio_ConEmailDuplicado_LanzaDomainException()
    {
        // Arrange
        var dni = 45000111;
        var email = "socio@tenisahora.com";
        _usuarioRepository.ExisteDniAsync(dni).Returns(false);
        _usuarioRepository.ExisteEmailAsync(email).Returns(true);

        var jwtProvider = Substitute.For<IJwtProvider>();
        var handler = new TenisAhora.Application.UseCases.Auth.Registro.RegistrarSocioCommandHandler(
            _usuarioRepository, _passwordHasher, jwtProvider);

        var command = new TenisAhora.Application.UseCases.Auth.Registro.RegistrarSocioCommand(
            Nombre: "Lucia",
            Apellido: "Gomez",
            Dni: dni,
            Email: email,
            Telefono: "1144556677",
            Direccion: "Belgrano 450",
            CodigoPostal: 1888,
            FechaDeNacimiento: new DateTime(1998, 2, 20),
            Password: "Password123!"
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        Assert.Contains(email, ex.Message);
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_RetornaTokenYDatos()
    {
        // Arrange
        var email = "admin@tenisahora.com";
        var password = "PasswordValida123!";
        var hash = "hash_valido";
        var usuario = new Persona
        {
            Id = 1,
            Email = email,
            Nombre = "Admin",
            Apellido = "General",
            PasswordHash = hash,
            Rol = Rol.Administrador,
            Activo = true
        };

        _usuarioRepository.ObtenerPorEmailAsync(email).Returns(usuario);
        _passwordHasher.Verify(password, hash).Returns(true);

        var jwtProvider = Substitute.For<IJwtProvider>();
        jwtProvider.GenerateToken(usuario).Returns("token_jwt_valido");

        var handler = new TenisAhora.Application.UseCases.Auth.Login.LoginCommandHandler(
            _usuarioRepository, _passwordHasher, jwtProvider);

        var request = new TenisAhora.Application.DTOs.Auth.LoginRequest(email, password);

        // Act
        var response = await handler.Handle(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("token_jwt_valido", response.Token);
        Assert.Equal(usuario.Id, response.UsuarioId);
        Assert.Equal(usuario.Email, response.Email);
        Assert.Equal(Rol.Administrador.ToString(), response.Rol);
    }
}
