# Convenciones de Arquitectura para Tenis Ahora

## Stack
.NET 8, EF Core 8, SQL Server. Tests con xUnit y NSubstitute.

## Arquitectura (Clean Architecture)
Solución en capas: Api / Application / Domain / Infrastructure.
- **Domain**: No referencia a ningún otro proyecto. Entidades ricas (validan sus propias reglas, no solo propiedades get/set anémicas).
- **Application**: Lógica de los casos de uso (Commands/Queries y Handlers). No sabe de HTTP ni de la base de datos (usa interfaces como `IRepository`).
- **Infrastructure**: Acceso a datos con EF Core (DbContext, Repositories). Solo Infrastructure sabe cómo comunicarse con SQL Server.
- **Api**: Capa de presentación con Controllers y Middlewares. Inyecta los Handlers. Cero lógica de negocio acá.

## Patrones
- **CQRS**: Separación de lectura y escritura. Sin librerías intermedias como MediatR. Los controladores inyectan directamente los Handlers.
- **Repository y Unit of Work**: Los repositorios son responsables de consultar y agregar. La Unidad de Trabajo (`IUnitOfWork`) se encarga de guardar los cambios (commit) para asegurar la atomicidad de las operaciones. Nunca usar `SaveChanges` en los repositorios.
- **Inyección de Dependencias**: Vía constructor.

## Convenciones de Código
- Controllers: Usan el atributo `[ApiController]` y enrutamiento basado en recursos (plural), sin verbos en las URL (Ej. `POST /api/reservas`, no `/api/crearReserva`).
- Entidades y tablas en base de datos: Nombres en español (Reserva, Cancha, Inventario).
- Las configuraciones de EF Core se hacen con Fluent API en el `DbContext` o usando clases `IEntityTypeConfiguration`, no Data Annotations en las entidades.
- Errores de negocio: Lanzar excepciones derivadas (ej. `DomainException`) que luego atrapa globalmente un middleware para devolver `HTTP 400 Bad Request`.
