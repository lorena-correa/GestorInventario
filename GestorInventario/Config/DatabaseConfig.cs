using Npgsql;

namespace GestorInventario.Config
{
    /// <summary>
    /// Configuración y gestión de la conexión a PostgreSQL.
    /// Patrón: Singleton para la configuración, Factory para las conexiones.
    /// </summary>
    public static class DatabaseConfig
    {
        // ── Parámetros de conexión ──────────────────────────────
        public static string Host { get; set; } = "localhost";
        public static int Port { get; set; } = 5432;
        public static string Database { get; set; } = "gestor_inventario";
        public static string Username { get; set; } = "postgres";
        public static string Password { get; set; } = "1234";

        public static string ConnectionString =>
            $"Host={Host};Port={Port};Database={Database};" +
            $"Username={Username};Password={Password};" +
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