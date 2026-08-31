using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GestorInventario.Helpers;

namespace GestorInventario.Components
{
    public class SidebarControl : UserControl
    {
        private string _activeItem = "Dashboard";
        public event Action<string>? NavigateTo;

        private readonly (string Category, (string Icon, string Label)[] Items)[] _menuGroups =
        {
            ("TABLAS", new[]
            {
                ("👥", "Clientes"),
                ("📦", "Productos"),
                ("🏷️", "Categorías"),
                ("🚚", "Proveedores")
            }),
            ("FACTURACIÓN", new[]
            {
                ("🧾", "Facturación"),
                ("📊", "Informes")
            }),
            ("INVENTARIO", new[]
            {
                ("🏠", "Dashboard"),
                ("📥", "Entradas"),
                ("📤", "Salidas"),
                ("🗄️", "Stock Actual"),
                ("🔔", "Alertas")
            }),
            ("SEGURIDAD", new[]
            {
                ("👨‍💼", "Empleados"),
                ("🛡️", "Roles"),
                ("👤", "Seguridad")
            }),
            ("AYUDA", new[]
            {
                ("🌐", "Ayuda Web"),
                ("ℹ️", "Acerca de")
            })
        };

        public string ActiveItem
        {
            get => _activeItem;
            set { _activeItem = value; BuildSidebar(); }
        }

        public SidebarControl()
        {
            Width = 240;
            BackColor = AppColors.Sidebar;
            Dock = DockStyle.Left;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BuildSidebar();
        }

        private void BuildSidebar()
        {
            SuspendLayout();
            Controls.Clear();

            // ── Logo Header ───────────────────────────────────────────
            var logoPanel = new Panel { Location = new Point(0, 0), Size = new Size(240, 75), BackColor = Color.Transparent, Dock = DockStyle.Top };
            logoPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using var lb = new SolidBrush(AppColors.Primary);
                g.FillEllipse(lb, 16, 16, 42, 42);
                using var lf = new Font("Segoe UI", 18f, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("F", lf, Brushes.White, new RectangleF(16, 16, 42, 42), sf);

                using var nf = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                g.DrawString("Facturación & Stock", nf, Brushes.White, new Point(66, 18));
                using var sf2 = new Font("Segoe UI", 7.5f);
                using var sb2 = new SolidBrush(Color.FromArgb(150, 255, 255, 255));
                g.DrawString("Pascual Bravo · Saber 2", sf2, sb2, new Point(67, 39));
            };
            Controls.Add(logoPanel);

            // ── Footer Panel (Logout & Versión) ────────────────────────
            var footerPanel = new Panel { Dock = DockStyle.Bottom, Height = 95, BackColor = Color.Transparent };
            footerPanel.Controls.Add(new Panel { Location = new Point(16, 0), Size = new Size(208, 1), BackColor = Color.FromArgb(40, 255, 255, 255) });
            footerPanel.Controls.Add(CreateLogoutItem(10));
            footerPanel.Controls.Add(new Label
            {
                Text = "v2.0.0 · Pantallas Facturación",
                Font = new Font("Segoe UI", 7f),
                ForeColor = Color.FromArgb(80, 255, 255, 255),
                Location = new Point(16, 68),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            Controls.Add(footerPanel);

            // ── Contenedor Scrollable para los Grupos de Menú ─────────
            var menuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 5, 0, 5)
            };

            int y = 5;
            foreach (var group in _menuGroups)
            {
                // Encabezado de Sección / Categoría
                var lblHeader = new Label
                {
                    Text = group.Category,
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(130, 255, 255, 255),
                    Location = new Point(18, y + 4),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                menuPanel.Controls.Add(lblHeader);
                y += 24;

                // Items de la categoría
                foreach (var (icon, label) in group.Items)
                {
                    menuPanel.Controls.Add(CreateNavItem(icon, label, y));
                    y += 38;
                }

                y += 6; // Espacio entre categorías
            }

            Controls.Add(menuPanel);
            ResumeLayout();
        }

        private Panel CreateNavItem(string icon, string label, int y)
        {
            bool isActive = label == _activeItem;

            var panel = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(218, 34),
                BackColor = isActive ? AppColors.Primary : Color.Transparent,
                Cursor = Cursors.Hand,
                Tag = label
            };

            if (isActive)
            {
                panel.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using var path = UIHelper.RoundedRect(new Rectangle(0, 0, panel.Width, panel.Height), 8);
                    using var brush = new SolidBrush(AppColors.Primary);
                    g.FillPath(brush, path);
                };
            }

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 11.5f),
                ForeColor = Color.White,
                Location = new Point(8, 5),
                Size = new Size(24, 22),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblText = new Label
            {
                Text = label,
                Font = isActive ? new Font("Segoe UI", 9f, FontStyle.Bold) : new Font("Segoe UI", 9f),
                ForeColor = isActive ? Color.White : Color.FromArgb(205, 255, 255, 255),
                Location = new Point(36, 7),
                Size = new Size(170, 20),
                BackColor = Color.Transparent
            };

            panel.Controls.Add(lblIcon);
            panel.Controls.Add(lblText);

            Action setHover = () => { if (label != _activeItem) panel.BackColor = Color.FromArgb(30, 255, 255, 255); };
            Action clearHover = () => { if (label != _activeItem) panel.BackColor = Color.Transparent; };
            Action onClick = () => { _activeItem = label; BuildSidebar(); NavigateTo?.Invoke(label); };

            panel.MouseEnter += (s, e) => setHover();
            panel.MouseLeave += (s, e) => clearHover();
            panel.Click += (s, e) => onClick();
            lblIcon.MouseEnter += (s, e) => setHover();
            lblIcon.MouseLeave += (s, e) => clearHover();
            lblIcon.Click += (s, e) => onClick();
            lblText.MouseEnter += (s, e) => setHover();
            lblText.MouseLeave += (s, e) => clearHover();
            lblText.Click += (s, e) => onClick();

            return panel;
        }

        private Panel CreateLogoutItem(int y)
        {
            var panel = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(218, 36),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            var lblIcon = new Label { Text = "🚪", Font = new Font("Segoe UI Emoji", 12f), ForeColor = Color.FromArgb(220, AppColors.Danger), Location = new Point(8, 6), Size = new Size(24, 22), BackColor = Color.Transparent };
            var lblText = new Label { Text = "Cerrar sesión", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(220, AppColors.Danger), Location = new Point(36, 8), Size = new Size(170, 20), BackColor = Color.Transparent };

            panel.Controls.Add(lblIcon);
            panel.Controls.Add(lblText);

            Action onClick = () => NavigateTo?.Invoke("Logout");
            Action setH = () => panel.BackColor = Color.FromArgb(30, 239, 68, 68);
            Action clrH = () => panel.BackColor = Color.Transparent;

            panel.MouseEnter += (s, e) => setH(); panel.MouseLeave += (s, e) => clrH(); panel.Click += (s, e) => onClick();
            lblIcon.MouseEnter += (s, e) => setH(); lblIcon.MouseLeave += (s, e) => clrH(); lblIcon.Click += (s, e) => onClick();
            lblText.MouseEnter += (s, e) => setH(); lblText.MouseLeave += (s, e) => clrH(); lblText.Click += (s, e) => onClick();

            return panel;
        }
    }
}
