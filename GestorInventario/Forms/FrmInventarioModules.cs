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

            DashboardStats stats;
            try { stats = _dashService.ObtenerEstadisticas(); }
            catch { stats = new DashboardStats(); }

            // Summary cards
            var cards = new[]
            {
                ("🗄️", "Total en Inventario", stats.TotalProductos.ToString(),  AppColors.Primary),
                ("⚠️",  "Productos Críticos",  stats.ProductosCriticos.ToString(), AppColors.Danger),
                ("💰", "Valor Estimado",       $"${stats.ValorInventario:N0}",   AppColors.Success),
            };

            int cx = 24, cy = 24;
            foreach (var (icon, label, value, color) in cards)
            {
                var card = new CardPanel { Location = new Point(cx, cy), Size = new Size(260, 100) };
                var capIcon = icon;
                var capLabel = label;
                var capValue = value;
                var capColor = color;
                card.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using var ab = new SolidBrush(Color.FromArgb(20, capColor));
                    g.FillEllipse(ab, card.Width - 60, 14, 44, 44);
                    using var ef = new Font("Segoe UI Emoji", 18f);
                    g.DrawString(capIcon, ef, new SolidBrush(capColor), card.Width - 56, 17);
                    using var vf = new Font("Segoe UI", 22f, FontStyle.Bold);
                    g.DrawString(capValue, vf, new SolidBrush(AppColors.TextPrimary), new Point(16, 16));
                    using var lf = AppFonts.BodyBold;
                    g.DrawString(capLabel, lf, new SolidBrush(AppColors.TextSecondary), new Point(16, 56));
                };
                Controls.Add(card);
                cx += 276;
            }

            // Filter bar
            var lblFilter = new Label { Text = "Filtrar por estado:", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(24, 144), AutoSize = true, BackColor = Color.Transparent };
            Controls.Add(lblFilter);

            var cbFilter = new ComboBox { Location = new Point(160, 140), Size = new Size(180, 36), Font = AppFonts.Body, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cbFilter.Items.AddRange(new[] { "Todos", "Stock Crítico", "Stock Normal" });
            cbFilter.SelectedIndex = 0;
            cbFilter.SelectedIndexChanged += (s, e) => LoadInventario(cbFilter.SelectedItem?.ToString() ?? "Todos");
            Controls.Add(cbFilter);

            // Table
            var tableCard = new CardPanel { Location = new Point(24, 180), Size = new Size(1140, 480), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom };
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
            try
            {
                dgvInventario.Rows.Clear();
                var productos = filter == "Stock Crítico"
                    ? _service.ObtenerCriticos()
                    : _service.ObtenerTodos();

                foreach (var p in productos)
                {
                    bool critico = p.StockActual <= p.StockMinimo;
                    if (filter == "Stock Normal" && critico) continue;

                    string estado = critico ? "⚠️ Crítico" : "✅ Normal";
                    int r = dgvInventario.Rows.Add(
                        p.Codigo, p.Nombre, p.Categoria,
                        p.StockActual, p.StockMinimo,
                        $"${p.PrecioVenta:N0}", p.Proveedor, estado);

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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar inventario: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        private ComboBox cboTipo = null!;
        private DateTimePicker dtDesde = null!, dtHasta = null!;

        public FrmHistorialMovimientos()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            var filterCard = new CardPanel { Location = new Point(24, 16), Size = new Size(1140, 68), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right };

            var lblTipo = new Label { Text = "Tipo:", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(16, 22), AutoSize = true, BackColor = Color.Transparent };
            filterCard.Controls.Add(lblTipo);

            cboTipo = new ComboBox { Location = new Point(60, 18), Size = new Size(140, 36), Font = AppFonts.Body, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTipo.Items.AddRange(new[] { "Todos", "Entrada", "Salida" });
            cboTipo.SelectedIndex = 0;
            filterCard.Controls.Add(cboTipo);

            var lblDesde = new Label { Text = "Desde:", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(220, 22), AutoSize = true, BackColor = Color.Transparent };
            filterCard.Controls.Add(lblDesde);
            dtDesde = new DateTimePicker { Location = new Point(275, 18), Size = new Size(140, 36), Font = AppFonts.Body, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };
            filterCard.Controls.Add(dtDesde);

            var lblHasta = new Label { Text = "Hasta:", Font = AppFonts.BodyBold, ForeColor = AppColors.TextPrimary, Location = new Point(435, 22), AutoSize = true, BackColor = Color.Transparent };
            filterCard.Controls.Add(lblHasta);
            dtHasta = new DateTimePicker { Location = new Point(485, 18), Size = new Size(140, 36), Font = AppFonts.Body, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            filterCard.Controls.Add(dtHasta);

            var btnFiltrar = UIHelper.CreatePrimaryButton("🔍 Filtrar", new Size(105, 38), new Point(650, 15));
            btnFiltrar.Click += (s, e) => LoadData();
            filterCard.Controls.Add(btnFiltrar);

            var btnExportar = UIHelper.CreateSecondaryButton("📋 Exportar", new Size(110, 38), new Point(765, 15));
            btnExportar.Click += (s, e) => ExportarCSV();
            filterCard.Controls.Add(btnExportar);

            Controls.Add(filterCard);

            var tableCard = new CardPanel { Location = new Point(24, 96), Size = new Size(1140, 520), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom };
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

            LoadData();
            ResumeLayout();
        }

        private void LoadData()
        {
            try
            {
                dgvMovimientos.Rows.Clear();
                string tipoFiltro = cboTipo.SelectedItem?.ToString() ?? "Todos";

                // Filtrar por fechas usando el servicio real
                var movimientos = _service.Filtrar(
                    desde: dtDesde.Value.Date,
                    hasta: dtHasta.Value.Date.AddDays(1));

                foreach (var m in movimientos)
                {
                    // Filtro adicional por tipo entrada/salida
                    if (tipoFiltro == "Entrada" && m.TipoEntradaSalida != "entrada") continue;
                    if (tipoFiltro == "Salida" && m.TipoEntradaSalida != "salida") continue;

                    int r = dgvMovimientos.Rows.Add(
                        m.Fecha.ToString("dd/MM/yyyy HH:mm"),
                        m.TipoMovimiento,
                        m.NombreProducto,
                        m.Cantidad,
                        m.Proveedor,
                        m.Observacion,
                        m.NombreUsuario);

                    dgvMovimientos.Rows[r].Cells["Tipo"].Style.ForeColor =
                        m.TipoEntradaSalida == "entrada" ? AppColors.Success : AppColors.Danger;
                    dgvMovimientos.Rows[r].Cells["Tipo"].Style.Font = AppFonts.BodyBold;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportarCSV()
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV (*.csv)|*.csv",
                    FileName = $"historial_movimientos_{DateTime.Today:yyyyMMdd}.csv"
                };
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Fecha,Tipo,Producto,Cantidad,Proveedor,Observación,Usuario");

                foreach (DataGridViewRow row in dgvMovimientos.Rows)
                {
                    if (row.IsNewRow) continue;
                    sb.AppendLine(string.Join(",",
                        row.Cells["Fecha"].Value,
                        row.Cells["Tipo"].Value,
                        row.Cells["Producto"].Value,
                        row.Cells["Cantidad"].Value,
                        row.Cells["Proveedor"].Value,
                        row.Cells["Observacion"].Value,
                        row.Cells["Usuario"].Value));
                }

                System.IO.File.WriteAllText(sfd.FileName, sb.ToString(),
                    System.Text.Encoding.UTF8);
                MessageBox.Show("Historial exportado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // FrmAlertas
    // ═══════════════════════════════════════════════════════════════
    public class FrmAlertas : Form
    {
        private readonly AlertaService _alertService = new();
        private readonly ProductoService _prodService = new();
        private DataGridView dgv = null!;

        public FrmAlertas()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // Banner de advertencia
            var banner = new Panel { Location = new Point(24, 16), Size = new Size(1140, 72), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right, BackColor = Color.FromArgb(254, 243, 199) };
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
                using var sb2 = new SolidBrush(ColorTranslator.FromHtml("#B45309"));
                g.DrawString("Revisa y realiza pedidos a proveedores para reabastecer el inventario.", sf2, sb2, new Point(60, 36));
            };
            Controls.Add(banner);

            // Botones acción
            var btnResolver = UIHelper.CreatePrimaryButton("✅ Marcar Resuelta", new Size(160, 38), new Point(24, 100));
            btnResolver.Click += BtnResolver_Click;
            Controls.Add(btnResolver);

            var btnIgnorar = UIHelper.CreateSecondaryButton("🚫 Ignorar", new Size(110, 38), new Point(194, 100));
            btnIgnorar.Click += BtnIgnorar_Click;
            Controls.Add(btnIgnorar);

            var btnRefresh = UIHelper.CreateSecondaryButton("🔄 Actualizar", new Size(120, 38), new Point(314, 100));
            btnRefresh.Click += (s, e) => LoadAlertas();
            Controls.Add(btnRefresh);

            // Tabla
            var tableCard = new CardPanel { Location = new Point(24, 150), Size = new Size(1140, 480), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom };
            dgv = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgv);
            dgv.Columns.Add("AlertaId", "ID");
            dgv.Columns.Add("Producto", "Producto");
            dgv.Columns.Add("Codigo", "Código");
            dgv.Columns.Add("StockActual", "Stock Actual");
            dgv.Columns.Add("StockMinimo", "Stock Mínimo");
            dgv.Columns.Add("Faltantes", "Unidades Faltantes");
            dgv.Columns.Add("Estado", "Estado");
            dgv.Columns.Add("Fecha", "Generada");
            dgv.Columns["AlertaId"].Visible = false;
            tableCard.Controls.Add(dgv);
            Controls.Add(tableCard);

            LoadAlertas();
            ResumeLayout();
        }

        private void LoadAlertas()
        {
            try
            {
                dgv.Rows.Clear();
                var alertas = _alertService.ObtenerAlertas();

                if (alertas.Count == 0)
                {
                    // Si no hay alertas en BD, mostrar productos críticos directamente
                    foreach (var p in _prodService.ObtenerCriticos())
                    {
                        int r = dgv.Rows.Add(
                            0, p.Nombre, p.Codigo,
                            p.StockActual, p.StockMinimo,
                            p.StockMinimo - p.StockActual,
                            "Activa",
                            DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        dgv.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Danger;
                        dgv.Rows[r].Cells["Estado"].Style.Font = AppFonts.BodyBold;
                        dgv.Rows[r].DefaultCellStyle.BackColor = Color.FromArgb(10, 239, 68, 68);
                    }
                    return;
                }

                foreach (var a in alertas)
                {
                    int r = dgv.Rows.Add(
                        a.Id,
                        a.NombreProducto,
                        a.CodigoProducto,
                        a.StockActual,
                        a.StockMinimo,
                        a.UnidadesFaltantes,
                        a.Estado,
                        a.CreadoEn.ToString("dd/MM/yyyy HH:mm"));

                    dgv.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Danger;
                    dgv.Rows[r].Cells["Estado"].Style.Font = AppFonts.BodyBold;
                    dgv.Rows[r].DefaultCellStyle.BackColor = Color.FromArgb(10, 239, 68, 68);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar alertas: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnResolver_Click(object? sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona una alerta.", "Selección requerida",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int alertaId = Convert.ToInt32(dgv.SelectedRows[0].Cells["AlertaId"].Value);
            if (alertaId == 0) { MessageBox.Show("Esta alerta no está registrada en BD.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            try
            {
                _alertService.Resolver(alertaId);
                MessageBox.Show("Alerta marcada como resuelta.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAlertas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnIgnorar_Click(object? sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona una alerta.", "Selección requerida",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int alertaId = Convert.ToInt32(dgv.SelectedRows[0].Cells["AlertaId"].Value);
            if (alertaId == 0) { MessageBox.Show("Esta alerta no está registrada en BD.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            try
            {
                _alertService.Ignorar(alertaId);
                MessageBox.Show("Alerta ignorada.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAlertas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}