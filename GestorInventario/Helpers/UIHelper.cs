using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GestorInventario.Helpers
{
    public static class UIHelper
    {
        // ── Responsive Binding: mantiene un control ajustado al tamaño del formulario ──
        // Los formularios de la app posicionan sus tarjetas con Location/Size fijos porque
        // el Anchor de WinForms calcula su referencia en el momento en que el control se
        // agrega al formulario (cuando este aún tiene su tamaño por defecto, antes de
        // insertarse en el panel de contenido). Por eso no basta con Anchor: hay que
        // recalcular el tamaño explícitamente cada vez que el formulario cambia de tamaño.
        //
        // IMPORTANTE: no se ejecuta de inmediato. Estos formularios se construyen
        // ANTES de insertarse en el panel de contenido principal (todavía tienen el
        // tamaño por defecto, pequeño, de WinForms). Si recalculáramos ya, el control
        // se encogería de golpe a ese tamaño provisional y descuadraría otros controles
        // anclados dentro de él (por eso el botón de acciones llegó a superponerse con
        // el título). Basta con esperar al primer Resize real, que ocurre apenas el
        // formulario se acopla (Dock = Fill) al panel de contenido, antes de mostrarse.
        public static void BindFillWidth(Form form, Control control, int rightMargin, int minWidth = 400)
        {
            form.Resize += (s, e) => control.Width = Math.Max(minWidth, form.ClientSize.Width - control.Left - rightMargin);
        }

        // Igual que BindFillWidth, pero el control se estira solo hasta el borde
        // izquierdo de OTRO control (por ejemplo, una descripción que no debe
        // invadir la fila de botones de acciones, sin importar cuánto se achique
        // esa fila de botones al redimensionar la ventana).
        public static void BindFillWidthUntil(Form form, Control control, Control stopBefore, int gap, int minWidth = 60)
        {
            form.Resize += (s, e) => control.Width = Math.Max(minWidth, stopBefore.Left - control.Left - gap);
        }

        public static void BindFillHeight(Form form, Control control, int bottomMargin, int minHeight = 150)
        {
            form.Resize += (s, e) => control.Height = Math.Max(minHeight, form.ClientSize.Height - control.Top - bottomMargin);
        }

        // ── Estado vacío para tablas ─────────────────────────────────────
        // Superpone un mensaje centrado sobre el DataGridView cuando no tiene
        // filas, en vez de dejarlo en blanco. Debe llamarse DESPUÉS de agregar
        // el grid a su contenedor (necesita conocer dgv.Parent).
        public static void BindEmptyState(DataGridView dgv, string message, string icon = "🗂️")
        {
            if (dgv.Parent == null) return;

            var placeholder = new Label
            {
                Text = $"{icon}  {message}",
                Font = AppFonts.Body,
                ForeColor = AppColors.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = dgv.RowCount == 0
            };
            dgv.Parent.Controls.Add(placeholder);
            placeholder.BringToFront();

            void UpdateVisibility() => placeholder.Visible = dgv.RowCount == 0;
            dgv.RowsAdded += (s, e) => UpdateVisibility();
            dgv.RowsRemoved += (s, e) => UpdateVisibility();
        }

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

        // ── Style DataGridView (Consistente en todo el sistema) ───────────
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
            dgv.EnableHeadersVisualStyles = false;

            // Column headers unificados en toda la aplicación
            dgv.ColumnHeadersDefaultCellStyle.BackColor = AppColors.TableHeader;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TableHeaderText;
            // Sin esto, WinForms usa un azul de sistema por defecto para el estado
            // "seleccionado" del encabezado (al pasar el mouse o hacer clic para
            // ordenar). Se iguala al color normal para que no cambie nunca.
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = AppColors.TableHeader;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = AppColors.TableHeaderText;
            dgv.ColumnHeadersDefaultCellStyle.Font = AppFonts.SmallBold;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 0, 0);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeight = 42;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Rows
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgv.DefaultCellStyle.Font = AppFonts.Body;
            dgv.DefaultCellStyle.SelectionBackColor = AppColors.PrimaryLight;
            dgv.DefaultCellStyle.SelectionForeColor = AppColors.Primary;
            dgv.DefaultCellStyle.Padding = new Padding(12, 6, 0, 6);
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.RowTemplate.Height = 44;

            // Alternating rows suaves
            dgv.AlternatingRowsDefaultCellStyle.BackColor = AppColors.TableRowAlt;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = AppColors.PrimaryLight;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = AppColors.Primary;
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

        // ── Create Primary Button (Radius 8px, Púrpura Brand) ───────────
        public static Components.RoundedButton CreatePrimaryButton(string text, Size size, Point location, int radius = 8)
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
                Radius = radius
            };
        }

        // ── Create Secondary Button (Radius 8px, Blanco con Borde Suave) ──
        // Para acciones neutras: cancelar, salir, actualizar/exportar, etc.
        public static Components.RoundedButton CreateSecondaryButton(string text, Size size, Point location, int radius = 8)
        {
            return new Components.RoundedButton
            {
                Text = text,
                Size = size,
                Location = location,
                ButtonColor = Color.White,
                HoverColor = AppColors.BackgroundGeneral,
                TextColor = AppColors.TextPrimary,
                BorderColor = AppColors.Border,
                Font = AppFonts.Button,
                Radius = radius
            };
        }

        // ── Create Edit Button (Radius 8px, misma gama morada que el primario) ──
        // Para acciones de editar/modificar/ver detalle: mismo tono que "Nuevo"
        // pero en versión suave (contorno), para diferenciar peso visual sin
        // salirse de la gama de colores del botón primario.
        public static Components.RoundedButton CreateEditButton(string text, Size size, Point location, int radius = 8)
        {
            return new Components.RoundedButton
            {
                Text = text,
                Size = size,
                Location = location,
                ButtonColor = Color.White,
                HoverColor = AppColors.PrimaryLight,
                TextColor = AppColors.Primary,
                BorderColor = AppColors.Primary,
                Font = AppFonts.Button,
                Radius = radius
            };
        }

        // ── Create Danger Button (Radius 8px, Rojo Alerta) ───────────────
        public static Components.RoundedButton CreateDangerButton(string text, Size size, Point location, int radius = 8)
        {
            return new Components.RoundedButton
            {
                Text = text,
                Size = size,
                Location = location,
                ButtonColor = AppColors.DangerButton,
                HoverColor = AppColors.DangerButtonHover,
                TextColor = Color.White,
                Font = AppFonts.Button,
                Radius = radius
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

        // ── Standardized Modal Header & Frame ───────────────────────────
        public static Panel CreateModalHeader(Form modal, string title, string icon = "")
        {
            modal.FormBorderStyle = FormBorderStyle.None;
            modal.BackColor = Color.White;
            modal.StartPosition = FormStartPosition.CenterParent;

            // Borde sutil (1px gris claro) alrededor del modal
            modal.Paint += (s, e) =>
            {
                using var borderPen = new Pen(AppColors.Border, 1.5f);
                e.Graphics.DrawRectangle(borderPen, 0, 0, modal.Width - 1, modal.Height - 1);
            };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = AppColors.ModalHeader
            };

            // Soporte para arrastrar modal desde la cabecera
            Point dragStart = Point.Empty;
            header.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) dragStart = e.Location; };
            header.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && dragStart != Point.Empty)
                {
                    modal.Left += e.X - dragStart.X;
                    modal.Top += e.Y - dragStart.Y;
                }
            };

            string fullTitle = string.IsNullOrEmpty(icon) ? title : $"{icon}  {title}";
            var lblTitle = new Label
            {
                Text = fullTitle,
                Font = AppFonts.SubHeading,
                ForeColor = Color.White,
                Location = new Point(24, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            lblTitle.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) dragStart = e.Location; };
            lblTitle.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && dragStart != Point.Empty)
                {
                    modal.Left += e.X - dragStart.X;
                    modal.Top += e.Y - dragStart.Y;
                }
            };
            header.Controls.Add(lblTitle);

            var btnClose = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Size = new Size(34, 34),
                Location = new Point(modal.Width - 46, 13),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(40, 255, 255, 255);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
            btnClose.Click += (s, e) => modal.Close();

            header.Controls.Add(btnClose);
            return header;
        }

        // ── Create Rounded Input Field (8px radius) ─────────────────────
        public static Panel CreateRoundedTextBox(Control parent, string labelText, out TextBox txt, int x, int y, int width, int height = 38, bool multiline = false, bool readOnly = false)
        {
            if (!string.IsNullOrEmpty(labelText))
            {
                var lbl = new Label
                {
                    Text = labelText,
                    Font = AppFonts.SmallBold,
                    ForeColor = AppColors.TextSecondary,
                    Location = new Point(x, y),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                parent.Controls.Add(lbl);
                y += 20;
            }

            var container = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.Transparent
            };

            bool isFocused = false;
            container.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, container.Width - 1, container.Height - 1);
                using var path = RoundedRect(rect, 8);
                using var bgBrush = new SolidBrush(readOnly ? AppColors.ReadOnlyBackground : Color.White);
                g.FillPath(bgBrush, path);

                Color borderCol = isFocused ? AppColors.BorderFocus : AppColors.Border;
                float borderW = isFocused ? 1.5f : 1f;
                using var pen = new Pen(borderCol, borderW);
                g.DrawPath(pen, path);
            };

            txt = new TextBox
            {
                Location = multiline ? new Point(10, 8) : new Point(10, (height - 20) / 2),
                Size = new Size(width - 20, multiline ? height - 16 : 22),
                Font = AppFonts.Body,
                ForeColor = AppColors.TextPrimary,
                BackColor = readOnly ? AppColors.ReadOnlyBackground : Color.White,
                BorderStyle = BorderStyle.None,
                Multiline = multiline,
                ReadOnly = readOnly
            };

            var localTxt = txt;
            txt.GotFocus += (s, e) => { isFocused = true; container.Invalidate(); };
            txt.LostFocus += (s, e) => { isFocused = false; container.Invalidate(); };

            container.Controls.Add(txt);
            parent.Controls.Add(container);
            return container;
        }

        // ── Create Rounded ComboBox (8px radius) ────────────────────────
        public static Panel CreateRoundedComboBox(Control parent, string labelText, out ComboBox cbo, int x, int y, int width, int height = 38)
        {
            if (!string.IsNullOrEmpty(labelText))
            {
                var lbl = new Label
                {
                    Text = labelText,
                    Font = AppFonts.SmallBold,
                    ForeColor = AppColors.TextSecondary,
                    Location = new Point(x, y),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                parent.Controls.Add(lbl);
                y += 20;
            }

            var container = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.Transparent
            };

            container.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, container.Width - 1, container.Height - 1);
                using var path = RoundedRect(rect, 8);
                using var bgBrush = new SolidBrush(Color.White);
                g.FillPath(bgBrush, path);
                using var pen = new Pen(AppColors.Border, 1f);
                g.DrawPath(pen, path);
            };

            cbo = new ComboBox
            {
                Location = new Point(8, (height - 24) / 2),
                Size = new Size(width - 16, 24),
                Font = AppFonts.Body,
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            container.Controls.Add(cbo);
            parent.Controls.Add(container);
            return container;
        }

        // ── Create Rounded DateTimePicker (8px radius) ──────────────────
        public static Panel CreateRoundedDateTimePicker(Control parent, string labelText, out DateTimePicker dtp, int x, int y, int width, int height = 38, DateTimePickerFormat format = DateTimePickerFormat.Short, bool showCheckBox = false)
        {
            if (!string.IsNullOrEmpty(labelText))
            {
                var lbl = new Label
                {
                    Text = labelText,
                    Font = AppFonts.SmallBold,
                    ForeColor = AppColors.TextSecondary,
                    Location = new Point(x, y),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                parent.Controls.Add(lbl);
                y += 20;
            }

            var container = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.Transparent
            };

            container.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, container.Width - 1, container.Height - 1);
                using var path = RoundedRect(rect, 8);
                using var bgBrush = new SolidBrush(Color.White);
                g.FillPath(bgBrush, path);
                using var pen = new Pen(AppColors.Border, 1f);
                g.DrawPath(pen, path);
            };

            dtp = new DateTimePicker
            {
                Location = new Point(6, (height - 24) / 2),
                Size = new Size(width - 12, 24),
                Font = AppFonts.Body,
                Format = format,
                ShowCheckBox = showCheckBox,
                CalendarForeColor = AppColors.TextPrimary,
                CalendarMonthBackground = Color.White,
                CalendarTitleBackColor = AppColors.Primary,
                CalendarTitleForeColor = Color.White,
                CalendarTrailingForeColor = AppColors.TextSecondary
            };

            container.Controls.Add(dtp);
            parent.Controls.Add(container);
            return container;
        }

        // ── Create Search Bar (Rounded 8px with Icon) ───────────────────
        public static Panel CreateSearchInput(Control parent, out TextBox txtSearch, int x, int y, int width, int height = 40, string placeholder = "Buscar...")
        {
            var container = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.Transparent
            };

            bool isFocused = false;
            container.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, container.Width - 1, container.Height - 1);
                using var path = RoundedRect(rect, 8);
                using var bgBrush = new SolidBrush(Color.White);
                g.FillPath(bgBrush, path);

                Color borderCol = isFocused ? AppColors.BorderFocus : AppColors.Border;
                using var pen = new Pen(borderCol, isFocused ? 1.5f : 1f);
                g.DrawPath(pen, path);
            };

            var lblIcon = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI Emoji", 10.5f),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(8, (height - 20) / 2),
                Size = new Size(22, 20),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            container.Controls.Add(lblIcon);

            txtSearch = new TextBox
            {
                Location = new Point(34, (height - 20) / 2),
                Size = new Size(width - 44, 20),
                Font = AppFonts.Body,
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                PlaceholderText = placeholder
            };

            var localTxt = txtSearch;
            txtSearch.GotFocus += (s, e) => { isFocused = true; container.Invalidate(); };
            txtSearch.LostFocus += (s, e) => { isFocused = false; container.Invalidate(); };

            container.Controls.Add(txtSearch);
            parent.Controls.Add(container);
            return container;
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
