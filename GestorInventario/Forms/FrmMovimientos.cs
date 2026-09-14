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

            var formCard = new CardPanel { Location = new Point(24, 24), Size = new Size(460, 540) };
            Controls.Add(formCard);

            var lblFormTitle = new Label { Text = "📥  Registrar Entrada", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(20, 16), AutoSize = true, BackColor = Color.Transparent };
            formCard.Controls.Add(lblFormTitle);
            var divider = new Panel { Location = new Point(20, 46), Size = new Size(420, 1), BackColor = AppColors.Border };
            formCard.Controls.Add(divider);

            int x = 20, y = 60, w = 420;

            // Producto
            UIHelper.CreateRoundedComboBox(formCard, "PRODUCTO *", out cboProducto, x, y, w);
            try
            {
                _productos = _prodService.ObtenerTodos();
                foreach (var p in _productos) cboProducto.Items.Add(p.Nombre);
            }
            catch { }
            y += 76;

            // Proveedor
            UIHelper.CreateRoundedComboBox(formCard, "PROVEEDOR", out cboProveedor, x, y, w);
            try
            {
                _proveedores = _provService.ObtenerTodos();
                foreach (var p in _proveedores) cboProveedor.Items.Add(p.Nombre);
            }
            catch { }
            y += 76;

            // Cantidad | Fecha
            UIHelper.CreateRoundedTextBox(formCard, "CANTIDAD *", out txtCantidad, x, y, 190);
            UIHelper.CreateRoundedDateTimePicker(formCard, "FECHA *", out dtpFecha, x + 210, y, 210);
            y += 76;

            // Observación
            UIHelper.CreateRoundedTextBox(formCard, "OBSERVACIÓN", out txtObservacion, x, y, w, 90, multiline: true);
            y += 116;

            var btnGuardar = UIHelper.CreatePrimaryButton("💾 Registrar Entrada", new Size(180, 42), new Point(x, y));
            btnGuardar.Click += BtnGuardar_Click;
            formCard.Controls.Add(btnGuardar);

            var btnLimpiar = UIHelper.CreateSecondaryButton("🗑 Limpiar", new Size(110, 42), new Point(x + 195, y));
            btnLimpiar.Click += (s, e) => Limpiar();
            formCard.Controls.Add(btnLimpiar);

            // Tabla recientes (Empieza en Y=24 con margen derecho y fondo)
            var tableCard = new CardPanel
            {
                Location = new Point(508, 24),
                Size = new Size(640, 540),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            var lblRecent = new Label { Text = "📋  Entradas Recientes", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(20, 16), AutoSize = true, BackColor = Color.Transparent };
            tableCard.Controls.Add(lblRecent);
            var divTable = new Panel { Location = new Point(20, 46), Size = new Size(tableCard.Width - 40, 1), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right, BackColor = AppColors.Border };
            tableCard.Controls.Add(divTable);

            dgvRecientes = new DataGridView
            {
                Location = new Point(20, 58),
                Size = new Size(tableCard.Width - 40, tableCard.Height - 78),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            UIHelper.StyleDataGridView(dgvRecientes);
            dgvRecientes.Columns.Add("Producto", "Producto");
            dgvRecientes.Columns.Add("Proveedor", "Proveedor");
            dgvRecientes.Columns.Add("Cantidad", "Cantidad");
            dgvRecientes.Columns.Add("Usuario", "Usuario");
            dgvRecientes.Columns.Add("Fecha", "Fecha");
            tableCard.Controls.Add(dgvRecientes);
            UIHelper.BindEmptyState(dgvRecientes, "No hay entradas registradas todavía.", "📥");

            Controls.Add(tableCard);

            UIHelper.BindFillWidth(this, tableCard, 24);
            UIHelper.BindFillHeight(this, tableCard, 24);

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
                ModernMessageBox.ShowError($"Error al cargar entradas: {ex.Message}");
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            // Validaciones
            if (cboProducto.SelectedIndex < 0)
            {
                ModernMessageBox.ShowWarning("Debes seleccionar un producto.", "Validación");
                return;
            }
            if (!int.TryParse(txtCantidad.Text, out int qty) || qty <= 0)
            {
                ModernMessageBox.ShowWarning("La cantidad debe ser un número mayor a cero.", "Validación");
                txtCantidad.Focus();
                return;
            }
            if (dtpFecha.Value.Date > DateTime.Today)
            {
                ModernMessageBox.ShowWarning("La fecha no puede ser futura.", "Validación");
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
                ModernMessageBox.ShowSuccess("Entrada registrada correctamente.");
                Limpiar();
                LoadRecentes();
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError(ex.Message, "Error al registrar");
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

            var formCard = new CardPanel { Location = new Point(24, 24), Size = new Size(460, 540) };
            Controls.Add(formCard);

            var lblTitle = new Label { Text = "📤  Registrar Salida", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(20, 16), AutoSize = true, BackColor = Color.Transparent };
            formCard.Controls.Add(lblTitle);
            var div = new Panel { Location = new Point(20, 46), Size = new Size(420, 1), BackColor = AppColors.Border };
            formCard.Controls.Add(div);

            int x = 20, y = 60, w = 420;

            // Producto + stock disponible
            UIHelper.CreateRoundedComboBox(formCard, "PRODUCTO *", out cboProducto, x, y, w);
            try
            {
                _productos = _prodService.ObtenerTodos();
                foreach (var p in _productos) cboProducto.Items.Add(p.Nombre);
            }
            catch { }
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
            UIHelper.CreateRoundedComboBox(formCard, "MOTIVO *", out cboMotivo, x, y, w);
            foreach (var motivo in _motivos.Keys) cboMotivo.Items.Add(motivo);
            y += 76;

            // Cantidad | Fecha
            UIHelper.CreateRoundedTextBox(formCard, "CANTIDAD *", out txtCantidad, x, y, 190);
            UIHelper.CreateRoundedDateTimePicker(formCard, "FECHA *", out dtpFecha, x + 210, y, 210);
            y += 76;

            // Observación
            UIHelper.CreateRoundedTextBox(formCard, "OBSERVACIÓN", out txtObservacion, x, y, w, 90, multiline: true);
            y += 116;

            var btnGuardar = UIHelper.CreateDangerButton("💾 Registrar Salida", new Size(180, 42), new Point(x, y));
            btnGuardar.Click += BtnGuardar_Click;
            formCard.Controls.Add(btnGuardar);

            var btnLimpiar = UIHelper.CreateSecondaryButton("🗑 Limpiar", new Size(110, 42), new Point(x + 195, y));
            btnLimpiar.Click += (s, e) => Limpiar();
            formCard.Controls.Add(btnLimpiar);

            // Tabla recientes (Empieza en Y=24 con margen derecho y fondo)
            var tableCard = new CardPanel
            {
                Location = new Point(508, 24),
                Size = new Size(640, 540),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            var lblRecent = new Label { Text = "📋  Salidas Recientes", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(20, 16), AutoSize = true, BackColor = Color.Transparent };
            tableCard.Controls.Add(lblRecent);
            var divTable = new Panel { Location = new Point(20, 46), Size = new Size(tableCard.Width - 40, 1), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right, BackColor = AppColors.Border };
            tableCard.Controls.Add(divTable);

            dgvRecientes = new DataGridView
            {
                Location = new Point(20, 58),
                Size = new Size(tableCard.Width - 40, tableCard.Height - 78),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            UIHelper.StyleDataGridView(dgvRecientes);
            dgvRecientes.Columns.Add("Producto", "Producto");
            dgvRecientes.Columns.Add("Cantidad", "Cantidad");
            dgvRecientes.Columns.Add("Motivo", "Motivo");
            dgvRecientes.Columns.Add("Usuario", "Usuario");
            dgvRecientes.Columns.Add("Fecha", "Fecha");
            tableCard.Controls.Add(dgvRecientes);
            UIHelper.BindEmptyState(dgvRecientes, "No hay salidas registradas todavía.", "📤");

            Controls.Add(tableCard);

            UIHelper.BindFillWidth(this, tableCard, 24);
            UIHelper.BindFillHeight(this, tableCard, 24);

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
                ModernMessageBox.ShowError($"Error al cargar salidas: {ex.Message}");
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            // Validaciones
            if (cboProducto.SelectedIndex < 0)
            {
                ModernMessageBox.ShowWarning("Debes seleccionar un producto.", "Validación");
                return;
            }
            if (cboMotivo.SelectedIndex < 0)
            {
                ModernMessageBox.ShowWarning("Debes seleccionar un motivo.", "Validación");
                return;
            }
            if (!int.TryParse(txtCantidad.Text, out int qty) || qty <= 0)
            {
                ModernMessageBox.ShowWarning("La cantidad debe ser un número mayor a cero.", "Validación");
                txtCantidad.Focus();
                return;
            }
            if (dtpFecha.Value.Date > DateTime.Today)
            {
                ModernMessageBox.ShowWarning("La fecha no puede ser futura.", "Validación");
                return;
            }

            // Verificar stock antes de enviar
            var producto = _productos[cboProducto.SelectedIndex];
            if (qty > producto.StockActual)
            {
                ModernMessageBox.ShowWarning($"Stock insuficiente. Disponible: {producto.StockActual}, solicitado: {qty}.", "Stock insuficiente");
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
                ModernMessageBox.ShowSuccess("Salida registrada correctamente.");
                Limpiar();
                LoadRecentes();
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError(ex.Message, "Error al registrar");
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