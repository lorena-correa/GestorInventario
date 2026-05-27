using System;
using System.Collections.Generic;
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

        // Listas completas para resolver IDs
        private List<Producto> _productos = new();
        private List<Proveedor> _proveedores = new();

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

            var formCard = new CardPanel { Location = new Point(24, 24), Size = new Size(560, 500) };
            Controls.Add(formCard);

            var lblFormTitle = new Label { Text = "📥  Registrar Entrada", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(20, 16), AutoSize = true, BackColor = Color.Transparent };
            formCard.Controls.Add(lblFormTitle);
            var divider = new Panel { Location = new Point(20, 46), Size = new Size(520, 1), BackColor = AppColors.Border };
            formCard.Controls.Add(divider);

            int x = 20, y = 60, w = 510;

            // Producto
            AddComboField(formCard, "PRODUCTO *", out cboProducto, x, y, w);
            try
            {
                _productos = _prodService.ObtenerTodos();
                foreach (var p in _productos) cboProducto.Items.Add(p.Nombre);
            }
            catch { }
            y += 76;

            // Proveedor
            AddComboField(formCard, "PROVEEDOR", out cboProveedor, x, y, w);
            try
            {
                _proveedores = _provService.ObtenerTodos();
                foreach (var p in _proveedores) cboProveedor.Items.Add(p.Nombre);
            }
            catch { }
            y += 76;

            // Cantidad | Fecha
            AddTextField(formCard, "CANTIDAD *", out txtCantidad, x, y, 240);
            var lblF = new Label { Text = "FECHA *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x + 260, y), AutoSize = true, BackColor = Color.Transparent };
            dtpFecha = new DateTimePicker { Location = new Point(x + 260, y + 22), Size = new Size(250, 36), Font = AppFonts.Body, Format = DateTimePickerFormat.Short };
            formCard.Controls.Add(lblF);
            formCard.Controls.Add(dtpFecha);
            y += 76;

            // Observación
            AddTextField(formCard, "OBSERVACIÓN", out txtObservacion, x, y, w, multiline: true);
            txtObservacion.Height = 72;
            y += 108;

            var btnGuardar = UIHelper.CreatePrimaryButton("💾  Registrar Entrada", new Size(200, 44), new Point(x, y));
            btnGuardar.Click += BtnGuardar_Click;
            formCard.Controls.Add(btnGuardar);

            var btnLimpiar = UIHelper.CreateSecondaryButton("🗑  Limpiar", new Size(120, 44), new Point(x + 210, y));
            btnLimpiar.Click += (s, e) => Limpiar();
            formCard.Controls.Add(btnLimpiar);

            // Tabla recientes
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

        private void LoadRecentes()
        {
            try
            {
                dgvRecientes.Rows.Clear();
                var movimientos = _movService.Filtrar(tipoId: 1); // Compra a proveedor
                // Si no hay con tipoId=1 traer todos los de entrada
                if (movimientos.Count == 0)
                    movimientos = _movService.ObtenerTodos()
                        .FindAll(m => m.TipoEntradaSalida == "entrada");

                foreach (var m in movimientos)
                    dgvRecientes.Rows.Add(
                        m.NombreProducto, m.Proveedor,
                        m.Cantidad, m.NombreUsuario,
                        m.Fecha.ToString("dd/MM/yyyy HH:mm"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar entradas: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            // Validaciones
            if (cboProducto.SelectedIndex < 0)
            {
                MessageBox.Show("Debes seleccionar un producto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtCantidad.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número mayor a cero.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCantidad.Focus();
                return;
            }
            if (dtpFecha.Value.Date > DateTime.Today)
            {
                MessageBox.Show("La fecha no puede ser futura.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Resolver IDs
            var producto = _productos[cboProducto.SelectedIndex];
            var proveedorId = cboProveedor.SelectedIndex >= 0
                ? _proveedores[cboProveedor.SelectedIndex].Id
                : 0;

            var mov = new Movimiento
            {
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                TipoMovimientoId = 1, // Compra a proveedor
                TipoMovimiento = "Compra a proveedor",
                Cantidad = qty,
                ProveedorId = proveedorId,
                Observacion = txtObservacion.Text.Trim(),
                Fecha = dtpFecha.Value,
                NombreUsuario = Config.Session.UserName,
                UsuarioId = Config.Session.UserId
            };

            try
            {
                _movService.RegistrarEntrada(mov);
                MessageBox.Show("Entrada registrada correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Limpiar();
                LoadRecentes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al registrar",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Limpiar()
        {
            cboProducto.SelectedIndex = -1;
            cboProveedor.SelectedIndex = -1;
            txtCantidad.Clear();
            txtObservacion.Clear();
            dtpFecha.Value = DateTime.Now;
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
    }

    // ═══════════════════════════════════════════════════════════════
    // FrmSalidaInventario
    // ═══════════════════════════════════════════════════════════════
    public class FrmSalidaInventario : Form
    {
        private ComboBox cboProducto = null!, cboMotivo = null!;
        private TextBox txtCantidad = null!, txtObservacion = null!;
        private DateTimePicker dtpFecha = null!;
        private Label lblStockDisponible = null!;
        private readonly MovimientoService _movService = new();
        private readonly ProductoService _prodService = new();
        private DataGridView dgvRecientes = null!;

        private List<Producto> _productos = new();

        // Mapa motivo → TipoMovimientoId
        private readonly Dictionary<string, int> _motivos = new()
        {
            { "Venta",                   2 },
            { "Uso interno",             3 },
            { "Devolución a proveedor",  4 },
            { "Ajuste de inventario",    5 },
            { "Daño/pérdida",            6 }
        };

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

            var formCard = new CardPanel { Location = new Point(24, 24), Size = new Size(560, 480) };
            Controls.Add(formCard);

            var lblTitle = new Label { Text = "📤  Registrar Salida", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(20, 16), AutoSize = true, BackColor = Color.Transparent };
            formCard.Controls.Add(lblTitle);
            var div = new Panel { Location = new Point(20, 46), Size = new Size(520, 1), BackColor = AppColors.Border };
            formCard.Controls.Add(div);

            int x = 20, y = 60, w = 510;

            // Producto + stock disponible
            var lblP = new Label { Text = "PRODUCTO *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
            cboProducto = new ComboBox { Location = new Point(x, y + 22), Size = new Size(w, 36), Font = AppFonts.Body, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            try
            {
                _productos = _prodService.ObtenerTodos();
                foreach (var p in _productos) cboProducto.Items.Add(p.Nombre);
            }
            catch { }
            formCard.Controls.Add(lblP);
            formCard.Controls.Add(cboProducto);
            y += 76;

            // Label stock disponible
            lblStockDisponible = new Label { Text = "", Font = AppFonts.Small, ForeColor = AppColors.Success, Location = new Point(x, y - 14), AutoSize = true, BackColor = Color.Transparent };
            formCard.Controls.Add(lblStockDisponible);

            // Mostrar stock al seleccionar producto
            cboProducto.SelectedIndexChanged += (s, e) =>
            {
                if (cboProducto.SelectedIndex >= 0)
                {
                    var prod = _productos[cboProducto.SelectedIndex];
                    lblStockDisponible.Text = $"Stock disponible: {prod.StockActual} unidades";
                    lblStockDisponible.ForeColor = prod.EsCritico ? AppColors.Danger : AppColors.Success;
                }
            };

            // Motivo
            var lblM = new Label { Text = "MOTIVO *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true, BackColor = Color.Transparent };
            cboMotivo = new ComboBox { Location = new Point(x, y + 22), Size = new Size(w, 36), Font = AppFonts.Body, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var motivo in _motivos.Keys) cboMotivo.Items.Add(motivo);
            formCard.Controls.Add(lblM);
            formCard.Controls.Add(cboMotivo);
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
            formCard.Controls.Add(lblObs);
            formCard.Controls.Add(txtObservacion);
            y += 96;

            var btnGuardar = UIHelper.CreateDangerButton("💾  Registrar Salida", new Size(200, 44), new Point(x, y));
            btnGuardar.Click += BtnGuardar_Click;
            formCard.Controls.Add(btnGuardar);

            var btnLimpiar = UIHelper.CreateSecondaryButton("🗑  Limpiar", new Size(120, 44), new Point(x + 210, y));
            btnLimpiar.Click += (s, e) => Limpiar();
            formCard.Controls.Add(btnLimpiar);

            // Tabla recientes
            var lblRecent = new Label { Text = "Salidas Recientes", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(608, 24), AutoSize = true, BackColor = Color.Transparent };
            Controls.Add(lblRecent);

            var tableCard = new CardPanel { Location = new Point(608, 56), Size = new Size(680, 448) };
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
            try
            {
                dgvRecientes.Rows.Clear();
                var movimientos = _movService.ObtenerTodos()
                    .FindAll(m => m.TipoEntradaSalida == "salida");

                foreach (var m in movimientos)
                    dgvRecientes.Rows.Add(
                        m.NombreProducto, m.Cantidad,
                        m.TipoMovimiento, m.NombreUsuario,
                        m.Fecha.ToString("dd/MM/yyyy HH:mm"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar salidas: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            // Validaciones
            if (cboProducto.SelectedIndex < 0)
            {
                MessageBox.Show("Debes seleccionar un producto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cboMotivo.SelectedIndex < 0)
            {
                MessageBox.Show("Debes seleccionar un motivo.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtCantidad.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número mayor a cero.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCantidad.Focus();
                return;
            }
            if (dtpFecha.Value.Date > DateTime.Today)
            {
                MessageBox.Show("La fecha no puede ser futura.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verificar stock antes de enviar
            var producto = _productos[cboProducto.SelectedIndex];
            if (qty > producto.StockActual)
            {
                MessageBox.Show($"Stock insuficiente. Disponible: {producto.StockActual}, solicitado: {qty}.",
                    "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string motivoSeleccionado = cboMotivo.SelectedItem!.ToString()!;
            int tipoId = _motivos[motivoSeleccionado];

            var mov = new Movimiento
            {
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                TipoMovimientoId = tipoId,
                TipoMovimiento = motivoSeleccionado,
                Cantidad = qty,
                Observacion = string.IsNullOrEmpty(txtObservacion.Text)
                                    ? motivoSeleccionado
                                    : $"{motivoSeleccionado}: {txtObservacion.Text.Trim()}",
                Fecha = dtpFecha.Value,
                NombreUsuario = Config.Session.UserName,
                UsuarioId = Config.Session.UserId
            };

            try
            {
                _movService.RegistrarSalida(mov);
                MessageBox.Show("Salida registrada correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Limpiar();
                LoadRecentes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al registrar",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Limpiar()
        {
            cboProducto.SelectedIndex = -1;
            cboMotivo.SelectedIndex = -1;
            txtCantidad.Clear();
            txtObservacion.Clear();
            dtpFecha.Value = DateTime.Now;
            lblStockDisponible.Text = "";
        }
    }
}