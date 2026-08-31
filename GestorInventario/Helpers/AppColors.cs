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
        public static readonly Color DangerHover = ColorTranslator.FromHtml("#DC2626");
        public static readonly Color Info = ColorTranslator.FromHtml("#3B82F6");

        // Borders
        public static readonly Color Border = ColorTranslator.FromHtml("#E2E8F0");
        public static readonly Color BorderFocus = ColorTranslator.FromHtml("#7B61FF");

        // Table DataGridView
        public static readonly Color TableHeader = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color TableHeaderText = ColorTranslator.FromHtml("#475569");
        public static readonly Color TableRowAlt = ColorTranslator.FromHtml("#F8FAFC");

        // Status Backgrounds (Badges)
        public static readonly Color SuccessLight = ColorTranslator.FromHtml("#D1FAE5");
        public static readonly Color WarningLight = ColorTranslator.FromHtml("#FEF3C7");
        public static readonly Color DangerLight = ColorTranslator.FromHtml("#FEE2E2");
        public static readonly Color InfoLight = ColorTranslator.FromHtml("#DBEAFE");

        // TopBar & Modals
        public static readonly Color TopBar = Color.White;
        public static readonly Color TopBarBorder = ColorTranslator.FromHtml("#E2E8F0");
        public static readonly Color ModalHeader = ColorTranslator.FromHtml("#7B61FF");
    }
}
