using Microsoft.EntityFrameworkCore;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Cada "DbSet" representará una TABLA en tu base de datos:
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Socio> Socios { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Instructor> Instructores { get; set; }
        public DbSet<Profesor> Profesores { get; set; }
        public DbSet<Entrenador> Entrenadores { get; set; }
        public DbSet<Cancha> Canchas { get; set; }
        public DbSet<TipoDeCancha> TiposDeCancha { get; set; }
        public DbSet<Disponibilidad> Disponibilidades { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Detalle_stock_reserva> DetallesStockReserva { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Descuento> Descuentos { get; set; }
        public DbSet<Competencia> Competencias { get; set; }
        public DbSet<Torneo> Torneos { get; set; }
        public DbSet<Liga> Ligas { get; set; }
        public DbSet<Reglamento> Reglamentos { get; set; }
        public DbSet<Partido> Partidos { get; set; }
        public DbSet<Equipo> Equipos { get; set; }

        public DbSet<Actividad> Actividades { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<Inscripcion_actividad> InscripcionesActividad { get; set; }
        public DbSet<Inscripcion_competencia> InscripcionesCompetencia { get; set; }

        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Recibo> Recibos { get; set; }

        public DbSet<Reporte> Reportes { get; set; }
        public DbSet<CertificacionDeportiva> CertificacionesDeportivas { get; set; }


        // configurar las reglas de las tablas (claves foráneas, tipos, etc.):
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Acá aplicamos las configuraciones de cada entidad
        }
    }
}