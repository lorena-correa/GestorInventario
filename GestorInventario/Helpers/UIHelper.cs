using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GestorInventario.Helpers
{
    public static class UIHelper
    {
        // ── Rounded Rectangle ──────────────────────────────────────────
        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ── Style TextBox ───────────────────────────────────────────────
        public static void StyleTextBox(TextBox txt)
        {
            txt.Font = AppFonts.Body;
            txt.ForeColor = AppColors.TextPrimary;
            txt.BackColor = Color.White;
            txt.BorderStyle = BorderStyle.FixedSingle;
        }

        // ── Style ComboBox ──────────────────────────────────────────────
        public static void StyleComboBox(ComboBox cbo)
        {
            cbo.Font = AppFonts.Body;
            cbo.ForeColor = AppColors.TextPrimary;
            cbo.BackColor = Color.White;
            cbo.FlatStyle = FlatStyle.Flat;
            cbo.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        // ── Style DataGridView ──────────────────────────────────────────
        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = AppColors.Border;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Column headers
            dgv.ColumnHeadersDefaultCellStyle.BackColor = AppColors.BackgroundGeneral;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextSecondary;
            dgv.ColumnHeadersDefaultCellStyle.Font = AppFonts.SmallBold;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeight = 40;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Rows
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgv.DefaultCellStyle.Font = AppFonts.Body;
            dgv.DefaultCellStyle.SelectionBackColor = AppColors.PrimaryLight;
            dgv.DefaultCellStyle.SelectionForeColor = AppColors.Primary;
            dgv.DefaultCellStyle.Padding = new Padding(8, 6, 0, 6);
            dgv.RowTemplate.Height = 44;

            // Alternating rows
            dgv.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FAFAFA");
        }

        // ── Create Label ────────────────────────────────────────────────
        public static Label CreateLabel(string text, Font font, Color color, Point location, Size? size = null)
        {
            var lbl = new Label
            {
                Text = text,
                Font = font,
                ForeColor = color,
                Location = location,
                AutoSize = size == null,
                BackColor = Color.Transparent
            };
            if (size.HasValue) lbl.Size = size.Value;
            return lbl;
        }

        // ── Create Primary Button ───────────────────────────────────────
        public static Components.RoundedButton CreatePrimaryButton(string text, Size size, Point location)
        {
            return new Components.RoundedButton
            {
                Text = text,
                Size = size,
                Location = location,
                ButtonColor = AppColors.Primary,
                HoverColor = AppColors.PrimaryHover,
                TextColor = Color.White,
                Font = AppFonts.Button,
                Radius = 8
            };
        }

        // ── Create Secondary Button ─────────────────────────────────────
        public static Components.RoundedButton CreateSecondaryButton(string text, Size size, Point location)
        {
            return new Components.RoundedButton
            {
                Text = text,
                Size = size,
                Location = location,
                ButtonColor = Color.White,
                HoverColor = AppColors.BackgroundGeneral,
                TextColor = AppColors.TextSecondary,
                BorderColor = AppColors.Border,
                Font = AppFonts.Button,
                Radius = 8
            };
        }

        // ── Create Danger Button ────────────────────────────────────────
        public static Components.RoundedButton CreateDangerButton(string text, Size size, Point location)
        {
            return new Components.RoundedButton
            {
                Text = text,
                Size = size,
                Location = location,
                ButtonColor = AppColors.Danger,
                HoverColor = Color.FromArgb(220, 38, 38),
                TextColor = Color.White,
                Font = AppFonts.Button,
                Radius = 8
            };
        }

        // ── Create Card Panel ───────────────────────────────────────────
        public static Panel CreateCard(Point location, Size size)
        {
            return new Components.CardPanel
            {
                Location = location,
                Size = size,
                BackColor = Color.White
            };
        }

        // ── Draw Shadow ─────────────────────────────────────────────────
        public static void DrawShadow(Graphics g, Rectangle rect, int radius, Color shadowColor, int blur = 4)
        {
            for (int i = blur; i >= 1; i--)
            {
                using var brush = new SolidBrush(Color.FromArgb(15 / i, shadowColor));
                var shadowRect = new Rectangle(rect.X - i, rect.Y - i + 2, rect.Width + i * 2, rect.Height + i * 2);
                using var path = RoundedRect(shadowRect, radius + i);
                g.FillPath(brush, path);
            }
        }
    }
}
