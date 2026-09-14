using System;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;

namespace GestorInventario.Forms
{
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
                g.DrawString("Aplicación de Escritorio en C# .NET 8 con Diseño Gráfico Moderno y bonito", fontSub, brushSub, new Point(122, 68));
            };
            mainCard.Controls.Add(banner);

            // Contenedor de Información
            int y = 140;

            // Fila 1: Cards de Metadatos
            var cardMeta1 = new CardPanel { Location = new Point(30, y), Size = new Size(330, 110), BackColor = AppColors.SurfaceMuted };
            cardMeta1.Controls.Add(new Label { Text = "🏛️  INSTITUCIÓN", Font = AppFonts.SmallBold, ForeColor = AppColors.Primary, Location = new Point(15, 12), AutoSize = true });
            cardMeta1.Controls.Add(new Label { Text = "Institución Universitaria Pascual Bravo", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(15, 36), AutoSize = true });
            cardMeta1.Controls.Add(new Label { Text = "Facultad de Ingeniería\nTecnología en Desarrollo de Software", Font = AppFonts.Small, ForeColor = AppColors.TextSecondary, Location = new Point(15, 60), AutoSize = true });
            mainCard.Controls.Add(cardMeta1);

            var cardMeta2 = new CardPanel { Location = new Point(380, y), Size = new Size(330, 110), BackColor = AppColors.SurfaceMuted };
            cardMeta2.Controls.Add(new Label { Text = "📚  ASIGNATURA", Font = AppFonts.SmallBold, ForeColor = AppColors.Primary, Location = new Point(15, 12), AutoSize = true });
            cardMeta2.Controls.Add(new Label { Text = "Herramientas de programación III", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(15, 36), AutoSize = true });
            cardMeta2.Controls.Add(new Label { Text = "Código: ET0056\nUnidad: Saber 2 (Diseño UI en C#)", Font = AppFonts.Small, ForeColor = AppColors.TextSecondary, Location = new Point(15, 60), AutoSize = true });
            mainCard.Controls.Add(cardMeta2);

            var cardMeta3 = new CardPanel { Location = new Point(730, y), Size = new Size(330, 110), BackColor = AppColors.SurfaceMuted };
            cardMeta3.Controls.Add(new Label { Text = "💻  ARQUITECTURA Y VERSIÓN", Font = AppFonts.SmallBold, ForeColor = AppColors.Primary, Location = new Point(15, 12), AutoSize = true });
            cardMeta3.Controls.Add(new Label { Text = "Versión: 3.0.0 Proyecto universitario\n— MVP Control de Inventario", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(15, 36), AutoSize = true });
            cardMeta3.Controls.Add(new Label { Text = "Framework: C# .NET 8 WinForms", Font = AppFonts.Small, ForeColor = AppColors.TextSecondary, Location = new Point(15, 60), AutoSize = true });
            mainCard.Controls.Add(cardMeta3);

            y += 125;

            // TextBox con propiedad Multiline = true según lo exigido por la guía
            var lblDetalles = new Label
            {
                Text = "📝  DESCRIPCIÓN TÉCNICA Y ESTÁNDARES IMPLEMENTADOS:",
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
                BackColor = AppColors.ReadOnlyBackground,
                ForeColor = AppColors.TextPrimary,
                Text = @"Sistema Integrado de Facturación y Control de Inventario, desarrollado como solución de escritorio en entorno modular. Permite gestionar eficientemente las operaciones comerciales mediante una interfaz moderna sin contenedores MDI, integrando la administración de tablas maestras (Clientes, Productos y Categorías), un completo módulo de facturación con cálculo automatizado en tiempo real, control de seguridad por roles y usuarios, y un área de informes gerenciales. Cuenta con validaciones estrictas de datos en todos sus formularios para garantizar la integridad de la información."
            };
            mainCard.Controls.Add(txtDescripcionTecnica);

            y += 210;

            // Botón de salida
            var btnCerrar = UIHelper.CreatePrimaryButton("Aceptar / Continuar", new Size(200, 44), new Point(860, y));
            btnCerrar.Click += (s, e) => Close();
            mainCard.Controls.Add(btnCerrar);

            Controls.Add(mainCard);

            UIHelper.BindFillWidth(this, mainCard, 40);
            UIHelper.BindFillHeight(this, mainCard, 24);

            ResumeLayout();
        }
    }
}
