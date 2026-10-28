using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure
{
    public static class DataSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Evita duplicar los datos si la aplicación se inicia varias veces.
            if (context.Personas.Any())
            {
                SeedProductosSiNoExisten(context);
                return;
            }

            // ============================================================
            // 1. ADMINISTRADOR
            // ============================================================

            var administrador = new Administrador
            {
                Nombre = "Pablo",
                Apellido = "Gomez",
                Dni = 30111222,
                FechaDeNacimiento = new DateTime(1990, 5, 10),
                Telefono = "1122334455",
                Direccion = "Av. Siempre Viva 123",
                CodigoPostal = 1888,
                Email = "pablo.gomez@tenisahora.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                Rol = Rol.Administrador,
                Activo = true
            };

            context.Administradores.Add(administrador);
            context.SaveChanges();

            // ============================================================
            // 2. SOCIO
            // ============================================================

            var socio = new Socio
            {
                Nombre = "Juan",
                Apellido = "Pérez",
                Dni = 40123456,
                FechaDeNacimiento = new DateTime(1998, 8, 20),
                Telefono = "1166778899",
                Direccion = "Calle Falsa 456",
                CodigoPostal = 1888,
                Email = "juan.perez@tenisahora.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                Rol = Rol.Socio,
                estadoMembresia = EstadoMembresia.Activa,
                Activo = true
            };

            context.Socios.Add(socio);
            context.SaveChanges();

            // ============================================================
            // 3. PROFESOR
            // ============================================================

            var profesor = new Profesor
            {
                Nombre = "Carlos",
                Apellido = "Peralta",
                Dni = 32123456,
                FechaDeNacimiento = new DateTime(1988, 3, 15),
                Telefono = "1155667788",
                Direccion = "Calle Mitre 100",
                CodigoPostal = 1888,
                Email = "carlos.gomez@tenisahora.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                Rol = Rol.Profesor,
                Activo = true
            };

            SetPrivate(profesor, "especialidad", "Profesor de tenis");
            SetPrivate(profesor, "antiguedad", 8);

            profesor.AdministradorId = GetId(administrador);

            context.Profesores.Add(profesor);
            context.SaveChanges();

            // ============================================================
            // 4. ENTRENADOR
            // ============================================================

            var entrenador = new Entrenador
            {
                Nombre = "Pedro",
                Apellido = "Martínez",
                Dni = 33123456,
                FechaDeNacimiento = new DateTime(1985, 7, 12),
                Telefono = "1144556677",
                Direccion = "Calle Rivadavia 200",
                CodigoPostal = 1888,
                Email = "pedro.martinez@tenisahora.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                Rol = Rol.Entrenador,
                Activo = true
            };

            SetPrivate(entrenador, "especialidad", "Entrenamiento deportivo");
            SetPrivate(entrenador, "antiguedad", 10);

            entrenador.AdministradorId = GetId(administrador);

            context.Entrenadores.Add(entrenador);
            context.SaveChanges();

            // ============================================================
            // 5. CERTIFICACIONES
            // ============================================================

            var certificacionProfesor = new CertificacionDeportiva();

            SetPrivate(certificacionProfesor, "certificacionDeportiva", true);
            SetPrivate(certificacionProfesor, "FechaEmision", new DateTime(2024, 3, 1));
            SetPrivate(certificacionProfesor, "NombreCertificacion", "Certificación de Profesor de Tenis");
            SetPrivate(certificacionProfesor, "EnteEmisor", "Federación Argentina de Tenis");

            certificacionProfesor.InstructorId = GetId(profesor);

            context.CertificacionesDeportivas.Add(certificacionProfesor);

            var certificacionEntrenador = new CertificacionDeportiva();

            SetPrivate(certificacionEntrenador, "certificacionDeportiva", true);
            SetPrivate(certificacionEntrenador, "FechaEmision", new DateTime(2023, 6, 15));
            SetPrivate(certificacionEntrenador, "NombreCertificacion", "Certificación de Entrenador de Tenis");
            SetPrivate(certificacionEntrenador, "EnteEmisor", "Federación Argentina de Tenis");

            certificacionEntrenador.InstructorId = GetId(entrenador);

            context.CertificacionesDeportivas.Add(certificacionEntrenador);
            context.SaveChanges();

            // ============================================================
            // 6. TIPOS DE CANCHA
            // ============================================================

            var tipoLadrillo = new TipoDeCancha();

            SetPrivate(tipoLadrillo, "superficie", Superficie.Ladrillo);
            SetPrivate(tipoLadrillo, "capacidad", 4);
            SetPrivate(tipoLadrillo, "precio", 12000.0);

            var tipoCemento = new TipoDeCancha();

            SetPrivate(tipoCemento, "superficie", Superficie.Cemento);
            SetPrivate(tipoCemento, "capacidad", 4);
            SetPrivate(tipoCemento, "precio", 10000.0);

            var tipoPasto = new TipoDeCancha();

            SetPrivate(tipoPasto, "superficie", Superficie.Pasto);
            SetPrivate(tipoPasto, "capacidad", 4);
            SetPrivate(tipoPasto, "precio", 15000.0);

            context.TiposDeCancha.AddRange(
                tipoLadrillo,
                tipoCemento,
                tipoPasto
            );

            context.SaveChanges();

            // ============================================================
            // 7. CANCHAS
            // ============================================================

            var cancha1 = new Cancha
            {
                TipoDeCanchaId = GetId(tipoLadrillo)
            };

            var cancha2 = new Cancha
            {
                TipoDeCanchaId = GetId(tipoCemento)
            };

            var cancha3 = new Cancha
            {
                TipoDeCanchaId = GetId(tipoPasto)
            };

            context.Canchas.AddRange(
                cancha1,
                cancha2,
                cancha3
            );

            context.SaveChanges();

            // ============================================================
            // 8. DISPONIBILIDADES
            // ============================================================

            var disponibilidad1 = new Disponibilidad();

            SetPrivate(
                disponibilidad1,
                "FechaHoraInicio",
                new DateTime(2026, 10, 5, 8, 0, 0)
            );

            SetPrivate(
                disponibilidad1,
                "FechaHoraFin",
                new DateTime(2026, 10, 5, 22, 0, 0)
            );

            SetPrivate(
                disponibilidad1,
                "estado",
                EstadoDisponibilidad.Disponible
            );

            disponibilidad1.CanchaId = GetId(cancha1);

            var disponibilidad2 = new Disponibilidad();

            SetPrivate(
                disponibilidad2,
                "FechaHoraInicio",
                new DateTime(2026, 10, 5, 8, 0, 0)
            );

            SetPrivate(
                disponibilidad2,
                "FechaHoraFin",
                new DateTime(2026, 10, 20, 22, 0, 0)
            );

            SetPrivate(
                disponibilidad2,
                "estado",
                EstadoDisponibilidad.Disponible
            );

            disponibilidad2.CanchaId = GetId(cancha2);

            var disponibilidad3 = new Disponibilidad();

            SetPrivate(
                disponibilidad3,
                "FechaHoraInicio",
                new DateTime(2026, 10, 5, 8, 0, 0)
            );

            SetPrivate(
                disponibilidad3,
                "FechaHoraFin",
                new DateTime(2026, 10, 5, 22, 0, 0)
            );

            SetPrivate(
                disponibilidad3,
                "estado",
                EstadoDisponibilidad.Mantenimiento
            );

            disponibilidad3.CanchaId = GetId(cancha3);

            context.Disponibilidades.AddRange(
                disponibilidad1,
                disponibilidad2,
                disponibilidad3
            );

            context.SaveChanges();

            // ============================================================
            // 9. STOCK
            // ============================================================

            var pelotas = new Stock();

            SetPrivate(pelotas, "tipo_elemento", "Pelotas de tenis");
            SetPrivate(pelotas, "cantidad_disponible", 30);

            var raquetas = new Stock();

            SetPrivate(raquetas, "tipo_elemento", "Raquetas");
            SetPrivate(raquetas, "cantidad_disponible", 10);

            var redes = new Stock();

            SetPrivate(redes, "tipo_elemento", "Redes");
            SetPrivate(redes, "cantidad_disponible", 5);

            context.Stocks.AddRange(
                pelotas,
                raquetas,
                redes
            );

            context.SaveChanges();

            // ============================================================
            // 9.1 PRODUCTOS
            // ============================================================

            var tuboWilson = new Producto
            {
                Nombre = "Tubo de Pelotas Wilson US Open (x3)",
                Descripcion = "Tubo presurizado con 3 pelotas de tenis de alto rendimiento para todas las superficies.",
                Precio = 8500f,
                StockId = GetId(pelotas),
                Activo = true
            };

            var raquetaWilson = new Producto
            {
                Nombre = "Raqueta Wilson Blade 98",
                Descripcion = "Raqueta de control y precisión superior con marco de grafito.",
                Precio = 210000f,
                StockId = GetId(raquetas),
                Activo = true
            };

            var redTenis = new Producto
            {
                Nombre = "Red de Tenis Profesional Doble Malla",
                Descripcion = "Red reglamentaria con cable de acero galvanizado y banda superior reforzada.",
                Precio = 95000f,
                StockId = GetId(redes),
                Activo = true
            };

            context.Productos.AddRange(
                tuboWilson,
                raquetaWilson,
                redTenis
            );

            context.SaveChanges();

            // ============================================================
            // 10. REGLAMENTO
            // ============================================================

            var reglamento = new Reglamento();

            SetPrivate(reglamento, "Nombre", "Reglamento de competencias");
            SetPrivate(
                reglamento,
                "Contenido",
                "Reglamento oficial de competencias y normas internas del club."
            );
            SetPrivate(reglamento, "Vigencia", true);
            SetPrivate(reglamento, "TipoReglamento", "Competencia");

            reglamento.Activo = true;

            context.Reglamentos.Add(reglamento);
            context.SaveChanges();

            // ============================================================
            // 11. ACTIVIDAD - CLASE
            // ============================================================

            var clase = new Clase();

            SetPrivate(clase, "HoraInicio", DateTime.Today.AddHours(10));
            SetPrivate(clase, "HoraFin", DateTime.Today.AddHours(11));
            SetPrivate(clase, "CapacidadMaxma", 30);
            SetPrivate(clase, "CantidadDeAlumnos", 10);
            SetPrivate(clase, "Duracion", 60);

            clase.Activo = true;
            clase.ProfesorId = GetId(profesor);
            clase.EntrenadorId = null;

            context.Actividades.Add(clase);
            context.SaveChanges();

            // ============================================================
            // ACTIVIDAD - ENTRENAMIENTO
            // ============================================================

            var entrenamiento = new Entrenamiento();

            SetPrivate(entrenamiento, "HoraInicio", DateTime.Today.AddHours(15));
            SetPrivate(entrenamiento, "HoraFin", DateTime.Today.AddHours(16));
            SetPrivate(entrenamiento, "CapacidadMaxma", 10);
            SetPrivate(entrenamiento, "CantidadDeAlumnos", 4);
            SetPrivate(entrenamiento, "Duracion", 60);

            entrenamiento.Activo = true;
            entrenamiento.ProfesorId = null;
            entrenamiento.EntrenadorId = GetId(entrenador);

            context.Actividades.Add(entrenamiento);
            context.SaveChanges();

            // ============================================================
            // 12. ASISTENCIA
            // ============================================================

            var asistencia = new Asistencia();

            SetPrivate(
                asistencia,
                "Fecha",
                new DateTime(2026, 10, 6)
            );

            SetPrivate(
                asistencia,
                "presente",
                true
            );

            asistencia.ActividadId = GetId(clase);
            asistencia.SocioId = GetId(socio);

            context.Asistencias.Add(asistencia);
            context.SaveChanges();

            // ============================================================
            // 13. EQUIPO
            // ============================================================

            var equipo = new Equipo();

            SetPrivate(equipo, "Nombre", "Los Saques");

            equipo.Socios.Add(socio);
            socio.Equipos.Add(equipo);

            context.Equipos.Add(equipo);
            context.SaveChanges();

            // ============================================================
            // 14. COMPETENCIA
            // ============================================================

            var competencia = new Torneo();

            SetPrivate(competencia, "Nombre", "Torneo Primavera");

            SetPrivate(
                competencia,
                "FechaInicio",
                new DateTime(2026, 10, 10)
            );

            SetPrivate(
                competencia,
                "FechaFin",
                new DateTime(2026, 10, 20)
            );

            SetPrivate(
                competencia,
                "categoriaGenero",
                CategoriaGenero.Masculino
            );

            SetPrivate(
                competencia,
                "modalidad",
                Modalidad.Dobles
            );

            SetPrivate(
                competencia,
                "estado",
                EstadoCompetencia.EnCurso
            );

            SetPrivate(
                competencia,
                "etapa_actual",
                "Cuartos de final"
            );

            SetPrivate(
                competencia,
                "tipo_llave",
                "Eliminación directa"
            );

            competencia.Activo = true;
            competencia.ReglamentoId = GetId(reglamento);

            context.Torneos.Add(competencia);
            context.SaveChanges();

            // ============================================================
            // 15. INSCRIPCIÓN A COMPETENCIA
            // ============================================================

            var inscripcionCompetencia = new Inscripcion_competencia();

            SetPrivate(
                inscripcionCompetencia,
                "Fecha",
                new DateTime(2026, 10, 1)
            );

            SetPrivate(
                inscripcionCompetencia,
                "estadoInscripcion",
                EstadoInscripcion.Confirmada
            );

            inscripcionCompetencia.SocioId = GetId(socio);
            inscripcionCompetencia.CompetenciaId = GetId(competencia);

            inscripcionCompetencia.Equipos.Add(equipo);
            equipo.InscripcionesCompetencia.Add(inscripcionCompetencia);

            context.InscripcionesCompetencia.Add(inscripcionCompetencia);
            context.SaveChanges();

            // ============================================================
            // 16. PARTIDO
            // ============================================================

            var partido = new Partido();

            SetPrivate(
                partido,
                "fecha",
                new DateTime(2026, 10, 12)
            );

            SetPrivate(
                partido,
                "HoraInicio",
                new DateTime(2026, 10, 12, 18, 0, 0)
            );

            SetPrivate(
                partido,
                "HoraFin",
                new DateTime(2026, 10, 12, 20, 0, 0)
            );

            SetPrivate(
                partido,
                "ronda",
                "Cuartos de final"
            );

            SetPrivate(
                partido,
                "resultado",
                "6-4 / 6-3"
            );

            partido.CompetenciaId = GetId(competencia);
            partido.CanchaId = GetId(cancha1);

            context.Partidos.Add(partido);
            context.SaveChanges();

            // ============================================================
            // 17. DESCUENTO
            // ============================================================

            var descuento = new Descuento();

            SetPrivate(descuento, "porcentaje", 10f);

            SetPrivate(
                descuento,
                "descripcion",
                "Descuento para miembros del club"
            );

            SetPrivate(
                descuento,
                "condiciones",
                "Membresía activa"
            );

            descuento.AdministradorId = GetId(administrador);

            context.Descuentos.Add(descuento);
            context.SaveChanges();

            // ============================================================
            // 18. INSCRIPCIÓN NORMAL
            // ============================================================

            var inscripcion = new Inscripcion();

            SetPrivate(
                inscripcion,
                "Fecha",
                new DateTime(2026, 10, 1)
            );

            SetPrivate(
                inscripcion,
                "estadoInscripcion",
                EstadoInscripcion.Confirmada
            );

            inscripcion.SocioId = GetId(socio);

            context.Inscripciones.Add(inscripcion);
            context.SaveChanges();

            // ============================================================
            // 19. RESERVA
            // ============================================================

            var reserva = new Reserva();

            SetPrivate(
                reserva,
                "fecha_reserva",
                new DateTime(2026, 10, 8)
            );

            SetPrivate(
                reserva,
                "HoraInicio",
                new DateTime(2026, 10, 8, 18, 0, 0)
            );

            SetPrivate(
                reserva,
                "HoraFin",
                new DateTime(2026, 10, 8, 20, 0, 0)
            );

            SetPrivate(
                reserva,
                "estado",
                EstadoReserva.Confirmada
            );

            SetPrivate(
                reserva,
                "importe",
                12000f
            );

            reserva.CanchaId = GetId(cancha1);

            reserva.Socios.Add(socio);
            socio.Reservas.Add(reserva);

            context.Reservas.Add(reserva);
            context.SaveChanges();

            // ============================================================
            // 20. DETALLE DE STOCK DE LA RESERVA
            // ============================================================

            var detalleStock = new Detalle_stock_reserva();

            SetPrivate(detalleStock, "cantidad", 3);

            detalleStock.ReservaId = GetId(reserva);
            detalleStock.StockId = GetId(pelotas);

            context.DetallesStockReserva.Add(detalleStock);
            context.SaveChanges();

            // ============================================================
            // 21. PAGOS
            // ============================================================

            // ------------------------------------------------------------
            // Pago de la reserva
            // ------------------------------------------------------------

            var pagoReserva = new Pago();

            SetPrivate(
                pagoReserva,
                "importe",
                6000f
            );

            SetPrivate(
                pagoReserva,
                "Fecha_pago",
                new DateTime(2026, 10, 8, 17, 30, 0)
            );

            SetPrivate(
                pagoReserva,
                "tipo_pago",
                TipoDePago.Transferencia
            );

            SetPrivate(
                pagoReserva,
                "estado",
                EstadoPago.Validado
            );

            SetPrivate(
                pagoReserva,
                "porcentaje_pago",
                50f
            );

            // Este pago corresponde a la reserva.
            pagoReserva.ReservaId = GetId(reserva);
            pagoReserva.InscripcionId = null;
            pagoReserva.DescuentoId = GetId(descuento);

            context.Pagos.Add(pagoReserva);
            context.SaveChanges();

            // ------------------------------------------------------------
            // Pago de la inscripción
            // ------------------------------------------------------------

            var pagoInscripcion = new Pago();

            SetPrivate(
                pagoInscripcion,
                "importe",
                5000f
            );

            SetPrivate(
                pagoInscripcion,
                "Fecha_pago",
                new DateTime(2026, 10, 1, 12, 0, 0)
            );

            SetPrivate(
                pagoInscripcion,
                "tipo_pago",
                TipoDePago.Transferencia
            );

            SetPrivate(
                pagoInscripcion,
                "estado",
                EstadoPago.Validado
            );

            SetPrivate(
                pagoInscripcion,
                "porcentaje_pago",
                100f
            );

            // Este pago corresponde a la inscripción.
            pagoInscripcion.ReservaId = null;
            pagoInscripcion.InscripcionId = GetId(inscripcion);
            pagoInscripcion.DescuentoId = GetId(descuento);

            context.Pagos.Add(pagoInscripcion);
            context.SaveChanges();

            // ============================================================
            // 22. RECIBOS
            // ============================================================

            // Recibo del pago de la reserva
            var reciboReserva = new Recibo();

            SetPrivate(
                reciboReserva,
                "FechaEmision",
                new DateTime(2026, 10, 8, 17, 35, 0)
            );

            SetPrivate(
                reciboReserva,
                "importe",
                6000f
            );

            reciboReserva.PagoId = GetId(pagoReserva);

            context.Recibos.Add(reciboReserva);
            context.SaveChanges();

            // Recibo del pago de la inscripción
            var reciboInscripcion = new Recibo();

            SetPrivate(
                reciboInscripcion,
                "FechaEmision",
                new DateTime(2026, 10, 1, 12, 5, 0)
            );

            SetPrivate(
                reciboInscripcion,
                "importe",
                5000f
            );

            reciboInscripcion.PagoId = GetId(pagoInscripcion);

            context.Recibos.Add(reciboInscripcion);
            context.SaveChanges();

            // ============================================================
            // 23. REPORTE
            // ============================================================

            var reporte = new Reporte();

            SetPrivate(
                reporte,
                "FechaGeneracion",
                new DateTime(2026, 10, 8, 20, 0, 0)
            );

            SetPrivate(
                reporte,
                "tipo_reporte",
                "Reporte de reservas"
            );

            SetPrivate(
                reporte,
                "contenido_detalle",
                "Reporte de prueba generado por el administrador."
            );

            reporte.AdministradorId = GetId(administrador);

            context.Reportes.Add(reporte);
            context.SaveChanges();
        }

        // ================================================================
        // MÉTODOS AUXILIARES
        // ================================================================

        private static void SetPrivate(
            object entity,
            string propertyName,
            object value)
        {
            var type = entity.GetType();

            PropertyInfo? property = null;

            while (type != null)
            {
                property = type.GetProperty(
                    propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly
                );

                if (property != null)
                    break;

                type = type.BaseType;
            }

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"No se encontró la propiedad '{propertyName}' en '{entity.GetType().Name}' ni en sus clases base."
                );
            }

            property.SetValue(entity, value);
        }

        private static int GetId(object entity)
        {
            var property = entity
                .GetType()
                .GetProperty(
                    "Id",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"No se encontró la propiedad Id en '{entity.GetType().Name}'."
                );
            }

            return (int)property.GetValue(entity)!;
        }

        private static void SeedProductosSiNoExisten(AppDbContext context)
        {
            // Limpiar producto de prueba si existe en la tabla Productos
            var productoTubo = context.Productos.FirstOrDefault(p => p.Id == 4 || p.Nombre.Contains("Tubo Pelotas Babolat"));
            if (productoTubo != null)
            {
                context.Productos.Remove(productoTubo);
                context.SaveChanges();
            }

            if (context.Productos.Any())
                return;

            var pelotas = context.Stocks.FirstOrDefault(s => s.tipo_elemento.Contains("Pelotas")) ?? context.Stocks.FirstOrDefault();
            var raquetas = context.Stocks.FirstOrDefault(s => s.tipo_elemento.Contains("Raquetas")) ?? pelotas;
            var redes = context.Stocks.FirstOrDefault(s => s.tipo_elemento.Contains("Redes")) ?? pelotas;

            if (pelotas == null)
                return;

            var productos = new List<Producto>
            {
                new Producto
                {
                    Nombre = "Tubo de Pelotas Wilson US Open (x3)",
                    Descripcion = "Tubo presurizado con 3 pelotas de tenis de alto rendimiento para todas las superficies.",
                    Precio = 8500f,
                    StockId = pelotas.Id,
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Raqueta Wilson Blade 98",
                    Descripcion = "Raqueta de control y precisión superior con marco de grafito.",
                    Precio = 210000f,
                    StockId = raquetas?.Id ?? pelotas.Id,
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Red de Tenis Profesional Doble Malla",
                    Descripcion = "Red reglamentaria con cable de acero galvanizado y banda superior reforzada.",
                    Precio = 95000f,
                    StockId = redes?.Id ?? pelotas.Id,
                    Activo = true
                }
            };

            context.Productos.AddRange(productos);
            context.SaveChanges();
        }
    }
}
