using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;

namespace GestorInventario.Forms
{
    // =========================================================================
    // frmAyuda — Formulario con componente WebBrowser integrado
    // =========================================================================
    public class frmAyuda : Form
    {
        private WebBrowser webBrowserAyuda = null!;
        private TextBox txtUrl = null!;
        private Button btnIr = null!;
        private Button btnAtras = null!;
        private Button btnRecargar = null!;
        private Button btnAbrirExterno = null!;

        private const string UrlPredeterminada = "https://learn.microsoft.com/es-es/dotnet/csharp/";

        public frmAyuda()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // Barra de Navegación Web
            var navCard = new CardPanel
            {
                Location = new Point(20, 16),
                Size = new Size(1140, 68),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            btnAtras = UIHelper.CreateSecondaryButton("◀ Atrás", new Size(80, 36), new Point(16, 16));
            btnAtras.Click += (s, e) => { if (webBrowserAyuda.CanGoBack) webBrowserAyuda.GoBack(); };
            navCard.Controls.Add(btnAtras);

            btnRecargar = UIHelper.CreateSecondaryButton("🔄 Recargar", new Size(95, 36), new Point(102, 16));
            btnRecargar.Click += (s, e) => webBrowserAyuda.Refresh();
            navCard.Controls.Add(btnRecargar);

            txtUrl = new TextBox
            {
                Location = new Point(205, 18),
                Size = new Size(670, 32),
                Font = AppFonts.Body,
                BorderStyle = BorderStyle.FixedSingle,
                Text = UrlPredeterminada
            };
            txtUrl.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Navegar(txtUrl.Text); };
            navCard.Controls.Add(txtUrl);

            btnIr = UIHelper.CreatePrimaryButton("Ir", new Size(55, 36), new Point(885, 16));
            btnIr.Click += (s, e) => Navegar(txtUrl.Text);
            navCard.Controls.Add(btnIr);

            btnAbrirExterno = UIHelper.CreateSecondaryButton("🔗 Navegador", new Size(130, 36), new Point(948, 16));
            btnAbrirExterno.Click += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = txtUrl.Text, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo abrir el navegador externo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            navCard.Controls.Add(btnAbrirExterno);

            Controls.Add(navCard);

            // Contenedor WebBrowser
            var browserCard = new CardPanel
            {
                Location = new Point(20, 92),
                Size = new Size(1140, 528),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            webBrowserAyuda = new WebBrowser
            {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true,
                IsWebBrowserContextMenuEnabled = true
            };

            webBrowserAyuda.Navigated += (s, e) =>
            {
                if (webBrowserAyuda.Url != null)
                    txtUrl.Text = webBrowserAyuda.Url.ToString();
            };

            browserCard.Controls.Add(webBrowserAyuda);
            Controls.Add(browserCard);

            ResumeLayout();

            // Cargar página inicial
            Navegar(UrlPredeterminada);
        }

        private void Navegar(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            try
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                    url = "https://" + url;
                webBrowserAyuda.Navigate(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al navegar a la URL: {ex.Message}", "Navegación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    // =========================================================================
    // frmAcercaDe — Formulario Acerca De (Panel, Labels, Botones, TextBox Multiline=true)
    // =========================================================================
    public class frmAcercaDe : Form
    {
        public frmAcercaDe()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // Card Principal Central
            var mainCard = new CardPanel
            {
                Location = new Point(40, 24),
                Size = new Size(1100, 595),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.White
            };

            // Banner Superior con Degradado
            var banner = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1100, 120),
                Dock = DockStyle.Top,
                BackColor = AppColors.Sidebar
            };
            banner.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Logo Círculo
                using var brushCircle = new SolidBrush(AppColors.Primary);
                g.FillEllipse(brushCircle, 30, 25, 70, 70);

                using var fontLogo = new Font("Segoe UI", 26f, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("📦", fontLogo, Brushes.White, new RectangleF(30, 25, 70, 70), sf);

                // Título
                using var fontTitulo = AppFonts.Title;
                g.DrawString("SISTEMA DE FACTURACIÓN Y CONTROL DE INVENTARIO", fontTitulo, Brushes.White, new Point(120, 26));

                // Subtítulo
                using var fontSub = AppFonts.Body;
                using var brushSub = new SolidBrush(Color.FromArgb(190, 255, 255, 255));
                g.DrawString("Aplicación de Escritorio en C# .NET 8 con Diseño Gráfico Moderno y Navegación Modular", fontSub, brushSub, new Point(122, 68));
            };
            mainCard.Controls.Add(banner);

            // Contenedor de Información
            int y = 140;

            // Fila 1: Cards de Metadatos
            var cardMeta1 = new CardPanel { Location = new Point(30, y), Size = new Size(330, 110), BackColor = Color.FromArgb(248, 249, 253) };
            cardMeta1.Controls.Add(new Label { Text = "🏛️  INSTITUCIÓN", Font = AppFonts.SmallBold, ForeColor = AppColors.Primary, Location = new Point(15, 12), AutoSize = true });
            cardMeta1.Controls.Add(new Label { Text = "Institución Universitaria Pascual Bravo", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(15, 36), AutoSize = true });
            cardMeta1.Controls.Add(new Label { Text = "Facultad de Ingeniería\nTecnología en Desarrollo de Software", Font = AppFonts.Small, ForeColor = AppColors.TextSecondary, Location = new Point(15, 60), AutoSize = true });
            mainCard.Controls.Add(cardMeta1);

            var cardMeta2 = new CardPanel { Location = new Point(380, y), Size = new Size(330, 110), BackColor = Color.FromArgb(248, 249, 253) };
            cardMeta2.Controls.Add(new Label { Text = "📚  ASIGNATURA", Font = AppFonts.SmallBold, ForeColor = AppColors.Primary, Location = new Point(15, 12), AutoSize = true });
            cardMeta2.Controls.Add(new Label { Text = "Herramientas de programación III", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(15, 36), AutoSize = true });
            cardMeta2.Controls.Add(new Label { Text = "Código: ET0056\nUnidad: Saber 2 (Diseño UI en C#)", Font = AppFonts.Small, ForeColor = AppColors.TextSecondary, Location = new Point(15, 60), AutoSize = true });
            mainCard.Controls.Add(cardMeta2);

            var cardMeta3 = new CardPanel { Location = new Point(730, y), Size = new Size(330, 110), BackColor = Color.FromArgb(248, 249, 253) };
            cardMeta3.Controls.Add(new Label { Text = "💻  ARQUITECTURA Y VERSIÓN", Font = AppFonts.SmallBold, ForeColor = AppColors.Primary, Location = new Point(15, 12), AutoSize = true });
            cardMeta3.Controls.Add(new Label { Text = "Versión: 2.0.0 (Saber 2)", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(15, 36), AutoSize = true });
            cardMeta3.Controls.Add(new Label { Text = "Framework: C# .NET 8 WinForms\nPatrón: Single-Container Panel (Sin MDI)", Font = AppFonts.Small, ForeColor = AppColors.TextSecondary, Location = new Point(15, 60), AutoSize = true });
            mainCard.Controls.Add(cardMeta3);

            y += 125;

            // TextBox con propiedad Multiline = true según lo exigido por la guía
            var lblDetalles = new Label
            {
                Text = "📝  DESCRIPCIÓN TÉCNICA Y ESTÁNDARES IMPLEMENTADOS (TextBox Multiline):",
                Font = AppFonts.SmallBold,
                ForeColor = AppColors.TextSecondary,
                Location = new Point(30, y),
                AutoSize = true
            };
            mainCard.Controls.Add(lblDetalles);

            var txtDescripcionTecnica = new TextBox
            {
                Location = new Point(30, y + 24),
                Size = new Size(1030, 175),
                Font = AppFonts.Body,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(250, 250, 250),
                ForeColor = AppColors.TextPrimary,
                Text = @"PROYECTO: Pantallas_Sistema_facturación / Gestor de Inventario y Facturación

1. OBJETIVO DE LA PRÁCTICA:
Desarrollar un aplicativo de escritorio con un diseño gráfico moderno aplicando estándares de nomenclatura para código y controles visuales (prefijos txt, btn, cbo, dgv, dtp, errProvider).

2. CARACTERÍSTICAS TÉCNICAS:
• Navegación moderna sin formularios MDI mediante contenedor dinámico de paneles (Panel Container).
• Validación obligatoria de campos con el componente ErrorProvider en todos los formularios de alta y modificación.
• Módulo completo de Facturación con cálculo dinámico en tiempo real de Subtotal, Descuentos, IVA (19%) y Total.
• Módulo de Ayuda interactivo con navegador WebBrowser embebido conectado a la documentación oficial.
• Módulo de Reportes e Informes con filtros por rango de fechas, formatos resumidos/detallados y exportación a formato CSV.
• Administración completa de Tablas Maestras (Clientes, Productos, Categorías, Proveedores) y Seguridad (Empleados, Roles, Usuarios).

3. DESARROLLADOR / ESTUDIANTE:
• Institución Universitaria Pascual Bravo
• Medellín, Colombia — 2026"
            };
            mainCard.Controls.Add(txtDescripcionTecnica);

            y += 210;

            // Botón de salida
            var btnCerrar = UIHelper.CreatePrimaryButton("Aceptar / Continuar", new Size(200, 44), new Point(860, y));
            btnCerrar.Click += (s, e) => Close();
            mainCard.Controls.Add(btnCerrar);

            Controls.Add(mainCard);
            ResumeLayout();
        }
    }
}
