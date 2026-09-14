using System;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;
using GestorInventario.Models;
using GestorInventario.Services;

namespace GestorInventario.Forms
{
    public class FrmProductos : Form
    {
        private DataGridView dgvProductos = null!;
        private TextBox txtBuscar = null!;
        private readonly ProductoService _service = new();
        private Producto? _selectedProduct;

        public FrmProductos()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // ── Toolbar ─────────────────────────────────────────────────
            // El título del módulo ya se muestra en la barra superior, así que
            // aquí solo dejamos un ícono de referencia y la fila de acciones.
            var toolbarCard = new CardPanel
            {
                Location = new Point(24, 16),
                Size = new Size(1140, 64),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Padding = new Padding(0, 13, 20, 13)
            };

            var lblIcono = new Label
            {
                Text = "📦",
                Font = new Font("Segoe UI Emoji", 16f),
                Location = new Point(20, 14),
                Size = new Size(36, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblIcono);

            var lblSubtitulo = new Label
            {
                Text = "Catálogo general, control de precios de compra/venta y niveles de stock",
                Font = AppFonts.Small,
                ForeColor = AppColors.TextSecondary,
                Location = new Point(64, 24),
                Size = new Size(400, 18),
                AutoEllipsis = true,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblSubtitulo);

            // Contenedor de acciones alineado a la derecha (Dock, no Anchor con
            // coordenada fija: así nunca se descuadra al redimensionar la tarjeta)
            var actionsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Right,
                BackColor = Color.Transparent
            };
            UIHelper.BindFillWidthUntil(this, lblSubtitulo, actionsPanel, 16);

            UIHelper.CreateSearchInput(actionsPanel, out txtBuscar, 0, 0, 220, 38, "Buscar producto...");
            txtBuscar.TextChanged += (s, e) => LoadData(txtBuscar.Text);

            var btnAgregar = UIHelper.CreatePrimaryButton("＋ NUEVO", new Size(105, 38), new Point(0, 0));
            btnAgregar.Margin = new Padding(6, 0, 0, 0);
            btnAgregar.Click += (s, e) => OpenProductForm(null);
            actionsPanel.Controls.Add(btnAgregar);

            var btnEditar = UIHelper.CreateEditButton("✏️ EDITAR", new Size(95, 38), new Point(0, 0));
            btnEditar.Margin = new Padding(6, 0, 0, 0);
            btnEditar.Click += (s, e) =>
            {
                if (_selectedProduct == null) { ShowSelectWarning(); return; }
                OpenProductForm(_selectedProduct);
            };
            actionsPanel.Controls.Add(btnEditar);

            var btnEliminar = UIHelper.CreateDangerButton("🗑 BORRAR", new Size(95, 38), new Point(0, 0));
            btnEliminar.Margin = new Padding(6, 0, 0, 0);
            btnEliminar.Click += (s, e) =>
            {
                if (_selectedProduct == null) { ShowSelectWarning(); return; }
                ConfirmDelete();
            };
            actionsPanel.Controls.Add(btnEliminar);

            toolbarCard.Controls.Add(actionsPanel);
            Controls.Add(toolbarCard);

            // ── DataGridView card ────────────────────────────────────────
            var tableCard = new CardPanel
            {
                Location = new Point(24, 92),
                Size = new Size(1140, 520),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            dgvProductos = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvProductos);
            SetupColumns();
            dgvProductos.SelectionChanged += (s, e) =>
            {
                if (dgvProductos.SelectedRows.Count > 0)
                    _selectedProduct = dgvProductos.SelectedRows[0].Tag as Producto;
            };
            dgvProductos.CellDoubleClick += (s, e) =>
            {
                if (_selectedProduct != null) OpenProductForm(_selectedProduct);
            };

            tableCard.Controls.Add(dgvProductos);
            UIHelper.BindEmptyState(dgvProductos, "No hay productos registrados todavía.");
            Controls.Add(tableCard);

            UIHelper.BindFillWidth(this, toolbarCard, 24);
            UIHelper.BindFillWidth(this, tableCard, 24);
            UIHelper.BindFillHeight(this, tableCard, 44);

            // Row count label
            var lblCount = new Label
            {
                Name = "lblCount",
                Text = "",
                Font = AppFonts.Small,
                ForeColor = AppColors.TextSecondary,
                Location = new Point(24, 600),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            Controls.Add(lblCount);

            ResumeLayout();
        }

        private void SetupColumns()
        {
            dgvProductos.Columns.Clear();
            dgvProductos.Columns.Add("Codigo", "Código");
            dgvProductos.Columns.Add("Nombre", "Nombre");
            dgvProductos.Columns.Add("Categoria", "Categoría");
            dgvProductos.Columns.Add("Proveedor", "Proveedor");
            dgvProductos.Columns.Add("PrecioVenta", "Precio Venta");
            dgvProductos.Columns.Add("Stock", "Stock Actual");
            dgvProductos.Columns.Add("StockMin", "Stock Mín.");
            dgvProductos.Columns.Add("Estado", "Estado");
            dgvProductos.Columns["Codigo"].Width = 100;
            dgvProductos.Columns["Stock"].Width = 100;
            dgvProductos.Columns["StockMin"].Width = 100;
            dgvProductos.Columns["Estado"].Width = 90;
        }

        private void LoadData(string search = "")
        {
            try
            {
                var productos = string.IsNullOrEmpty(search)
                    ? _service.ObtenerTodos()
                    : _service.Buscar(search);

                dgvProductos.Rows.Clear();
                foreach (var p in productos)
                {
                    int r = dgvProductos.Rows.Add(
                        p.Codigo, p.Nombre, p.Categoria, p.Proveedor,
                        $"${p.PrecioVenta:N0}",
                        p.StockActual,
                        p.StockMinimo,
                        p.Activo ? "Activo" : "Inactivo");
                    dgvProductos.Rows[r].Tag = p;

                    if (p.StockActual <= p.StockMinimo)
                    {
                        dgvProductos.Rows[r].Cells["Stock"].Style.ForeColor = AppColors.Danger;
                        dgvProductos.Rows[r].Cells["Stock"].Style.Font = AppFonts.BodyBold;
                    }
                }

                if (Controls["lblCount"] is Label lbl)
                    lbl.Text = $"{productos.Count} producto(s) encontrado(s)";
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError($"Error al cargar productos: {ex.Message}");
            }
        }

        private void OpenProductForm(Producto? product)
        {
            var frm = new FrmProducto(product);
            frm.Saved += () => LoadData();
            frm.ShowDialog(this);
        }

        private void ConfirmDelete()
        {
            if (ModernMessageBox.ShowConfirm($"¿Eliminar el producto \"{_selectedProduct!.Nombre}\"?", "Confirmar eliminación", "Eliminar") == DialogResult.Yes)
            {
                try
                {
                    _service.Eliminar(_selectedProduct.Id);
                    _selectedProduct = null;
                    LoadData();
                    ModernMessageBox.ShowSuccess("Producto eliminado correctamente.");
                }
                catch (Exception ex)
                {
                    ModernMessageBox.ShowError($"Error al eliminar: {ex.Message}");
                }
            }
        }

        private void ShowSelectWarning() =>
            ModernMessageBox.ShowInfo("Selecciona un producto de la lista.", "Selección requerida");
    }

    // Alias para la guía
    public class frmlista_productos : FrmProductos
    {
    }
}
