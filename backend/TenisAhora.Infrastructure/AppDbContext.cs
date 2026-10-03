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
            
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Busca la propiedad "Id" en la clase, incluso si es privada (NonPublic)
                var prop = entity.ClrType.GetProperty("Id", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic);

                if (prop != null)
                {
                    modelBuilder.Entity(entity.ClrType).HasKey("Id");
                }
            }

            //certificacionDeportiva - instructor hecho
            modelBuilder.Entity<CertificacionDeportiva>()
                .HasOne(c => c.instructor)
                .WithOne(i => i.certificacionDeportiva)
                .HasForeignKey<CertificacionDeportiva>(c => c.InstructorId);

            // Actividad - Profesor hecho
            modelBuilder.Entity<Actividad>()
                .HasOne(a => a.Profesor)
                .WithMany(p => p.Actividades)
                .HasForeignKey(a => a.ProfesorId);

            //  Actividad - Entrenador hecho
            modelBuilder.Entity<Actividad>()
                .HasOne(a => a.Entrenador)
                .WithMany(e => e.Actividades)
                .HasForeignKey(a => a.EntrenadorId);


            // Actividad - Asistencia hecho
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Actividad)
                .WithMany(a => a.Asistencias)
                .HasForeignKey(a => a.ActividadId);


            // Asistencia - Socio hecho
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Socio)
                .WithMany(s => s.Asistencias)
                .HasForeignKey(a => a.SocioId);




            // Socio - Equipo hecho
            modelBuilder.Entity<Socio>()
                .HasMany(s => s.Equipos)
                .WithMany(e => e.Socios);


            // Socio - Inscripcion hecho
            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.Socio)
                .WithMany(s => s.Inscripciones)
                .HasForeignKey(i => i.SocioId);


            // Socio - Reserva hecho
            modelBuilder.Entity<Socio>()
                .HasMany(s => s.Reservas)
                .WithMany(r => r.Socios);


            // Equipo - Inscripcion_competencia hecho
            modelBuilder.Entity<Equipo>()
                .HasMany(e => e.InscripcionesCompetencia)
                .WithMany(ic => ic.Equipos);



            // Actividad - Inscripcion_actividad hecho
            modelBuilder.Entity<Inscripcion_actividad>()
                .HasOne(i => i.Actividad)
                .WithMany(a => a.InscripcionesActividad)
                .HasForeignKey(i => i.ActividadId);


            // Inscripcion_competencia - Competencia hecho
            modelBuilder.Entity<Inscripcion_competencia>()
                .HasOne(ic => ic.Competencia)
                .WithMany(c => c.Inscripciones)
                .HasForeignKey(ic => ic.CompetenciaId);



            // Reserva - Pago hecho 
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Reserva)
                .WithMany(r => r.Pagos)
                .HasForeignKey(p => p.ReservaId);



            // Inscripcion - Pago hecho
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Inscripcion)
                .WithMany(i => i.Pagos)
                .HasForeignKey(p => p.InscripcionId);


            // Pago - Recibo hecho
            modelBuilder.Entity<Recibo>()
                .HasOne(r => r.Pago)
                .WithOne(p => p.Recibo)
                .HasForeignKey<Recibo>(r => r.PagoId);


            // Pago - Descuento hecho
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Descuento)
                .WithMany(d => d.Pagos)
                .HasForeignKey(p => p.DescuentoId);



            // Reserva - Detalle_stock_reserva hecho
            modelBuilder.Entity<Detalle_stock_reserva>()
                .HasOne(d => d.Reserva)
                .WithMany(r => r.DetallesStockReserva)
                .HasForeignKey(d => d.ReservaId);



            //detalle_stock_reserva - stock
            modelBuilder.Entity<Detalle_stock_reserva>()
                .HasOne(d => d.Stock)
                .WithMany(s => s.DetallesStockReserva)
                .HasForeignKey(d => d.StockId);



            //reserva - cancha hecho
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Cancha)
                .WithMany(c => c.Reservas)
                 .HasForeignKey(r => r.CanchaId);



            //cancha - disponibilidad hecho
            modelBuilder.Entity<Disponibilidad>()
                  .HasOne(d => d.Cancha)
                  .WithMany(c => c.Disponibilidades)
                  .HasForeignKey(d => d.CanchaId);



            //cancha - tipoDeCancha hecho
            modelBuilder.Entity<Cancha>()
                 .HasOne(c => c.TipoDeCancha)
                 .WithMany(t => t.Canchas)
                 .HasForeignKey(c => c.TipoDeCanchaId);



            //cancha - partido hecho
            modelBuilder.Entity<Partido>()
                .HasOne(p => p.Cancha)
                .WithMany(c => c.Partidos)
                 .HasForeignKey(p => p.CanchaId);



            //administrador- reporte hecho
            modelBuilder.Entity<Reporte>()
              .HasOne(r => r.Administrador)
              .WithMany(a => a.Reportes)
             .HasForeignKey(r => r.AdministradorId);


            //administrador - instuctor hecho
            modelBuilder.Entity<Instructor>()
                 .HasOne(i => i.Administrador)
                 .WithMany(a => a.Instructores)
                 .HasForeignKey(i => i.AdministradorId);



            //reglamento- competencia hecho
            modelBuilder.Entity<Competencia>()
                .HasOne(c => c.Reglamento)
                .WithMany(r => r.Competencias)
                .HasForeignKey(c => c.ReglamentoId);




            //competencia - partido hecho
            modelBuilder.Entity<Partido>()
                 .HasOne(p => p.Competencia)
                 .WithMany(c => c.Partidos)
                 .HasForeignKey(p => p.CompetenciaId);

            //administrador - descuento hecho
            modelBuilder.Entity<Descuento>()
                 .HasOne(d => d.Administrador)
                 .WithMany(a => a.Descuentos)
                 .HasForeignKey(d => d.AdministradorId);





        }
    }
}