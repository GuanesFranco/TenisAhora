using Microsoft.EntityFrameworkCore;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;

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
            
           

            //certificacionDeportiva - instructor 
            modelBuilder.Entity<CertificacionDeportiva>()
                .HasOne(c => c.instructor)
                .WithOne(i => i.certificacionDeportiva)
                .HasForeignKey<CertificacionDeportiva>(c => c.InstructorId);
          


            // Actividad - Profesor 
            modelBuilder.Entity<Actividad>()
                .HasOne(a => a.Profesor)
                .WithMany(p => p.Actividades)
                .HasForeignKey(a => a.ProfesorId);
            


            //  Actividad - Entrenador 
            modelBuilder.Entity<Actividad>()
                .HasOne(a => a.Entrenador)
                .WithMany(e => e.Actividades)
                .HasForeignKey(a => a.EntrenadorId);
           


            // Actividad - Asistencia 
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Actividad)
                .WithMany(a => a.Asistencias)
                .HasForeignKey(a => a.ActividadId)
                .OnDelete(DeleteBehavior.Restrict);




            // Asistencia - Socio 
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Socio)
                .WithMany(s => s.Asistencias)
                .HasForeignKey(a => a.SocioId)
                .OnDelete(DeleteBehavior.Restrict);




            // Socio - Equipo 
            modelBuilder.Entity<Socio>()
                .HasMany(s => s.Equipos)
                .WithMany(e => e.Socios);


            // Socio - Inscripcion 
            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.Socio)
                .WithMany(s => s.Inscripciones)
                .HasForeignKey(i => i.SocioId)
                .OnDelete(DeleteBehavior.Restrict);



            // Socio - Reserva 
            modelBuilder.Entity<Socio>()
                .HasMany(s => s.Reservas)
                .WithMany(r => r.Socios);


            // Equipo - Inscripcion_competencia 
            modelBuilder.Entity<Equipo>()
                .HasMany(e => e.InscripcionesCompetencia)
                .WithMany(ic => ic.Equipos);
            



            // Actividad - Inscripcion_actividad 
            modelBuilder.Entity<Inscripcion_actividad>()
                .HasOne(i => i.Actividad)
                .WithMany(a => a.InscripcionesActividad)
                .HasForeignKey(i => i.ActividadId)
                .OnDelete(DeleteBehavior.Restrict);


            // Inscripcion_competencia - Competencia 
            modelBuilder.Entity<Inscripcion_competencia>()
                .HasOne(ic => ic.Competencia)
                .WithMany(c => c.Inscripciones)
                .HasForeignKey(ic => ic.CompetenciaId)
                .OnDelete(DeleteBehavior.Restrict);



            // Reserva - Pago 
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Reserva)
                .WithMany(r => r.Pagos)
                .HasForeignKey(p => p.ReservaId)
                .OnDelete(DeleteBehavior.Restrict);




            // Inscripcion - Pago 
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Inscripcion)
                .WithMany(i => i.Pagos)
                .HasForeignKey(p => p.InscripcionId)
                 .OnDelete(DeleteBehavior.Restrict);


            // Pago - Recibo 
            modelBuilder.Entity<Recibo>()
                .HasOne(r => r.Pago)
                .WithOne(p => p.Recibo)
                .HasForeignKey<Recibo>(r => r.PagoId);
           


            // Pago - Descuento 
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Descuento)
                .WithMany(d => d.Pagos)
                .HasForeignKey(p => p.DescuentoId)
                 .OnDelete(DeleteBehavior.Restrict);




            // Reserva - Detalle_stock_reserva 
            modelBuilder.Entity<Detalle_stock_reserva>()
                .HasOne(d => d.Reserva)
                .WithMany(r => r.DetallesStockReserva)
                .HasForeignKey(d => d.ReservaId);
            



            //detalle_stock_reserva - stock
            modelBuilder.Entity<Detalle_stock_reserva>()
                .HasOne(d => d.Stock)
                .WithMany(s => s.DetallesStockReserva)
                .HasForeignKey(d => d.StockId)
                .OnDelete(DeleteBehavior.Restrict);




            //reserva - cancha 
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Cancha)
                .WithMany(c => c.Reservas)
                 .HasForeignKey(r => r.CanchaId)
                 .OnDelete(DeleteBehavior.Restrict);




            //cancha - disponibilidad 
            modelBuilder.Entity<Disponibilidad>()
                  .HasOne(d => d.Cancha)
                  .WithMany(c => c.Disponibilidades)
                  .HasForeignKey(d => d.CanchaId);
            




            //cancha - tipoDeCancha 
            modelBuilder.Entity<Cancha>()
                 .HasOne(c => c.TipoDeCancha)
                 .WithMany(t => t.Canchas)
                 .HasForeignKey(c => c.TipoDeCanchaId)
                 .OnDelete(DeleteBehavior.Restrict);



            //cancha - partido 
            modelBuilder.Entity<Partido>()
                .HasOne(p => p.Cancha)
                .WithMany(c => c.Partidos)
                 .HasForeignKey(p => p.CanchaId)
                 .OnDelete(DeleteBehavior.Restrict);




            //administrador- reporte 
            modelBuilder.Entity<Reporte>()
              .HasOne(r => r.Administrador)
              .WithMany(a => a.Reportes)
             .HasForeignKey(r => r.AdministradorId)
             .OnDelete(DeleteBehavior.Restrict);


            //administrador - instuctor 
            modelBuilder.Entity<Instructor>()
                 .HasOne(i => i.Administrador)
                 .WithMany(a => a.Instructores)
                 .HasForeignKey(i => i.AdministradorId)
                 .OnDelete(DeleteBehavior.Restrict);



            //reglamento- competencia 
            modelBuilder.Entity<Competencia>()
                .HasOne(c => c.Reglamento)
                .WithMany(r => r.Competencias)
                .HasForeignKey(c => c.ReglamentoId)
                .OnDelete(DeleteBehavior.Restrict);




            //competencia - partido
            modelBuilder.Entity<Partido>()
                 .HasOne(p => p.Competencia)
                 .WithMany(c => c.Partidos)
                 .HasForeignKey(p => p.CompetenciaId)
                 .OnDelete(DeleteBehavior.Restrict);

            //administrador - descuento
            modelBuilder.Entity<Descuento>()
                 .HasOne(d => d.Administrador)
                 .WithMany(a => a.Descuentos)
                 .HasForeignKey(d => d.AdministradorId)
                 .OnDelete(DeleteBehavior.Restrict);





        }
    }
}