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
            var toolbar = new Panel
            {
                Location = new Point(24, 24),
                Size = new Size(1200, 52),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            var lblSearch = new Label { Text = "🔍", Font = new Font("Segoe UI Emoji", 12f), Location = new Point(0, 12), AutoSize = true, BackColor = Color.Transparent };
            toolbar.Controls.Add(lblSearch);

            txtBuscar = new TextBox
            {
                Location = new Point(28, 8),
                Size = new Size(300, 36),
                Font = AppFonts.Body,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Buscar producto..."
            };
            txtBuscar.TextChanged += (s, e) => LoadData(txtBuscar.Text);
            toolbar.Controls.Add(txtBuscar);

            var btnAgregar = UIHelper.CreatePrimaryButton("＋  Nuevo Producto", new Size(160, 40), new Point(340, 6));
            btnAgregar.Click += (s, e) => OpenProductForm(null);
            toolbar.Controls.Add(btnAgregar);

            var btnEditar = UIHelper.CreateSecondaryButton("✏️  Editar", new Size(110, 40), new Point(510, 6));
            btnEditar.Click += (s, e) =>
            {
                if (_selectedProduct == null) { ShowSelectWarning(); return; }
                OpenProductForm(_selectedProduct);
            };
            toolbar.Controls.Add(btnEditar);

            var btnEliminar = UIHelper.CreateDangerButton("🗑  Eliminar", new Size(110, 40), new Point(630, 6));
            btnEliminar.Click += (s, e) =>
            {
                if (_selectedProduct == null) { ShowSelectWarning(); return; }
                ConfirmDelete();
            };
            toolbar.Controls.Add(btnEliminar);

            Controls.Add(toolbar);

            // ── DataGridView card ────────────────────────────────────────
            var tableCard = new CardPanel
            {
                Location = new Point(24, 92),
                Size = new Size(1200, 500),
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
            Controls.Add(tableCard);

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

                // Color critical stock
                if (p.StockActual <= p.StockMinimo)
                {
                    dgvProductos.Rows[r].Cells["Stock"].Style.ForeColor = AppColors.Danger;
                    dgvProductos.Rows[r].Cells["Stock"].Style.Font = AppFonts.BodyBold;
                }
            }

            if (Controls["lblCount"] is Label lbl)
                lbl.Text = $"{productos.Count} producto(s) encontrado(s)";
        }

        private void OpenProductForm(Producto? product)
        {
            var frm = new FrmProducto(product);
            frm.Saved += () => LoadData();
            frm.ShowDialog(this);
        }

        private void ConfirmDelete()
        {
            if (MessageBox.Show($"¿Eliminar el producto \"{_selectedProduct!.Nombre}\"?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _service.Eliminar(_selectedProduct.Id);
                _selectedProduct = null;
                LoadData();
                MessageBox.Show("Producto eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowSelectWarning() =>
            MessageBox.Show("Selecciona un producto de la lista.", "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
