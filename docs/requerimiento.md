# Guía de Ejecución del Proyecto (Backend & Frontend)

Esta sección detalla cómo poner en funcionamiento el entorno completo de **TenisAhora** (Base de Datos SQL Server, API Backend en .NET 8 y Frontend React con Vite).

---

## 1. Requisitos Previos

- **.NET SDK 8.0** instalado.
- **Node.js** (v18 o superior) y **npm** instalados.
- **Docker** instalado y en ejecución (para la base de datos SQL Server).

---

## 2. Paso 1: Levantar la Base de Datos (SQL Server en Docker)

El backend está configurado en `backend/TenisAhora.Api/appsettings.json` para conectarse a un contenedor local en el puerto `1433`:

```bash
# Iniciar el contenedor si ya existe:
docker start sqlserver_tenis

# O crearlo por primera vez si no fue creado:
docker run -e "ACCEPT_EULA=Y" \
           -e "MSSQL_SA_PASSWORD=TenisAhora123!" \
           -p 1433:1433 \
           --name sqlserver_tenis \
           -d mcr.microsoft.com/mssql/server:2022-latest
```

> **Nota:** La cadena de conexión utilizada es:  
> `Server=localhost,1433;Database=TenisAhoraDb;User Id=sa;Password=TenisAhora123!;TrustServerCertificate=True;MultipleActiveResultSets=true`

---

## 3. Paso 2: Levantar el Backend (.NET 8 Web API)

Abre una terminal en la raíz del repositorio:

```bash
# 1. Exportar la variable de runtime (necesaria en Linux)
export XDG_RUNTIME_DIR=/tmp

# 2. Compilar la solución (opcional, para verificar)
dotnet build backend/TenisAhora.slnx

# 3. Ejecutar la API
dotnet run --project backend/TenisAhora.Api/TenisAhora.Api.csproj --launch-profile http
```

### ¿Qué sucede al iniciar la API?
- **Migraciones automáticas**: La aplicación aplica todas las migraciones pendientes en SQL Server al arrancar (`context.Database.Migrate()`).
- **Data Seeder**: Se cargan automáticamente datos de prueba (usuarios administradores, socios, profesores, entrenadores, canchas, stocks y productos).
- **URLs disponibles**:
  - **HTTP**: `http://localhost:5242`
  - **Swagger UI (Documentación interactiva)**: `http://localhost:5242/swagger`

---

## 4. Paso 3: Levantar el Frontend (React + Vite)

Abre **otra terminal** independiente:

```bash
# 1. Ingresar a la carpeta del frontend
cd frontend

# 2. Instalar dependencias (solo la primera vez)
npm install

# 3. Iniciar el servidor de desarrollo Vite
npm run dev
```

### URLs del Frontend:
- **Aplicación Web**: `http://localhost:5173`
- El cliente API (`frontend/src/services/api.js`) ya está pre-configurado para consumir `http://localhost:5242/api`.

---

## 5. Usuarios y Credenciales Reales del Sistema (DataSeeder)

El `DataSeeder` del proyecto crea los siguientes usuarios por defecto con la contraseña **`123`**:

| Rol | Correo Electrónico | Contraseña | Nombre y Apellido | Permisos y Vistas |
| :--- | :--- | :--- | :--- | :--- |
| **Administrador** | `pablo.gomez@tenisahora.com` | `123` | Pablo Gomez | Panel de control, canchas, disponibilidad, productos, reportes |
| **Socio** | `juan.perez@tenisahora.com` | `123` | Juan Pérez | Alquiler de canchas, torneos, ligas, perfil de socio |
| **Profesor** | `carlos.gomez@tenisahora.com` | `123` | Carlos Peralta | Portal docente, clases, asistencias |
| **Entrenador** | `pedro.martinez@tenisahora.com` | `123` | Pedro Martínez | Portal de entrenamientos, asistencias |

> **Nota:** También puedes dar de alta un nuevo socio haciendo clic en **"Registrate acá"** directamente desde la ventana de login del Frontend.

---

## 6. Comandos de Verificación Rápida

```bash
# Correr pruebas unitarias del backend:
export XDG_RUNTIME_DIR=/tmp
dotnet test backend/TenisAhora.UnitTests/TenisAhora.UnitTests.csproj

# Verificar el estado de la base de datos en Docker:
docker ps | grep sqlserver_tenis
```

