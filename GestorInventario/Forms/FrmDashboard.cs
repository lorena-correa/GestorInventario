using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;
using GestorInventario.Models;
using GestorInventario.Services;

namespace GestorInventario.Forms
{
    public class FrmDashboard : Form
    {
        private readonly DashboardService _dashService = new();
        private readonly MovimientoService _movService = new();

        public FrmDashboard()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            AutoScroll = true;
            BuildUI();
        }

        private void BuildUI()
        {
            var stats = _dashService.ObtenerEstadisticas();
            SuspendLayout();

            // Saludo
            Controls.Add(new Label { Text = $"Buenos días, {Config.Session.UserName} 👋", Font = AppFonts.Heading, ForeColor = AppColors.TextPrimary, Location = new Point(24, 24), AutoSize = true, BackColor = Color.Transparent });
            Controls.Add(new Label { Text = DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("es-CO")), Font = AppFonts.Body, ForeColor = AppColors.TextSecondary, Location = new Point(24, 54), AutoSize = true, BackColor = Color.Transparent });

            // ── Metric cards ────────────────────────────────────────────
            var metrics = new[]
            {
                ("📦", "Total Productos",    stats.TotalProductos.ToString(),      AppColors.Primary,  "activos en sistema"),
                ("⚠️",  "Stock Crítico",      stats.ProductosCriticos.ToString(),   AppColors.Danger,   "requieren atención"),
                ("🔄", "Movimientos Hoy",    stats.MovimientosHoy.ToString(),      AppColors.Success,  "entradas y salidas"),
                ("🔔", "Alertas Activas",    stats.AlertasActivas.ToString(),      AppColors.Warning,  "pendientes"),
                ("🚚", "Proveedores",        stats.TotalProveedores.ToString(),    AppColors.Info,     "registrados"),
                ("💰", "Valor Inventario",   $"${stats.ValorInventario:N0}",       ColorTranslator.FromHtml("#8B5CF6"), "COP estimado"),
            };

            int cardW = 230, cardH = 120, cx = 24, cy = 88, gap = 16;
            foreach (var (icon, label, value, color, sub) in metrics)
            {
                Controls.Add(CreateMetricCard(icon, label, value, color, sub, new Point(cx, cy), new Size(cardW, cardH)));
                cx += cardW + gap;
                if (cx + cardW > 1350) { cx = 24; cy += cardH + gap; }
            }

            int sectionY = cy + cardH + 28;

            // ── Título tabla ────────────────────────────────────────────
            Controls.Add(new Label { Text = "Movimientos Recientes", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(24, sectionY), AutoSize = true, BackColor = Color.Transparent });

            // ── Tabla movimientos ───────────────────────────────────────
            var tableCard = new CardPanel { Location = new Point(24, sectionY + 30), Size = new Size(740, 240) };
            var dgv = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgv);
            dgv.Columns.Add("Producto", "Producto");
            dgv.Columns.Add("Tipo", "Tipo");
            dgv.Columns.Add("Cantidad", "Cantidad");
            dgv.Columns.Add("Usuario", "Usuario");
            dgv.Columns.Add("Fecha", "Fecha");
            foreach (var m in _movService.ObtenerTodos())
            {
                int r = dgv.Rows.Add(m.Producto, m.TipoMovimiento, m.Cantidad, m.Usuario, m.Fecha.ToString("dd/MM/yyyy HH:mm"));
                dgv.Rows[r].Cells["Tipo"].Style.ForeColor = m.TipoMovimiento == "Entrada" ? AppColors.Success : AppColors.Danger;
            }
            tableCard.Controls.Add(dgv);
            Controls.Add(tableCard);

            // ── Panel estado ────────────────────────────────────────────
            Controls.Add(new Label { Text = "Estado del Inventario", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(780, sectionY), AutoSize = true, BackColor = Color.Transparent });
            var alertCard = new CardPanel { Location = new Point(780, sectionY + 30), Size = new Size(360, 240) };
            BuildAlertsSummary(alertCard, stats);
            Controls.Add(alertCard);

            ResumeLayout();
        }

        private Panel CreateMetricCard(string icon, string label, string value, Color accentColor, string sub, Point location, Size size)
        {
            var card = new Panel
            {
                Location = location,
                Size = size,
                BackColor = Color.White,
                Cursor = Cursors.Default
            };

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Fondo blanco redondeado con sombra
                var rect = new Rectangle(2, 2, card.Width - 5, card.Height - 5);
                for (int i = 3; i >= 1; i--)
                {
                    var sr = new Rectangle(rect.X - 1, rect.Y + i, rect.Width + 2, rect.Height);
                    using var sp = UIHelper.RoundedRect(sr, 10);
                    using var sb = new SolidBrush(Color.FromArgb(8 * i, 100, 100, 120));
                    g.FillPath(sb, sp);
                }
                using var bgPath = UIHelper.RoundedRect(rect, 10);
                g.FillPath(Brushes.White, bgPath);
                using var borderPen = new Pen(AppColors.Border, 1f);
                g.DrawPath(borderPen, bgPath);

                // Línea de acento izquierda
                using var accentPath = UIHelper.RoundedRect(new Rectangle(4, 16, 4, card.Height - 32), 2);
                using var accentBrush = new SolidBrush(accentColor);
                g.FillPath(accentBrush, accentPath);

                // Ícono fondo
                using var iconBgBrush = new SolidBrush(Color.FromArgb(20, accentColor));
                g.FillEllipse(iconBgBrush, card.Width - 58, 14, 40, 40);

                // Ícono emoji
                try
                {
                    using var iconFont = new Font("Segoe UI Emoji", 14f);
                    g.DrawString(icon, iconFont, new SolidBrush(accentColor), card.Width - 54, 18);
                }
                catch { }

                // Valor numérico grande
                using var valFont = new Font("Segoe UI", 26f, FontStyle.Bold);
                using var valBrush = new SolidBrush(AppColors.TextPrimary);
                g.DrawString(value, valFont, valBrush, new Point(20, 18));

                // Label
                using var lblFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                using var lblBrush = new SolidBrush(AppColors.TextPrimary);
                g.DrawString(label, lblFont, lblBrush, new Point(20, 70));

                // Sub-texto
                using var subFont = new Font("Segoe UI", 8.5f);
                using var subBrush = new SolidBrush(AppColors.TextSecondary);
                g.DrawString(sub, subFont, subBrush, new Point(20, 90));
            };

            return card;
        }

        private void BuildAlertsSummary(CardPanel card, DashboardStats stats)
        {
            int y = 14;
            var items = new[]
            {
                ($"📦 {stats.TotalProductos} productos registrados",      AppColors.TextPrimary),
                ($"⚠️  {stats.ProductosCriticos} con stock crítico",       AppColors.Danger),
                ($"🔔 {stats.AlertasActivas} alertas activas",             AppColors.Warning),
                ($"🚚 {stats.TotalProveedores} proveedores activos",        AppColors.TextPrimary),
                ($"🔄 {stats.MovimientosHoy} movimientos hoy",             AppColors.Success),
            };
            foreach (var (text, fg) in items)
            {
                card.Controls.Add(new Label { Text = text, Font = AppFonts.Body, ForeColor = fg, Location = new Point(16, y), Size = new Size(328, 30), BackColor = Color.Transparent });
                y += 36;
            }

            // Barra de salud
            int pct = stats.ProductosCriticos == 0 ? 100 : Math.Max(0, (int)(100 - stats.ProductosCriticos * 100.0 / Math.Max(1, stats.TotalProductos)));
            var barBg = new Panel { Location = new Point(16, y + 8), Size = new Size(328, 8), BackColor = AppColors.Border };
            var barFill = new Panel { Location = new Point(0, 0), Size = new Size(328 * pct / 100, 8), BackColor = pct > 70 ? AppColors.Success : pct > 40 ? AppColors.Warning : AppColors.Danger };
            barBg.Controls.Add(barFill);
            card.Controls.Add(barBg);
            card.Controls.Add(new Label { Text = $"Salud del inventario: {pct}%", Font = AppFonts.Small, ForeColor = AppColors.TextSecondary, Location = new Point(16, y + 22), AutoSize = true, BackColor = Color.Transparent });
        }
    }
}
