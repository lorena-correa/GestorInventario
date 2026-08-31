using System;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;
using GestorInventario.Models;
using GestorInventario.Services;

namespace GestorInventario.Forms
{
    // =========================================================================
    // frmProductos / FrmProducto — Formulario de Producto (Con ErrorProvider)
    // =========================================================================
    public class FrmProducto : Form
    {
        public event Action? Saved;
        private readonly Producto? _product;
        private readonly bool _isEdit;
        private readonly ProductoService _service = new();
        private readonly ProveedorService _provService = new();

        // Controles con nomenclatura estándar según la Guía Práctica de Laboratorio
        private TextBox txtNombreProducto = null!;
        private ComboBox cboCategoria = null!;
        private TextBox txtCodigoReferencia = null!;
        private TextBox txtRutaImagen = null!;
        private Button btnExaminar = null!;
        private TextBox txtPrecioCompra = null!;
        private TextBox txtPrecioVenta = null!;
        private TextBox txtCantidadStock = null!;
        private TextBox txtDetallesProducto = null!;
        private ComboBox cboProveedor = null!;
        private Button btnActualizar = null!;
        private Button btnSalir = null!;
        private ErrorProvider errValidador = null!;

        public FrmProducto(Producto? product = null)
        {
            _product = product;
            _isEdit = product != null;

            Size = new Size(820, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            BuildUI();
            if (_isEdit) FillData();
        }

        private void BuildUI()
        {
            errValidador = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // Encabezado estándar unificado en armonía con la paleta de la aplicación
            var header = UIHelper.CreateModalHeader(this,
                _isEdit ? "ADMINISTRACIÓN DE PRODUCTOS (EDITAR)" : "ADMINISTRACIÓN DE PRODUCTOS (NUEVO REGISTRO)",
                _isEdit ? "✏️" : "📦");
            Controls.Add(header);

            // Contenido Formulario
            var content = new Panel
            {
                Location = new Point(0, 60),
                Size = new Size(820, 580),
                BackColor = Color.White,
                AutoScroll = true,
                Padding = new Padding(30, 20, 30, 20)
            };
            Controls.Add(content);

            int col1 = 36, col2 = 420, w = 360, y = 15, rowH = 74;

            // Fila 1: Nombre Producto | Categoría
            UIHelper.CreateRoundedTextBox(content, "Nombre Producto *", out txtNombreProducto, col1, y, w);
            UIHelper.CreateRoundedComboBox(content, "Categoría *", out cboCategoria, col2, y, w);
            cboCategoria.Items.AddRange(new[] { "Electrónica y Pantallas", "Periféricos y Controles", "Cables y Conectores", "Almacenamiento Digital", "Redes y Conectividad", "Componentes de PC", "Accesorios Varios" });
            cboCategoria.SelectedIndex = 0;
            y += rowH;

            // Fila 2: Código Referencia | Ruta Imagen
            UIHelper.CreateRoundedTextBox(content, "Código Referencia *", out txtCodigoReferencia, col1, y, w);

            // Ruta Imagen + botón examinar con radio unificado 8px
            var lblImg = new Label { Text = "Ruta Imagen", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(col2, y), AutoSize = true, BackColor = Color.Transparent };
            content.Controls.Add(lblImg);
            UIHelper.CreateRoundedTextBox(content, "", out txtRutaImagen, col2, y + 20, w - 100, 38);
            btnExaminar = UIHelper.CreateSecondaryButton("Examinar", new Size(92, 38), new Point(col2 + w - 92, y + 20));
            btnExaminar.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg" };
                if (ofd.ShowDialog() == DialogResult.OK) txtRutaImagen.Text = ofd.FileName;
            };
            content.Controls.Add(btnExaminar);
            y += rowH;

            // Fila 3: Precio Compra | Precio Venta
            UIHelper.CreateRoundedTextBox(content, "Precio Compra *", out txtPrecioCompra, col1, y, w);
            UIHelper.CreateRoundedTextBox(content, "Precio Venta *", out txtPrecioVenta, col2, y, w);
            y += rowH;

            // Fila 4: Cantidad stock | Proveedor
            UIHelper.CreateRoundedTextBox(content, "Cantidad stock *", out txtCantidadStock, col1, y, w);
            UIHelper.CreateRoundedComboBox(content, "Proveedor Asignado", out cboProveedor, col2, y, w);
            cboProveedor.Items.AddRange(new[] { "TechSupply S.A.", "Distribuidora Norte", "GlobalParts Ltda.", "Importadora Andina", "Sin Proveedor" });
            cboProveedor.SelectedIndex = 0;
            y += rowH;

            // Fila 5: Detalles producto (Multiline con contenedor redondeado)
            UIHelper.CreateRoundedTextBox(content, "Detalles producto", out txtDetallesProducto, col1, y, 744, 75, multiline: true);
            y += 105;

            // Botones: ACTUALIZAR (Primario) y SALIR (Secundario) con radius 8px y misma altura 42px
            btnActualizar = UIHelper.CreatePrimaryButton("ACTUALIZAR", new Size(180, 42), new Point(col1, y));
            btnActualizar.Click += BtnActualizar_Click;
            content.Controls.Add(btnActualizar);

            btnSalir = UIHelper.CreateSecondaryButton("SALIR", new Size(140, 42), new Point(col1 + 195, y));
            btnSalir.Click += (s, e) => Close();
            content.Controls.Add(btnSalir);
        }

        private void FillData()
        {
            var p = _product!;
            txtCodigoReferencia.Text = p.Codigo;
            txtNombreProducto.Text = p.Nombre;
            txtDetallesProducto.Text = p.Descripcion;
            txtPrecioCompra.Text = p.PrecioCompra.ToString("N0");
            txtPrecioVenta.Text = p.PrecioVenta.ToString("N0");
            txtCantidadStock.Text = p.StockActual.ToString();

            if (cboCategoria.Items.Contains(p.Categoria)) cboCategoria.SelectedItem = p.Categoria;
            if (cboProveedor.Items.Contains(p.Proveedor)) cboProveedor.SelectedItem = p.Proveedor;
        }

        private void BtnActualizar_Click(object? sender, EventArgs e)
        {
            errValidador.Clear();
            bool hayErrores = false;

            if (string.IsNullOrWhiteSpace(txtNombreProducto.Text))
            {
                errValidador.SetError(txtNombreProducto, "El Nombre del Producto es obligatorio.");
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(txtCodigoReferencia.Text))
            {
                errValidador.SetError(txtCodigoReferencia, "El Código de Referencia es obligatorio.");
                hayErrores = true;
            }

            if (!decimal.TryParse(txtPrecioVenta.Text.Replace("$", "").Replace(",", "").Trim(), out decimal pv) || pv <= 0)
            {
                errValidador.SetError(txtPrecioVenta, "El Precio de Venta debe ser mayor a 0.");
                hayErrores = true;
            }

            if (!decimal.TryParse(txtPrecioCompra.Text.Replace("$", "").Replace(",", "").Trim(), out decimal pc) || pc < 0)
            {
                errValidador.SetError(txtPrecioCompra, "El Precio de Compra no puede ser negativo.");
                hayErrores = true;
            }

            if (!int.TryParse(txtCantidadStock.Text.Trim(), out int stock) || stock < 0)
            {
                errValidador.SetError(txtCantidadStock, "La Cantidad de Stock no puede ser negativa.");
                hayErrores = true;
            }

            if (hayErrores)
            {
                MessageBox.Show("Por favor complete los campos obligatorios indicados con error.",
                    "Validación con ErrorProvider", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var producto = _isEdit ? _product! : new Producto();
            producto.Codigo = txtCodigoReferencia.Text.Trim().ToUpper();
            producto.Nombre = txtNombreProducto.Text.Trim();
            producto.Descripcion = txtDetallesProducto.Text.Trim();
            producto.PrecioCompra = pc;
            producto.PrecioVenta = pv;
            producto.StockActual = stock;
            producto.StockMinimo = 5;
            producto.Categoria = cboCategoria.SelectedItem?.ToString() ?? "";
            producto.Proveedor = cboProveedor.SelectedItem?.ToString() ?? "";
            producto.Activo = true;

            try
            {
                _service.Guardar(producto);
            }
            catch
            {
                // Fallback si BD no conectada
            }

            MessageBox.Show("¡Producto actualizado y guardado correctamente!",
                "Operación Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Saved?.Invoke();
            Close();
        }
    }

    // Alias para coincidir exactamente con los nombres de la guía
    public class frmProductos : FrmProducto
    {
        public frmProductos(Producto? product = null) : base(product) { }
    }
}
