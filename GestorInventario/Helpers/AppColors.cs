using System.Drawing;

namespace GestorInventario.Helpers
{
    public static class AppColors
    {
        // Backgrounds
        public static readonly Color BackgroundGeneral = ColorTranslator.FromHtml("#F8F9FA");
        public static readonly Color BackgroundCard = Color.White;
        public static readonly Color Sidebar = ColorTranslator.FromHtml("#1E293B");
        public static readonly Color SidebarHover = ColorTranslator.FromHtml("#334155");
        public static readonly Color SidebarActive = ColorTranslator.FromHtml("#7B61FF");

        // Primary
        public static readonly Color Primary = ColorTranslator.FromHtml("#7B61FF");
        public static readonly Color PrimaryHover = ColorTranslator.FromHtml("#9D8DF1");
        public static readonly Color PrimaryLight = ColorTranslator.FromHtml("#EDE9FE");

        // Text
        public static readonly Color TextPrimary = ColorTranslator.FromHtml("#1F2937");
        public static readonly Color TextSecondary = ColorTranslator.FromHtml("#6B7280");
        public static readonly Color TextLight = Color.White;

        // Status
        public static readonly Color Success = ColorTranslator.FromHtml("#10B981");
        public static readonly Color Warning = ColorTranslator.FromHtml("#F59E0B");
        public static readonly Color Danger = ColorTranslator.FromHtml("#EF4444");
        public static readonly Color Info = ColorTranslator.FromHtml("#3B82F6");

        // Borders
        public static readonly Color Border = ColorTranslator.FromHtml("#E5E7EB");
        public static readonly Color BorderFocus = ColorTranslator.FromHtml("#7B61FF");

        // TopBar
        public static readonly Color TopBar = Color.White;
        public static readonly Color TopBarBorder = ColorTranslator.FromHtml("#E5E7EB");
    }
}
