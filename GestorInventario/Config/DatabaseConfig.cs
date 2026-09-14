using System;
using Npgsql;

namespace GestorInventario.Config
{
    /// <summary>
    /// Configuración y gestión de la conexión a PostgreSQL en Neon.
    /// Patrón: Singleton para la configuración, Factory para las conexiones.
    /// </summary>
    public static class DatabaseConfig
    {
        // ── Parámetros de conexión a la nube (Neon) ───────────────
        public static string Host { get; set; } = "ep-divine-frog-a52s9zlq-pooler.us-east-2.aws.neon.tech";
        public static int Port { get; set; } = 5432;
        public static string Database { get; set; } = "neondb";
        public static string Username { get; set; } = "neondb_owner";
        public static string Password { get; set; } = "npg_SeH2o3cgxiBT";

        public static string ConnectionString =>
            $"Host={Host};Port={Port};Database={Database};" +
            $"Username={Username};Password={Password};" +
            $"SSL Mode=Require;Trust Server Certificate=true;" +
            $"Pooling=true;Minimum Pool Size=1;Maximum Pool Size=10;";

        // ── Factory: crear conexión abierta ────────────────────
        public static NpgsqlConnection GetConnection()
        {
            var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        // ── Verificar que la BD esté disponible ────────────────
        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using var conn = GetConnection();
                return conn.State == System.Data.ConnectionState.Open;
            }
            catch (NpgsqlException ex)
            {
                errorMessage = $"Error de base de datos: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error inesperado: {ex.Message}";
                return false;
            }
        }
    }
}