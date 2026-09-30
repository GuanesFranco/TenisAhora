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


            //instructor- certificacionDeportiva
            modelBuilder.Entity<Instructor>()
                 .HasOne(i => i.certificacionDeportiva)
                 .WithOne(c => c.instructor);



            //relacion actividad-profesor
            modelBuilder.Entity<Actividad>()
                .HasOne(a => a.Profesor)
                .WithMany(p => p.Actividades);

            //relacion actividad- entrenador
            modelBuilder.Entity<Actividad>()
                .HasOne(a => a.Entrenador)
                .WithMany(e => e.Actividades);


            //socio- asistencia
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Socio)
                .WithMany(s => s.Asistencias);


            //asistencia- reporte
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Reporte)
                .WithMany(r => r.Asistencias);

            //asistencia -actividad
            modelBuilder.Entity<Asistencia>()
                 .HasOne(a => a.Actividad)
                 .WithMany(a => a.Asistencias);


            //socio - equipo
            modelBuilder.Entity<Socio>()
                 .HasMany(s => s.Equipos)
                 .WithMany(e => e.Socios);


            //socio-reserva
            modelBuilder.Entity<Socio>()
                .HasMany(s => s.Reservas)
                .WithMany(r => r.Socios);


            //socio-inscripcion
            modelBuilder.Entity<Inscripcion>()
                 .HasOne(i => i.Socio)
                 .WithMany(s => s.Inscripciones);


            //equipo-inscripcion competencia
            modelBuilder.Entity<Equipo>()
                .HasMany(e => e.InscripcionesCompetencia)
                .WithMany(ic => ic.Equipos);


            //inscripcion_competencia - competencia
            modelBuilder.Entity<Inscripcion_competencia>()
                .HasOne(ic => ic.Competencia)
                 .WithMany(c => c.Inscripciones);


            //competencia-reglamento
            modelBuilder.Entity<Reglamento>()
                .HasMany(r => r.Competencias)
                .WithOne(c => c.Reglamento);


            //inscripcionActividad - actividad
            modelBuilder.Entity<Inscripcion_actividad>()
                 .HasOne(i => i.Actividad)
                 .WithMany(a => a.InscripcionesActividad);



            //reserva- pago
            modelBuilder.Entity<Reserva>()
                 .HasMany(r => r.Pagos)
                 .WithOne(p => p.Reserva);

            

        }
    }
}