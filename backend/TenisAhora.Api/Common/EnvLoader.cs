using System;
using System.IO;

namespace TenisAhora.Api.Common;

/// <summary>
/// Utilidad para cargar variables de entorno desde un archivo .env local
/// en tiempo de desarrollo, mapeándolas automáticamente al esquema de IConfiguration.
/// </summary>
public static class EnvLoader
{
    public static void Cargar()
    {
        var directorios = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory,
            Path.Combine(Directory.GetCurrentDirectory(), "backend", "TenisAhora.Api"),
            Path.Combine(Directory.GetCurrentDirectory(), ".."),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..")
        };

        foreach (var dir in directorios)
        {
            try
            {
                var envPath = Path.GetFullPath(Path.Combine(dir, ".env"));
                if (File.Exists(envPath))
                {
                    CargarArchivo(envPath);
                    return;
                }
            }
            catch
            {
                // Continuar buscando en los demás candidatos
            }
        }
    }

    private static void CargarArchivo(string filePath)
    {
        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                continue;

            var idx = trimmed.IndexOf('=');
            if (idx <= 0)
                continue;

            var key = trimmed.Substring(0, idx).Trim();
            var val = trimmed.Substring(idx + 1).Trim().Trim('"', '\'');

            Environment.SetEnvironmentVariable(key, val);

            // Mapeos automáticos para que IConfiguration los reconozca en cualquier sintaxis
            if (key.Equals("MERCADOPAGO_ACCESS_TOKEN", StringComparison.OrdinalIgnoreCase))
            {
                Environment.SetEnvironmentVariable("MercadoPago__AccessToken", val);
                Environment.SetEnvironmentVariable("MercadoPago:AccessToken", val);
            }
            else if (key.Equals("MERCADOPAGO_PUBLIC_KEY", StringComparison.OrdinalIgnoreCase))
            {
                Environment.SetEnvironmentVariable("MercadoPago__PublicKey", val);
                Environment.SetEnvironmentVariable("MercadoPago:PublicKey", val);
            }
            else if (key.Equals("MERCADOPAGO_WEBHOOK_URL", StringComparison.OrdinalIgnoreCase))
            {
                Environment.SetEnvironmentVariable("MercadoPago__WebhookUrl", val);
                Environment.SetEnvironmentVariable("MercadoPago:WebhookUrl", val);
            }
            else if (key.Equals("DB_CONNECTION_STRING", StringComparison.OrdinalIgnoreCase))
            {
                Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", val);
            }
            else if (key.Equals("JWT_SECRET_KEY", StringComparison.OrdinalIgnoreCase))
            {
                Environment.SetEnvironmentVariable("Jwt__SecretKey", val);
            }
        }
    }
}
