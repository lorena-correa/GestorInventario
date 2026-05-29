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
        private string _activeItem = "Inicio";
        public event Action<string>? NavigateTo;

        // ── LÍNEA NUEVA ────────────────────────────────────────────────
        private readonly HashSet<string> _hiddenItems = new();

        private readonly (string Icon, string Label)[] _items =
        {
            ("🏠", "Inicio"),
            ("📦", "Productos"),
            ("🚚", "Proveedores"),
            ("📥", "Entradas"),
            ("📤", "Salidas"),
            ("🗄️", "Inventario"),
            ("📊", "Reportes"),
            ("🔔", "Alertas"),
            ("👤", "Usuarios"),
            ("⚙️", "Configuración"),
        };

        public string ActiveItem
        {
            get => _activeItem;
            set { _activeItem = value; BuildSidebar(); }
        }

        // ── MÉTODO NUEVO ───────────────────────────────────────────────
        public void SetModuleVisible(string label, bool visible)
        {
            if (visible) _hiddenItems.Remove(label);
            else _hiddenItems.Add(label);
            BuildSidebar();
        }

        public SidebarControl()
        {
            Width = 220;
            BackColor = AppColors.Sidebar;
            Dock = DockStyle.Left;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BuildSidebar();
        }

        private void BuildSidebar()
        {
            SuspendLayout();
            Controls.Clear();

            var logoPanel = new Panel { Location = new Point(0, 0), Size = new Size(220, 80), BackColor = Color.Transparent };
            logoPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                using var lb = new SolidBrush(AppColors.Primary);
                g.FillEllipse(lb, 16, 20, 38, 38);
                using var lf = new Font("Segoe UI", 16f, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("G", lf, Brushes.White, new RectangleF(16, 20, 38, 38), sf);
                using var nf = new Font("Segoe UI", 10f, FontStyle.Bold);
                g.DrawString("GestorInventario", nf, Brushes.White, new Point(62, 22));
                using var sf2 = new Font("Segoe UI", 7.5f);
                using var sb2 = new SolidBrush(Color.FromArgb(150, 255, 255, 255));
                g.DrawString("Sistema de Inventario", sf2, sb2, new Point(63, 42));
            };
            Controls.Add(logoPanel);

            Controls.Add(new Panel { Location = new Point(16, 78), Size = new Size(188, 1), BackColor = Color.FromArgb(50, 255, 255, 255) });
            Controls.Add(new Label { Text = "MENÚ PRINCIPAL", Font = new Font("Segoe UI", 7f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 255, 255, 255), Location = new Point(20, 90), AutoSize = true, BackColor = Color.Transparent });

            int y = 110;
            foreach (var (icon, label) in _items)
            {
                // ── LÍNEA NUEVA: saltar ítems ocultos ──────────────────
                if (_hiddenItems.Contains(label)) continue;

                Controls.Add(CreateNavItem(icon, label, y));
                y += 46;
            }

            Controls.Add(new Panel { Location = new Point(16, y + 4), Size = new Size(188, 1), BackColor = Color.FromArgb(50, 255, 255, 255) });
            Controls.Add(CreateLogoutItem(y + 16));
            Controls.Add(new Label { Text = "v1.0.0 · MVP Universitario", Font = new Font("Segoe UI", 7f), ForeColor = Color.FromArgb(60, 255, 255, 255), Location = new Point(16, y + 70), AutoSize = true, BackColor = Color.Transparent });

            ResumeLayout();
        }

        private Panel CreateNavItem(string icon, string label, int y)
        {
            bool isActive = label == _activeItem;
            var panel = new Panel
            {
                Location = new Point(8, y),
                Size = new Size(204, 40),
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

            var lblIcon = new Label { Text = icon, Font = new Font("Segoe UI Emoji", 13f), ForeColor = Color.White, Location = new Point(10, 8), Size = new Size(28, 26), BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleCenter };
            var lblText = new Label { Text = label, Font = isActive ? new Font("Segoe UI", 9.5f, FontStyle.Bold) : new Font("Segoe UI", 9.5f), ForeColor = isActive ? Color.White : Color.FromArgb(200, 255, 255, 255), Location = new Point(44, 10), Size = new Size(150, 22), BackColor = Color.Transparent };

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
            var panel = new Panel { Location = new Point(8, y), Size = new Size(204, 40), BackColor = Color.Transparent, Cursor = Cursors.Hand };
            var lblIcon = new Label { Text = "🚪", Font = new Font("Segoe UI Emoji", 13f), ForeColor = Color.FromArgb(200, AppColors.Danger), Location = new Point(10, 8), Size = new Size(28, 26), BackColor = Color.Transparent };
            var lblText = new Label { Text = "Cerrar sesión", Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(200, AppColors.Danger), Location = new Point(44, 10), Size = new Size(150, 22), BackColor = Color.Transparent };

            panel.Controls.Add(lblIcon);
            panel.Controls.Add(lblText);

            Action onClick = () => NavigateTo?.Invoke("Logout");
            Action setH = () => panel.BackColor = Color.FromArgb(20, 239, 68, 68);
            Action clrH = () => panel.BackColor = Color.Transparent;

            panel.MouseEnter += (s, e) => setH(); panel.MouseLeave += (s, e) => clrH(); panel.Click += (s, e) => onClick();
            lblIcon.MouseEnter += (s, e) => setH(); lblIcon.MouseLeave += (s, e) => clrH(); lblIcon.Click += (s, e) => onClick();
            lblText.MouseEnter += (s, e) => setH(); lblText.MouseLeave += (s, e) => clrH(); lblText.Click += (s, e) => onClick();

            return panel;
        }
    }
}