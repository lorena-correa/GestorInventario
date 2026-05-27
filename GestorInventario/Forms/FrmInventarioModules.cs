using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;
using GestorInventario.Models;
using GestorInventario.Services;

namespace GestorInventario.Forms
{
    // ═══════════════════════════════════════════════════════════════
    // FrmInventario
    // ═══════════════════════════════════════════════════════════════
    public class FrmInventario : Form
    {
        private readonly ProductoService _service = new();
        private readonly DashboardService _dashService = new();
        private DataGridView dgvInventario = null!;

        public FrmInventario()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();
            var stats = _dashService.ObtenerEstadisticas();

            // Summary cards
            var cards = new[]
            {
                ("🗄️", "Total en Inventario", stats.TotalProductos.ToString(), AppColors.Primary),
                ("⚠️", "Productos Críticos", stats.ProductosCriticos.ToString(), AppColors.Danger),
                ("💰", "Valor Estimado", $"${stats.ValorInventario:N0}", AppColors.Success),
            };

            int cx = 24, cy = 24;
            foreach (var (icon, label, value, color) in cards)
            {
                var card = new CardPanel { Location = new Point(cx, cy), Size = new Size(260, 100) };
                card.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using var ab = new SolidBrush(Color.FromArgb(20, color));
                    g.FillEllipse(ab, card.Width - 60, 14, 44, 44);
                    using var ef = new Font("Segoe UI Emoji", 18f);
                    g.DrawString(icon, ef, new SolidBrush(color), card.Width - 56, 17);
                    using var vf = new Font("Segoe UI", 22f, FontStyle.Bold);
                    g.DrawString(value, vf, new SolidBrush(AppColors.TextPrimary), new Point(16, 16));
                    using var lf = AppFonts.BodyBold;
                    g.DrawString(label, lf, new SolidBrush(AppColors.TextSecondary), new Point(16, 56));
                };
                Controls.Add(card);
                cx += 276;
            }

            // Filter bar
            var lblFilter = new Label { Text = "Filtrar:", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(24, 144), AutoSize = true, BackColor = Color.Transparent };
            Controls.Add(lblFilter);

            var cbFilter = new ComboBox { Location = new Point(76, 140), Size = new Size(200, 36), Font = AppFonts.Body, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cbFilter.Items.AddRange(new[] { "Todos", "Stock Crítico", "Stock Normal", "Inactivos" });
            cbFilter.SelectedIndex = 0;
            cbFilter.SelectedIndexChanged += (s, e) => LoadInventario(cbFilter.SelectedItem?.ToString() ?? "Todos");
            Controls.Add(cbFilter);

            // Table
            var tableCard = new CardPanel { Location = new Point(24, 180), Size = new Size(1200, 500), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom };
            dgvInventario = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvInventario);
            dgvInventario.Columns.Add("Codigo", "Código");
            dgvInventario.Columns.Add("Nombre", "Producto");
            dgvInventario.Columns.Add("Categoria", "Categoría");
            dgvInventario.Columns.Add("StockActual", "Stock Actual");
            dgvInventario.Columns.Add("StockMin", "Stock Mín.");
            dgvInventario.Columns.Add("PrecioVenta", "Precio Venta");
            dgvInventario.Columns.Add("Proveedor", "Proveedor");
            dgvInventario.Columns.Add("Estado", "Estado Stock");
            tableCard.Controls.Add(dgvInventario);
            Controls.Add(tableCard);

            LoadInventario("Todos");
            ResumeLayout();
        }

        private void LoadInventario(string filter)
        {
            dgvInventario.Rows.Clear();
            foreach (var p in _service.ObtenerTodos())
            {
                bool critico = p.StockActual <= p.StockMinimo;
                if (filter == "Stock Crítico" && !critico) continue;
                if (filter == "Stock Normal" && critico) continue;
                if (filter == "Inactivos" && p.Activo) continue;

                string estado = critico ? "⚠️ Crítico" : "✅ Normal";
                int r = dgvInventario.Rows.Add(p.Codigo, p.Nombre, p.Categoria, p.StockActual, p.StockMinimo, $"${p.PrecioVenta:N0}", p.Proveedor, estado);
                if (critico)
                {
                    dgvInventario.Rows[r].Cells["StockActual"].Style.ForeColor = AppColors.Danger;
                    dgvInventario.Rows[r].Cells["StockActual"].Style.Font = AppFonts.BodyBold;
                    dgvInventario.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Danger;
                }
                else
                {
                    dgvInventario.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Success;
                }
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // FrmHistorialMovimientos
    // ═══════════════════════════════════════════════════════════════
    public class FrmHistorialMovimientos : Form
    {
        private readonly MovimientoService _service = new();
        private DataGridView dgvMovimientos = null!;

        public FrmHistorialMovimientos()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // Filter bar
            var filterPanel = new Panel { Location = new Point(24, 24), Size = new Size(1200, 52), BackColor = Color.Transparent };

            var lblTipo = new Label { Text = "Tipo:", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(0, 12), AutoSize = true, BackColor = Color.Transparent };
            filterPanel.Controls.Add(lblTipo);

            var cboTipo = new ComboBox { Location = new Point(44, 8), Size = new Size(150, 36), Font = AppFonts.Body, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTipo.Items.AddRange(new[] { "Todos", "Entrada", "Salida" });
            cboTipo.SelectedIndex = 0;
            cboTipo.SelectedIndexChanged += (s, e) => LoadData(cboTipo.SelectedItem?.ToString() ?? "Todos");
            filterPanel.Controls.Add(cboTipo);

            var lblDesde = new Label { Text = "Desde:", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(210, 12), AutoSize = true, BackColor = Color.Transparent };
            filterPanel.Controls.Add(lblDesde);
            var dtDesde = new DateTimePicker { Location = new Point(260, 8), Size = new Size(160, 36), Font = AppFonts.Body, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };
            filterPanel.Controls.Add(dtDesde);

            var lblHasta = new Label { Text = "Hasta:", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(432, 12), AutoSize = true, BackColor = Color.Transparent };
            filterPanel.Controls.Add(lblHasta);
            var dtHasta = new DateTimePicker { Location = new Point(484, 8), Size = new Size(160, 36), Font = AppFonts.Body, Format = DateTimePickerFormat.Short };
            filterPanel.Controls.Add(dtHasta);

            var btnFiltrar = UIHelper.CreatePrimaryButton("🔍  Filtrar", new Size(120, 40), new Point(660, 6));
            btnFiltrar.Click += (s, e) => LoadData(cboTipo.SelectedItem?.ToString() ?? "Todos");
            filterPanel.Controls.Add(btnFiltrar);

            Controls.Add(filterPanel);

            // Table
            var tableCard = new CardPanel { Location = new Point(24, 92), Size = new Size(1200, 520), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom };
            dgvMovimientos = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvMovimientos);
            dgvMovimientos.Columns.Add("Fecha", "Fecha");
            dgvMovimientos.Columns.Add("Tipo", "Tipo");
            dgvMovimientos.Columns.Add("Producto", "Producto");
            dgvMovimientos.Columns.Add("Cantidad", "Cantidad");
            dgvMovimientos.Columns.Add("Proveedor", "Proveedor");
            dgvMovimientos.Columns.Add("Observacion", "Observación");
            dgvMovimientos.Columns.Add("Usuario", "Usuario");
            tableCard.Controls.Add(dgvMovimientos);
            Controls.Add(tableCard);

            LoadData("Todos");
            ResumeLayout();
        }

        private void LoadData(string filter)
        {
            dgvMovimientos.Rows.Clear();
            foreach (var m in _service.ObtenerTodos())
            {
                if (filter != "Todos" && m.TipoMovimiento != filter) continue;
                int r = dgvMovimientos.Rows.Add(m.Fecha.ToString("dd/MM/yyyy HH:mm"), m.TipoMovimiento, m.NombreProducto, m.Cantidad, m.Proveedor, m.Observacion, m.NombreUsuario);
                dgvMovimientos.Rows[r].Cells["Tipo"].Style.ForeColor = m.TipoMovimiento == "Entrada" ? AppColors.Success : AppColors.Danger;
                dgvMovimientos.Rows[r].Cells["Tipo"].Style.Font = AppFonts.BodyBold;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // FrmAlertas
    // ═══════════════════════════════════════════════════════════════
    public class FrmAlertas : Form
    {
        private readonly AlertaService _service = new();
        private readonly ProductoService _prodService = new();

        public FrmAlertas()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // Alert banner
            var banner = new Panel { Location = new Point(24, 24), Size = new Size(1200, 72), BackColor = Color.FromArgb(254, 243, 199) };
            banner.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = UIHelper.RoundedRect(new Rectangle(0, 0, banner.Width - 1, banner.Height - 1), 10);
                using var bg = new SolidBrush(Color.FromArgb(254, 243, 199));
                g.FillPath(bg, path);
                using var border = new Pen(AppColors.Warning, 1);
                g.DrawPath(border, path);
                using var ef = new Font("Segoe UI Emoji", 22f);
                g.DrawString("⚠️", ef, Brushes.Black, new Point(16, 20));
                using var tf = AppFonts.BodyBold;
                using var tb = new SolidBrush(ColorTranslator.FromHtml("#92400E"));
                g.DrawString("Atención: Hay productos con stock por debajo del mínimo requerido.", tf, tb, new Point(60, 14));
                using var sf2 = AppFonts.Body;
                using var sb = new SolidBrush(ColorTranslator.FromHtml("#B45309"));
                g.DrawString("Revisa y realiza pedidos a proveedores para reabastecer el inventario.", sf2, sb, new Point(60, 36));
            };
            Controls.Add(banner);

            // Alerts table
            var tableCard = new CardPanel { Location = new Point(24, 116), Size = new Size(1200, 520), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom };
            var dgv = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgv);
            dgv.Columns.Add("Producto", "Producto");
            dgv.Columns.Add("StockActual", "Stock Actual");
            dgv.Columns.Add("StockMinimo", "Stock Mínimo");
            dgv.Columns.Add("Diferencia", "Diferencia");
            dgv.Columns.Add("Estado", "Estado");
            dgv.Columns.Add("Fecha", "Generada");

            foreach (var a in _service.ObtenerAlertas())
            {
                int diff = a.StockActual - a.StockMinimo;
                int r = dgv.Rows.Add(a.NombreProducto, a.StockActual, a.StockMinimo, diff, a.Estado, a.CreadoEn.ToString("dd/MM/yyyy HH:mm"));
                if (a.Estado == "Crítico")
                {
                    dgv.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Danger;
                    dgv.Rows[r].Cells["Estado"].Style.Font = AppFonts.BodyBold;
                    dgv.Rows[r].Cells["StockActual"].Style.ForeColor = AppColors.Danger;
                    dgv.Rows[r].DefaultCellStyle.BackColor = Color.FromArgb(10, 239, 68, 68);
                }
            }

            // Also show all critical products
            foreach (var p in _prodService.ObtenerTodos())
            {
                if (p.StockActual > p.StockMinimo) continue;
                // Only add if not already in alertas
                bool found = false;
                foreach (var a in _service.ObtenerAlertas())
                    if (a.NombreProducto == p.Nombre) { found = true; break; }
                if (found) continue;

                int diff = p.StockActual - p.StockMinimo;
                int r = dgv.Rows.Add(p.Nombre, p.StockActual, p.StockMinimo, diff, "Crítico", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                dgv.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Danger;
                dgv.Rows[r].Cells["Estado"].Style.Font = AppFonts.BodyBold;
                dgv.Rows[r].DefaultCellStyle.BackColor = Color.FromArgb(10, 239, 68, 68);
            }

            tableCard.Controls.Add(dgv);
            Controls.Add(tableCard);

            ResumeLayout();
        }
    }
}
