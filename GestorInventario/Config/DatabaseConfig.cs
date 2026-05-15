namespace GestorInventario.Config
{
    /// <summary>
    /// Database connection configuration.
    /// Update these values to connect to your PostgreSQL instance.
    /// </summary>
    public static class DatabaseConfig
    {
        public static string Host { get; set; } = "localhost";
        public static int Port { get; set; } = 5432;
        public static string Database { get; set; } = "gestor_inventario";
        public static string Username { get; set; } = "postgres";
        public static string Password { get; set; } = "password";

        public static string ConnectionString =>
            $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";
    }
}
