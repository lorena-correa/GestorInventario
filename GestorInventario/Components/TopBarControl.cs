using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GestorInventario.Helpers;

namespace GestorInventario.Components
{
    public class TopBarControl : UserControl
    {
        private string _moduleName = "Dashboard";
        private string _userName = "Administrador";
        private int _notificationCount = 3;

        public string ModuleName { get => _moduleName; set { _moduleName = value; RebuildTopBar(); } }
        public string UserName { get => _userName; set { _userName = value; RebuildTopBar(); } }
        public int NotificationCount { get => _notificationCount; set { _notificationCount = value; RebuildTopBar(); } }

        public event Action? NotificationsClicked;

        public TopBarControl()
        {
            Height = 64;
            Dock = DockStyle.Top;
            BackColor = AppColors.TopBar;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            RebuildTopBar();
        }

        private void RebuildTopBar()
        {
            SuspendLayout();
            Controls.Clear();

            // Nombre del módulo
            Controls.Add(new Label
            {
                Text = _moduleName,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(24, 16),
                AutoSize = true,
                BackColor = Color.Transparent
            });

            // ── Contenedor derecho para Notificaciones y Usuario ─────────────
            var rightContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(Width - 250, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Height = 44
            };

            // ── Panel usuario (A la extrema derecha) ───────────────────
            var userPanel = new Panel
            {
                Size = new Size(170, 40),
                BackColor = AppColors.BackgroundGeneral,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 16, 0)
            };

            userPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, 169, 39);
                using var path = UIHelper.RoundedRect(rect, 8);
                using var brush = new SolidBrush(Color.White);
                g.FillPath(brush, path);
                using var pen = new Pen(AppColors.Border, 1f);
                g.DrawPath(pen, path);
            };

            // Avatar círculo
            var avatarPanel = new Panel
            {
                Location = new Point(8, 6),
                Size = new Size(28, 28),
                BackColor = Color.Transparent
            };
            avatarPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(AppColors.Primary);
                g.FillEllipse(brush, 0, 0, 27, 27);
                string initial = _userName.Length > 0 ? _userName[0].ToString().ToUpper() : "U";
                using var f = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(initial, f, Brushes.White, new RectangleF(0, 0, 27, 27), sf);
            };
            userPanel.Controls.Add(avatarPanel);

            // Nombre usuario
            userPanel.Controls.Add(new Label
            {
                Text = _userName,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(40, 6),
                Size = new Size(122, 16),
                BackColor = Color.Transparent
            });

            // Rol
            userPanel.Controls.Add(new Label
            {
                Text = Config.Session.Role.Length > 0 ? Config.Session.Role : "Administrador",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(40, 22),
                Size = new Size(122, 14),
                BackColor = Color.Transparent
            });

            rightContainer.Controls.Add(userPanel);

            // ── Panel de notificaciones (A la izquierda del usuario) ─────
            var notifPanel = new Panel
            {
                Size = new Size(40, 40),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0)
            };

            // Borde redondeado del botón notif
            notifPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, 39, 39);
                using var path = UIHelper.RoundedRect(rect, 8);
                using var brush = new SolidBrush(Color.White);
                g.FillPath(brush, path);
                using var pen = new Pen(AppColors.Border, 1f);
                g.DrawPath(pen, path);

                // Badge
                if (_notificationCount > 0)
                {
                    using var badgeBrush = new SolidBrush(AppColors.Danger);
                    g.FillEllipse(badgeBrush, 24, 2, 14, 14);
                    using var bf = new Font("Segoe UI", 7f, FontStyle.Bold);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(_notificationCount.ToString(), bf, Brushes.White, new RectangleF(24, 2, 14, 14), sf);
                }
            };

            // Ícono campana como Label
            var notifIcon = new Label
            {
                Text = "🔔",
                Font = new Font("Segoe UI Emoji", 13f),
                Location = new Point(4, 6),
                Size = new Size(28, 28),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            notifPanel.Controls.Add(notifIcon);
            notifPanel.Click += (s, e) => NotificationsClicked?.Invoke();
            notifIcon.Click += (s, e) => NotificationsClicked?.Invoke();

            rightContainer.Controls.Add(notifPanel);
            Controls.Add(rightContainer);

            ResumeLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Línea inferior
            using var pen = new Pen(AppColors.TopBarBorder, 1);
            e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RebuildTopBar();
        }
    }
}
