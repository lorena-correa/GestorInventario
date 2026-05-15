using System;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;
using GestorInventario.Models;
using GestorInventario.Services;

namespace GestorInventario.Forms
{
    public class FrmProducto : Form
    {
        public event Action? Saved;
        private readonly Producto? _product;
        private readonly bool _isEdit;
        private readonly ProductoService _service = new();
        private readonly ProveedorService _provService = new();

        // Controls
        private TextBox txtCodigo = null!, txtNombre = null!, txtDescripcion = null!;
        private TextBox txtPrecioCompra = null!, txtPrecioVenta = null!;
        private TextBox txtStockInicial = null!, txtStockMinimo = null!;
        private ComboBox cboCategoria = null!, cboProveedor = null!, cboEstado = null!;

        public FrmProducto(Producto? product = null)
        {
            _product = product;
            _isEdit = product != null;
            Size = new Size(680, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BuildUI();
            if (_isEdit) FillData();
        }

        private void BuildUI()
        {
            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = _isEdit ? AppColors.Primary : AppColors.Success };
            var lblTitle = new Label { Text = _isEdit ? "✏️  Editar Producto" : "➕  Nuevo Producto", Font = AppFonts.SubHeading, ForeColor = Color.White, Location = new Point(20, 18), AutoSize = true };
            var btnX = new Label { Text = "✕", Font = new Font("Segoe UI", 16f), ForeColor = Color.White, Cursor = Cursors.Hand, Location = new Point(640, 16), AutoSize = true };
            btnX.Click += (s, e) => Close();
            header.Controls.AddRange(new Control[] { lblTitle, btnX });
            Controls.Add(header);

            // Form content
            var content = new Panel { Location = new Point(0, 60), Size = new Size(680, 520), BackColor = Color.White, AutoScroll = true };
            Controls.Add(content);

            int col1 = 28, col2 = 360, rowH = 80, y = 20;

            // Row 1: Código | Nombre
            AddField(content, "CÓDIGO *", out txtCodigo, col1, y, 280);
            AddField(content, "NOMBRE *", out txtNombre, col2, y, 290);
            y += rowH;

            // Row 2: Descripción (full width)
            AddField(content, "DESCRIPCIÓN", out txtDescripcion, col1, y, 622, multiline: true);
            txtDescripcion.Height = 60;
            y += 100;

            // Row 3: Precio compra | Precio venta
            AddField(content, "PRECIO COMPRA *", out txtPrecioCompra, col1, y, 280);
            AddField(content, "PRECIO VENTA *", out txtPrecioVenta, col2, y, 290);
            y += rowH;

            // Row 4: Stock inicial | Stock mínimo
            AddField(content, "STOCK INICIAL", out txtStockInicial, col1, y, 280);
            AddField(content, "STOCK MÍNIMO *", out txtStockMinimo, col2, y, 290);
            y += rowH;

            // Row 5: Categoría | Proveedor
            AddCombo(content, "CATEGORÍA", out cboCategoria, col1, y, 280);
            cboCategoria.Items.AddRange(new[] { "Electrónica", "Periféricos", "Cables", "Accesorios", "Almacenamiento", "Redes", "Otros" });

            AddCombo(content, "PROVEEDOR", out cboProveedor, col2, y, 290);
            foreach (var p in _provService.ObtenerTodos())
                cboProveedor.Items.Add(p.Nombre);
            y += rowH;

            // Row 6: Estado
            AddCombo(content, "ESTADO", out cboEstado, col1, y, 280);
            cboEstado.Items.AddRange(new[] { "Activo", "Inactivo" });
            cboEstado.SelectedIndex = 0;
            y += rowH;

            // Buttons
            int btnY = content.Height - 60;
            var btnGuardar = UIHelper.CreatePrimaryButton("💾  Guardar", new Size(160, 44), new Point(col1, btnY));
            btnGuardar.Click += BtnGuardar_Click;
            content.Controls.Add(btnGuardar);

            var btnCancelar = UIHelper.CreateSecondaryButton("✕  Cancelar", new Size(120, 44), new Point(200, btnY));
            btnCancelar.Click += (s, e) => Close();
            content.Controls.Add(btnCancelar);

            var btnLimpiar = UIHelper.CreateSecondaryButton("🗑  Limpiar", new Size(120, 44), new Point(330, btnY));
            btnLimpiar.Click += (s, e) => LimpiarFormulario();
            content.Controls.Add(btnLimpiar);
        }

        private void AddField(Panel parent, string label, out TextBox txt, int x, int y, int width, bool multiline = false)
        {
            var lbl = new Label { Text = label, Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true };
            txt = new TextBox
            {
                Location = new Point(x, y + 22),
                Size = new Size(width, 36),
                Font = AppFonts.Body,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = multiline
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
        }

        private void AddCombo(Panel parent, string label, out ComboBox cbo, int x, int y, int width)
        {
            var lbl = new Label { Text = label, Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true };
            cbo = new ComboBox
            {
                Location = new Point(x, y + 22),
                Size = new Size(width, 36),
                Font = AppFonts.Body,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(cbo);
        }

        private void FillData()
        {
            var p = _product!;
            txtCodigo.Text = p.Codigo;
            txtNombre.Text = p.Nombre;
            txtDescripcion.Text = p.Descripcion;
            txtPrecioCompra.Text = p.PrecioCompra.ToString();
            txtPrecioVenta.Text = p.PrecioVenta.ToString();
            txtStockInicial.Text = p.StockActual.ToString();
            txtStockMinimo.Text = p.StockMinimo.ToString();
            if (cboCategoria.Items.Contains(p.Categoria)) cboCategoria.SelectedItem = p.Categoria;
            if (cboProveedor.Items.Contains(p.Proveedor)) cboProveedor.SelectedItem = p.Proveedor;
            cboEstado.SelectedItem = p.Activo ? "Activo" : "Inactivo";
        }

        private void LimpiarFormulario()
        {
            txtCodigo.Clear(); txtNombre.Clear(); txtDescripcion.Clear();
            txtPrecioCompra.Clear(); txtPrecioVenta.Clear();
            txtStockInicial.Clear(); txtStockMinimo.Clear();
            cboCategoria.SelectedIndex = -1;
            cboProveedor.SelectedIndex = -1;
            cboEstado.SelectedIndex = 0;
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Código y Nombre son obligatorios.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var producto = _isEdit ? _product! : new Producto();
            producto.Codigo = txtCodigo.Text.Trim();
            producto.Nombre = txtNombre.Text.Trim();
            producto.Descripcion = txtDescripcion.Text.Trim();
            decimal.TryParse(txtPrecioCompra.Text, out decimal pc); producto.PrecioCompra = pc;
            decimal.TryParse(txtPrecioVenta.Text, out decimal pv); producto.PrecioVenta = pv;
            int.TryParse(txtStockInicial.Text, out int si); producto.StockActual = si;
            int.TryParse(txtStockMinimo.Text, out int sm); producto.StockMinimo = sm;
            producto.Categoria = cboCategoria.SelectedItem?.ToString() ?? "";
            producto.Proveedor = cboProveedor.SelectedItem?.ToString() ?? "";
            producto.Activo = cboEstado.SelectedItem?.ToString() == "Activo";

            _service.Guardar(producto);
            MessageBox.Show("Producto guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Saved?.Invoke();
            Close();
        }
    }
}
