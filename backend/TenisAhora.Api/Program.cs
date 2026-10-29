using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.CertificacionesDeportivas.Handlers;
using TenisAhora.Application.UseCases.Entrenadores.Handlers;
using TenisAhora.Application.UseCases.Profesores.Handlers;
using TenisAhora.Application.UseCases.Clases.Handlers;
using TenisAhora.Application.UseCases.Entrenamientos.Handlers;
using TenisAhora.Infrastructure;
using TenisAhora.Infrastructure.Persistence;
using TenisAhora.Infrastructure.Repository;

using TenisAhora.Infrastructure.Security;
using TenisAhora.Application.UseCases.Reservas;
using TenisAhora.Application.UseCases.Productos;
using TenisAhora.Application.UseCases.Canchas.Handlers;
using TenisAhora.Application.UseCases.Reportes.Handlers;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicios de Autenticación y Seguridad
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();



// Persistencia y Repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICompetenciaRepository, CompetenciaRepository>();

// Handlers de Casos de Uso (Usuarios y Auth)
builder.Services.AddScoped<TenisAhora.Application.UseCases.Auth.Login.LoginCommandHandler>();
builder.Services.AddScoped<TenisAhora.Application.UseCases.Auth.Registro.RegistrarSocioCommandHandler>();
builder.Services.AddScoped<TenisAhora.Application.UseCases.Usuarios.CrearUsuario.CrearUsuarioCommandHandler>();
builder.Services.AddScoped<TenisAhora.Application.UseCases.Usuarios.ObtenerUsuario.ObtenerUsuarioPorDniQueryHandler>();
builder.Services.AddScoped<TenisAhora.Application.UseCases.Usuarios.ListarUsuarios.ListarUsuariosQueryHandler>();
builder.Services.AddScoped<TenisAhora.Application.UseCases.Usuarios.ModificarUsuario.ModificarUsuarioCommandHandler>();
builder.Services.AddScoped<TenisAhora.Application.UseCases.Usuarios.EliminarUsuario.EliminarUsuarioCommandHandler>();
builder.Services.AddScoped<TenisAhora.Application.UseCases.Usuarios.ObtenerFichaImpresion.ObtenerFichaImpresionQueryHandler>();
builder.Services.AddScoped<TenisAhora.Application.UseCases.Usuarios.AsignarProfesor.AsignarProfesorCommandHandler>();
builder.Services.AddScoped<TenisAhora.Application.UseCases.Usuarios.AsignarEntrenador.AsignarEntrenadorCommandHandler>();

//profesores, entrenadores y certificaciones
builder.Services.AddScoped<ICreateProfesorHandler, CreateProfesorHandler>();
builder.Services.AddScoped<IProfesorRepository, ProfesorRepository>();
builder.Services.AddScoped<IGetProfesorByIdHandler, GetProfesorByIdHandler>();
builder.Services.AddScoped<IUpdateProfesorHandler, UpdateProfesorHandler>();

builder.Services.AddScoped<ICreateCertificacionDeportivaHandler, CreateCertificacionDeportivaHandler>();

builder.Services.AddScoped<ICertificacionDeportivaRepository, CertificacionDeportivaRepository>();
builder.Services.AddScoped<IResolverCertificacionHandler, ResolverCertificacionHandler>();
builder.Services.AddScoped<IGetProfesoresHandler, GetProfesoresHandler>();
builder.Services.AddScoped<IDarDeBajaProfesorHandler, DarDeBajaProfesorHandler>();

builder.Services.AddScoped<ICreateEntrenadorHandler, CreateEntrenadorHandler>();
builder.Services.AddScoped<IGetEntrenadoresHandler, GetEntrenadoresHandler>();
builder.Services.AddScoped<IEntrenadorRepository, EntrenadorRepository>();
builder.Services.AddScoped<IDeleteEntrenadorHandler, DeleteEntrenadorHandler>();
builder.Services.AddScoped<IUpdateEntrenadorHandler, UpdateEntrenadorHandler>();

builder.Services.AddScoped<IGetEntrenadorPorIdHandler, GetEntrenadorPorIdHandler>();
builder.Services.AddScoped< IImprimirProfesorHandler,ImprimirProfesorHandler>();
builder.Services.AddScoped<IImprimirEntrenadorHandler, ImprimirEntrenadorHandler>();

//canchas
builder.Services.AddScoped< ICreateCanchaHandler, CreateCanchaHandler>();
builder.Services.AddScoped< ICanchaRepository, CanchaRepository>();
builder.Services.AddScoped<IDeleteCanchaHandler, DeleteCanchaHandler>();
builder.Services.AddScoped< IGetCanchaByIdHandler, GetCanchaByIdHandler>();
builder.Services.AddScoped< IGetAllCanchasHandler, GetAllCanchasHandler>();
builder.Services.AddScoped< IUpdateCanchaHandler, UpdateCanchaHandler>();


//reportes
builder.Services.AddScoped<ICreateReporteHandler, CreateReporteHandler>();
builder.Services.AddScoped<IReporteRepository, ReporteRepository>();


// Clases
builder.Services.AddScoped<IClaseRepository, ClaseRepository>();
builder.Services.AddScoped<ICreateClaseHandler, CreateClaseHandler>();
builder.Services.AddScoped<IGetClaseByIdHandler, GetClaseByIdHandler>();
builder.Services.AddScoped<IGetClasesHandler, GetClasesHandler>();
builder.Services.AddScoped<IUpdateClaseHandler, UpdateClaseHandler>();
builder.Services.AddScoped<IDarDeBajaClaseHandler, DarDeBajaClaseHandler>();
builder.Services.AddScoped<IInscribirAlumnoClaseHandler, InscribirAlumnoClaseHandler>();
builder.Services.AddScoped<IRegistrarAsistenciaClaseHandler, RegistrarAsistenciaClaseHandler>();
builder.Services.AddScoped<IAsignarProfesorParticularHandler, AsignarProfesorParticularHandler>();

// Entrenamientos
builder.Services.AddScoped<IEntrenamientoRepository, EntrenamientoRepository>();
builder.Services.AddScoped<ICreateEntrenamientoHandler, CreateEntrenamientoHandler>();
builder.Services.AddScoped<IGetEntrenamientoByIdHandler, GetEntrenamientoByIdHandler>();
builder.Services.AddScoped<IGetEntrenamientosHandler, GetEntrenamientosHandler>();
builder.Services.AddScoped<IUpdateEntrenamientoHandler, UpdateEntrenamientoHandler>();
builder.Services.AddScoped<IDarDeBajaEntrenamientoHandler, DarDeBajaEntrenamientoHandler>();
builder.Services.AddScoped<IInscribirAlumnoEntrenamientoHandler, InscribirAlumnoEntrenamientoHandler>();
builder.Services.AddScoped<IRegistrarAsistenciaEntrenamientoHandler, RegistrarAsistenciaEntrenamientoHandler>();
builder.Services.AddScoped<IAsignarEntrenadorParticularHandler, AsignarEntrenadorParticularHandler>();

// Reservas y Stock (RF-24, RF-25, RF-26, RF-27)
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<IStockReservasRepository, StockReservasRepository>();
builder.Services.AddScoped<CrearReservaHandler>();
builder.Services.AddScoped<ConfirmarReservaHandler>();
builder.Services.AddScoped<ModificarReservaHandler>();
builder.Services.AddScoped<CancelarReservaHandler>();
builder.Services.AddScoped<FinalizarReservaHandler>();
builder.Services.AddScoped<ObtenerReservaHandler>();
builder.Services.AddScoped<ListarReservasHandler>();

// Productos (RF-78 al RF-82)
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<CrearProductoHandler>();
builder.Services.AddScoped<ModificarProductoHandler>();
builder.Services.AddScoped<ObtenerProductoHandler>();
builder.Services.AddScoped<EliminarProductoHandler>();
builder.Services.AddScoped<ListarProductosHandler>();

// Configuración de Autenticación JWT
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? throw new InvalidOperationException("La clave secreta Jwt:SecretKey no está configurada.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

// Swagger con soporte para Bearer Token (JWT)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TenisAhora API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresá tu token JWT de esta forma: Bearer {tu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();



// MIGRACIONES Y DATA SEEDER


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();

    // Aplica las migraciones pendientes y crea la base de datos
    // si todavía no existe.
    context.Database.Migrate();

    // Carga los datos iniciales.
    DataSeeder.Seed(context);
}




// Configure the HTTP request pipeline.
app.UseMiddleware<TenisAhora.Api.Middlewares.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

// El middleware de autenticación debe ir antes de autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
