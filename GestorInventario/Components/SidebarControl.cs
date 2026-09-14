using System;
using System.Collections.Generic;
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

        // Referencias a los controles ya creados, para poder resaltar el ítem
        // activo sin destruir y reconstruir todo el menú en cada navegación.
        private readonly Dictionary<string, (Panel Panel, Label Text)> _navItems = new();

        private readonly (string Category, (string Icon, string Label)[] Items)[] _menuGroups =
        {
            ("PRINCIPAL", new[]
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
                ("👤", "Seguridad"),
                ("ℹ️", "Acerca de")
            })
        };

        public string ActiveItem
        {
            get => _activeItem;
            set => SetActiveItem(value);
        }

        public SidebarControl()
        {
            Width = 240;
            BackColor = AppColors.Sidebar;
            Dock = DockStyle.Left;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BuildSidebar();
        }

        private void SetActiveItem(string label)
        {
            if (_activeItem == label) return;
            string previous = _activeItem;
            _activeItem = label;
            ApplyItemStyle(previous);
            ApplyItemStyle(label);
        }

        private void ApplyItemStyle(string label)
        {
            if (!_navItems.TryGetValue(label, out var item)) return;
            bool isActive = label == _activeItem;
            item.Panel.BackColor = isActive ? AppColors.Primary : AppColors.Sidebar;
            item.Text.Font = isActive ? new Font("Segoe UI", 9f, FontStyle.Bold) : new Font("Segoe UI", 9f);
            item.Text.ForeColor = isActive ? Color.White : Color.FromArgb(205, 255, 255, 255);
            item.Panel.Invalidate();
        }

        private void BuildSidebar()
        {
            SuspendLayout();

            // ── Logo Header ───────────────────────────────────────────
            var logoPanel = new Panel { Location = new Point(0, 0), Size = new Size(240, 64), BackColor = Color.Transparent, Dock = DockStyle.Top };
            logoPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using var lb = new SolidBrush(AppColors.Primary);
                g.FillEllipse(lb, 14, 11, 36, 36);
                using var lf = new Font("Segoe UI", 15f, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("F", lf, Brushes.White, new RectangleF(14, 11, 36, 36), sf);

                using var nf = new Font("Segoe UI", 10f, FontStyle.Bold);
                g.DrawString("Facturación & Stock", nf, Brushes.White, new Point(60, 22));
            };
            Controls.Add(logoPanel);

            // ── Footer Panel (Logout & Versión) ────────────────────────
            var footerPanel = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.Transparent };
            footerPanel.Controls.Add(new Panel { Location = new Point(16, 0), Size = new Size(208, 1), BackColor = Color.FromArgb(40, 255, 255, 255) });
            footerPanel.Controls.Add(CreateLogoutItem(8));
            footerPanel.Controls.Add(new Label
            {
                Text = "MVP Gestor inventarios",
                Font = new Font("Segoe UI", 7f),
                ForeColor = Color.FromArgb(80, 255, 255, 255),
                Location = new Point(16, 56),
                AutoSize = true,
                BackColor = Color.Transparent
            });
            Controls.Add(footerPanel);

            // ── Contenedor del Menú (sin scroll: todo el contenido cabe
            //    siempre gracias al espaciado compacto de las filas) ─────
            var menuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Sidebar,
                Padding = new Padding(0, 4, 0, 4)
            };

            // Los primeros ~60px de este panel no se pintan al arrancar la
            // app (glitch de renderizado de WinForms al maximizar la
            // ventana); se deja ese margen como aire visual antes del menú
            // para que el encabezado y el primer ítem queden fuera de esa
            // franja y se vean siempre.
            int y = 65;
            foreach (var group in _menuGroups)
            {
                // Encabezado de Sección / Categoría
                var lblHeader = new Label
                {
                    Text = group.Category,
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(130, 255, 255, 255),
                    Location = new Point(18, y + 3),
                    AutoSize = true,
                    BackColor = AppColors.Sidebar
                };
                menuPanel.Controls.Add(lblHeader);
                y += 18;

                // Items de la categoría
                foreach (var (icon, label) in group.Items)
                {
                    var navItem = CreateNavItem(icon, label, y);
                    menuPanel.Controls.Add(navItem);
                    y += 32;
                }

                y += 4; // Espacio entre categorías
            }

            Controls.Add(menuPanel);
            ResumeLayout();
        }

        private Panel CreateNavItem(string icon, string label, int y)
        {
            var panel = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(218, 30),
                BackColor = AppColors.Sidebar,
                Cursor = Cursors.Hand,
                Tag = label
            };

            panel.Paint += (s, e) =>
            {
                if (label != _activeItem) return;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = UIHelper.RoundedRect(new Rectangle(0, 0, panel.Width, panel.Height), 8);
                using var brush = new SolidBrush(AppColors.Primary);
                g.FillPath(brush, path);
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 10.5f),
                ForeColor = Color.White,
                Location = new Point(8, 3),
                Size = new Size(24, 22),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblText = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(205, 255, 255, 255),
                Location = new Point(36, 5),
                Size = new Size(170, 20),
                BackColor = Color.Transparent
            };

            panel.Controls.Add(lblIcon);
            panel.Controls.Add(lblText);

            Action setHover = () => { if (label != _activeItem) panel.BackColor = Color.FromArgb(30, 255, 255, 255); };
            Action clearHover = () => { if (label != _activeItem) panel.BackColor = AppColors.Sidebar; };
            Action onClick = () => { SetActiveItem(label); NavigateTo?.Invoke(label); };

            panel.MouseEnter += (s, e) => setHover();
            panel.MouseLeave += (s, e) => clearHover();
            panel.Click += (s, e) => onClick();
            lblIcon.MouseEnter += (s, e) => setHover();
            lblIcon.MouseLeave += (s, e) => clearHover();
            lblIcon.Click += (s, e) => onClick();
            lblText.MouseEnter += (s, e) => setHover();
            lblText.MouseLeave += (s, e) => clearHover();
            lblText.Click += (s, e) => onClick();

            _navItems[label] = (panel, lblText);
            ApplyItemStyle(label);

            return panel;
        }

        private Panel CreateLogoutItem(int y)
        {
            var panel = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(218, 34),
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
