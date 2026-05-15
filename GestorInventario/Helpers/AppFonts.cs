using System.Drawing;

namespace GestorInventario.Helpers
{
    /// <summary>
    /// Font factory — always creates a new Font instance to avoid GDI+ 
    /// "Parameter is not valid" errors when reusing static Font objects
    /// across different paint contexts.
    /// </summary>
    public static class AppFonts
    {
        public static Font Title       => new Font("Segoe UI", 22f, FontStyle.Bold);
        public static Font Heading     => new Font("Segoe UI", 14f, FontStyle.Bold);
        public static Font SubHeading  => new Font("Segoe UI", 11f, FontStyle.Bold);
        public static Font Body        => new Font("Segoe UI", 9.5f, FontStyle.Regular);
        public static Font BodyBold    => new Font("Segoe UI", 9.5f, FontStyle.Bold);
        public static Font Small       => new Font("Segoe UI", 8.5f, FontStyle.Regular);
        public static Font SmallBold   => new Font("Segoe UI", 8.5f, FontStyle.Bold);
        public static Font Label       => new Font("Segoe UI", 9f, FontStyle.Regular);
        public static Font Button      => new Font("Segoe UI", 9.5f, FontStyle.Bold);
        public static Font SidebarItem => new Font("Segoe UI", 9.5f, FontStyle.Regular);
        public static Font SidebarActive => new Font("Segoe UI", 9.5f, FontStyle.Bold);
        public static Font Metric      => new Font("Segoe UI", 26f, FontStyle.Bold);
        public static Font MetricLabel => new Font("Segoe UI", 9f, FontStyle.Regular);
    }
}
