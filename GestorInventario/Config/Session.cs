namespace GestorInventario.Config
{
    /// <summary>
    /// Stores current authenticated user session data.
    /// </summary>
    public static class Session
    {
        public static int UserId { get; set; }
        public static string UserName { get; set; } = "Administrador";
        public static string Email { get; set; } = "admin@empresa.com";
        public static string Role { get; set; } = "Administrador";
        public static bool IsAuthenticated { get; set; } = false;

        public static void Clear()
        {
            UserId = 0;
            UserName = string.Empty;
            Email = string.Empty;
            Role = string.Empty;
            IsAuthenticated = false;
        }
    }
}
