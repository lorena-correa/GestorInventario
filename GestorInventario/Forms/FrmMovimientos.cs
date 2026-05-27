using System;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;
using GestorInventario.Models;
using GestorInventario.Services;

namespace GestorInventario.Forms
{
    // ═══════════════════════════════════════════════════════════════
    // FrmEntradaInventario
    // ═══════════════════════════════════════════════════════════════
    public class FrmEntradaInventario : Form
    {
        private ComboBox cboProducto = null!, cboProveedor = null!;
        private TextBox txtCantidad = null!, txtObservacion = null!;
        private DateTimePicker dtpFecha = null!;
        private readonly MovimientoService _movService = new();
        private readonly ProductoService _prodService = new();
        private readonly ProveedorService _provService = new();
        private DataGridView dgvRecientes = null!;

        public FrmEntradaInventario()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
            LoadRecentes();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // ── Form card ──────────────────────────────────────────────
            var formCard = new CardPanel { Location = new Point(24, 24), Size = new Size(560, 500) };
            Controls.Add(formCard);

            var lblFormTitle = new Label { Text = "📥  Registrar Entrada", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(20, 16), AutoSize = true, BackColor = Color.Transparent };
            formCard.Controls.Add(lblFormTitle);

            var divider = new Panel { Location = new Point(20, 46), Size = new Size(520, 1), BackColor = AppColors.Border };
            formCard.Controls.Add(divider);

            int x = 20, y = 60, w = 510;

            AddComboField(formCard, "PRODUCTO *", out cboProducto, x, y, w);
            foreach (var p in _prodService.ObtenerTodos()) cboProducto.Items.Add(p.Nombre);
            y += 76;

            AddComboField(formCard, "PROVEEDOR *", out cboProveedor, x, y, w);
            foreach (var p in _provService.ObtenerTodos()) cboProveedor.Items.Add(p.Nombre);
            y += 76;

            AddTextField(formCard, "CANTIDAD *", out txtCantidad, x, y, 240);
            // Fecha
            var lblF = new Label { Text = "FECHA *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x + 260, y), AutoSize = true, BackColor = Color.Transparent };
            dtpFecha = new DateTimePicker { Location = new Point(x + 260, y + 22), Size = new Size(250, 36), Font = AppFonts.Body, Format = DateTimePickerFormat.Short };
            formCard.Controls.Add(lblF);
            formCard.Controls.Add(dtpFecha);
            y += 76;

            AddTextField(formCard, "OBSERVACIÓN", out txtObservacion, x, y, w, multiline: true);
            txtObservacion.Height = 72;
            y += 108;

            var btnGuardar = UIHelper.CreatePrimaryButton("💾  Registrar Entrada", new Size(200, 44), new Point(x, y));
            btnGuardar.Click += BtnGuardar_Click;
            formCard.Controls.Add(btnGuardar);

            var btnLimpiar = UIHelper.CreateSecondaryButton("🗑  Limpiar", new Size(120, 44), new Point(x + 210, y));
            btnLimpiar.Click += (s, e) => Limpiar();
            formCard.Controls.Add(btnLimpiar);

            // ── Recentes table ─────────────────────────────────────────
            var lblRecent = new Label { Text = "Entradas Recientes", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(608, 24), AutoSize = true, BackColor = Color.Transparent };
            Controls.Add(lblRecent);

            var tableCard = new CardPanel { Location = new Point(608, 56), Size = new Size(680, 468) };
            dgvRecientes = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvRecientes);
            dgvRecientes.Columns.Add("Producto", "Producto");
            dgvRecientes.Columns.Add("Proveedor", "Proveedor");
            dgvRecientes.Columns.Add("Cantidad", "Cantidad");
            dgvRecientes.Columns.Add("Usuario", "Usuario");
            dgvRecientes.Columns.Add("Fecha", "Fecha");
            tableCard.Controls.Add(dgvRecientes);
            Controls.Add(tableCard);

            ResumeLayout();
        }

        private void AddComboField(Panel parent, string label, out ComboBox cbo, int x, int y, int w)
        {
            var lbl = new Label { Text = label, Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
            cbo = new ComboBox { Location = new Point(x, y + 22), Size = new Size(w, 36), Font = AppFonts.Body, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            parent.Controls.Add(lbl);
            parent.Controls.Add(cbo);
        }

        private void AddTextField(Panel parent, string label, out TextBox txt, int x, int y, int w, bool multiline = false)
        {
            var lbl = new Label { Text = label, Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
            txt = new TextBox { Location = new Point(x, y + 22), Size = new Size(w, 36), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle, Multiline = multiline };
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
        }

        private void LoadRecentes()
        {
            dgvRecientes.Rows.Clear();
            foreach (var m in _movService.ObtenerTodos())
            {
                if (m.TipoMovimiento != "Entrada") continue;
                dgvRecientes.Rows.Add(m.NombreProducto, m.Proveedor, m.Cantidad, m.NombreUsuario, m.Fecha.ToString("dd/MM/yyyy HH:mm"));
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (cboProducto.SelectedIndex < 0 || string.IsNullOrWhiteSpace(txtCantidad.Text))
            {
                MessageBox.Show("Producto y cantidad son obligatorios.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtCantidad.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número positivo.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var mov = new Movimiento
            {
                NombreProducto = cboProducto.SelectedItem?.ToString() ?? "",
                TipoMovimiento = "Entrada",
                Cantidad = qty,
                Proveedor = cboProveedor.SelectedItem?.ToString() ?? "",
                Observacion = txtObservacion.Text.Trim(),
                Fecha = dtpFecha.Value,
                NombreUsuario = Config.Session.UserName,
                UsuarioId = Config.Session.UserId
            };

            _movService.RegistrarEntrada(mov);
            MessageBox.Show("Entrada registrada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Limpiar();
            LoadRecentes();
        }

        private void Limpiar()
        {
            cboProducto.SelectedIndex = -1;
            cboProveedor.SelectedIndex = -1;
            txtCantidad.Clear();
            txtObservacion.Clear();
            dtpFecha.Value = DateTime.Now;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // FrmSalidaInventario
    // ═══════════════════════════════════════════════════════════════
    public class FrmSalidaInventario : Form
    {
        private ComboBox cboProducto = null!, cboMotivo = null!;
        private TextBox txtCantidad = null!, txtObservacion = null!;
        private DateTimePicker dtpFecha = null!;
        private readonly MovimientoService _movService = new();
        private readonly ProductoService _prodService = new();
        private DataGridView dgvRecientes = null!;

        public FrmSalidaInventario()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
            LoadRecentes();
        }

        private void BuildUI()
        {
            SuspendLayout();

            var formCard = new CardPanel { Location = new Point(24, 24), Size = new Size(560, 460) };
            Controls.Add(formCard);

            var lblTitle = new Label { Text = "📤  Registrar Salida", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(20, 16), AutoSize = true, BackColor = Color.Transparent };
            formCard.Controls.Add(lblTitle);
            var div = new Panel { Location = new Point(20, 46), Size = new Size(520, 1), BackColor = AppColors.Border };
            formCard.Controls.Add(div);

            int x = 20, y = 60, w = 510;

            // Producto
            var lblP = new Label { Text = "PRODUCTO *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
            cboProducto = new ComboBox { Location = new Point(x, y + 22), Size = new Size(w, 36), Font = AppFonts.Body, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var p in _prodService.ObtenerTodos()) cboProducto.Items.Add(p.Nombre);
            formCard.Controls.Add(lblP); formCard.Controls.Add(cboProducto);
            y += 76;

            // Motivo
            var lblM = new Label { Text = "MOTIVO *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
            cboMotivo = new ComboBox { Location = new Point(x, y + 22), Size = new Size(w, 36), Font = AppFonts.Body, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cboMotivo.Items.AddRange(new[] { "Venta", "Uso interno", "Daño/pérdida", "Devolución a proveedor", "Otro" });
            formCard.Controls.Add(lblM); formCard.Controls.Add(cboMotivo);
            y += 76;

            // Cantidad | Fecha
            var lblCant = new Label { Text = "CANTIDAD *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
            txtCantidad = new TextBox { Location = new Point(x, y + 22), Size = new Size(240, 36), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle };
            var lblF = new Label { Text = "FECHA *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x + 260, y), AutoSize = true, BackColor = Color.Transparent };
            dtpFecha = new DateTimePicker { Location = new Point(x + 260, y + 22), Size = new Size(250, 36), Font = AppFonts.Body, Format = DateTimePickerFormat.Short };
            formCard.Controls.Add(lblCant); formCard.Controls.Add(txtCantidad);
            formCard.Controls.Add(lblF); formCard.Controls.Add(dtpFecha);
            y += 76;

            // Observación
            var lblObs = new Label { Text = "OBSERVACIÓN", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
            txtObservacion = new TextBox { Location = new Point(x, y + 22), Size = new Size(w, 60), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle, Multiline = true };
            formCard.Controls.Add(lblObs); formCard.Controls.Add(txtObservacion);
            y += 96;

            var btnGuardar = UIHelper.CreateDangerButton("💾  Registrar Salida", new Size(200, 44), new Point(x, y));
            btnGuardar.Click += BtnGuardar_Click;
            formCard.Controls.Add(btnGuardar);

            var btnLimpiar = UIHelper.CreateSecondaryButton("🗑  Limpiar", new Size(120, 44), new Point(x + 210, y));
            btnLimpiar.Click += (s, e) => Limpiar();
            formCard.Controls.Add(btnLimpiar);

            // Table
            var lblRecent = new Label { Text = "Salidas Recientes", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(608, 24), AutoSize = true, BackColor = Color.Transparent };
            Controls.Add(lblRecent);

            var tableCard = new CardPanel { Location = new Point(608, 56), Size = new Size(680, 428) };
            dgvRecientes = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvRecientes);
            dgvRecientes.Columns.Add("Producto", "Producto");
            dgvRecientes.Columns.Add("Cantidad", "Cantidad");
            dgvRecientes.Columns.Add("Motivo", "Motivo");
            dgvRecientes.Columns.Add("Usuario", "Usuario");
            dgvRecientes.Columns.Add("Fecha", "Fecha");
            tableCard.Controls.Add(dgvRecientes);
            Controls.Add(tableCard);

            ResumeLayout();
        }

        private void LoadRecentes()
        {
            dgvRecientes.Rows.Clear();
            foreach (var m in _movService.ObtenerTodos())
            {
                if (m.TipoMovimiento != "Salida") continue;
                dgvRecientes.Rows.Add(m.NombreProducto, m.Cantidad, m.Observacion, m.NombreUsuario, m.Fecha.ToString("dd/MM/yyyy HH:mm"));
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (cboProducto.SelectedIndex < 0 || string.IsNullOrWhiteSpace(txtCantidad.Text))
            {
                MessageBox.Show("Producto y cantidad son obligatorios.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtCantidad.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número positivo.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var mov = new Movimiento
            {
                NombreProducto = cboProducto.SelectedItem?.ToString() ?? "",
                TipoMovimiento = "Salida",
                Cantidad = qty,
                Observacion = cboMotivo.SelectedItem?.ToString() + (string.IsNullOrEmpty(txtObservacion.Text) ? "" : $": {txtObservacion.Text}"),
                Fecha = dtpFecha.Value,
                NombreUsuario = Config.Session.UserName,
                UsuarioId = Config.Session.UserId
            };

            _movService.RegistrarSalida(mov);
            MessageBox.Show("Salida registrada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Limpiar();
            LoadRecentes();
        }

        private void Limpiar()
        {
            cboProducto.SelectedIndex = -1;
            cboMotivo.SelectedIndex = -1;
            txtCantidad.Clear();
            txtObservacion.Clear();
            dtpFecha.Value = DateTime.Now;
        }
    }
}
