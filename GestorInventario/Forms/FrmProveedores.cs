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
    // FrmProveedores — Supplier list
    // ═══════════════════════════════════════════════════════════════
    public class FrmProveedores : Form
    {
        private DataGridView dgvProveedores = null!;
        private TextBox txtBuscar = null!;
        private readonly ProveedorService _service = new();
        private Proveedor? _selected;

        public FrmProveedores()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            SuspendLayout();

            var toolbar = new Panel { Location = new Point(24, 24), Size = new Size(1100, 52), BackColor = Color.Transparent };

            var lblSearch = new Label { Text = "🔍", Font = new Font("Segoe UI Emoji", 12f), Location = new Point(0, 12), AutoSize = true, BackColor = Color.Transparent };
            toolbar.Controls.Add(lblSearch);

            txtBuscar = new TextBox { Location = new Point(28, 8), Size = new Size(280, 36), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Buscar proveedor..." };
            txtBuscar.TextChanged += (s, e) => LoadData(txtBuscar.Text);
            toolbar.Controls.Add(txtBuscar);

            var btnNuevo = UIHelper.CreatePrimaryButton("＋  Nuevo Proveedor", new Size(170, 40), new Point(320, 6));
            btnNuevo.Click += (s, e) => OpenForm(null);
            toolbar.Controls.Add(btnNuevo);

            var btnEditar = UIHelper.CreateSecondaryButton("✏️  Editar", new Size(110, 40), new Point(500, 6));
            btnEditar.Click += (s, e) => { if (_selected == null) { ShowWarn(); return; } OpenForm(_selected); };
            toolbar.Controls.Add(btnEditar);

            var btnEliminar = UIHelper.CreateDangerButton("🗑  Eliminar", new Size(110, 40), new Point(620, 6));
            btnEliminar.Click += (s, e) => { if (_selected == null) { ShowWarn(); return; } DeleteSelected(); };
            toolbar.Controls.Add(btnEliminar);

            Controls.Add(toolbar);

            var card = new CardPanel { Location = new Point(24, 92), Size = new Size(1100, 480), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom };
            dgvProveedores = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvProveedores);
            dgvProveedores.Columns.Add("Nombre", "Nombre");
            dgvProveedores.Columns.Add("Telefono", "Teléfono");
            dgvProveedores.Columns.Add("Correo", "Correo");
            dgvProveedores.Columns.Add("Direccion", "Dirección");
            dgvProveedores.Columns.Add("Estado", "Estado");
            dgvProveedores.SelectionChanged += (s, e) =>
            {
                if (dgvProveedores.SelectedRows.Count > 0) _selected = dgvProveedores.SelectedRows[0].Tag as Proveedor;
            };
            dgvProveedores.CellDoubleClick += (s, e) => { if (_selected != null) OpenForm(_selected); };
            card.Controls.Add(dgvProveedores);
            Controls.Add(card);
            ResumeLayout();
        }

        private void LoadData(string search = "")
        {
            dgvProveedores.Rows.Clear();
            foreach (var p in _service.ObtenerTodos())
            {
                if (!string.IsNullOrEmpty(search) && !p.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase)) continue;
                int r = dgvProveedores.Rows.Add(p.Nombre, p.Telefono, p.Correo, p.Direccion, p.Activo ? "Activo" : "Inactivo");
                dgvProveedores.Rows[r].Tag = p;
            }
        }

        private void OpenForm(Proveedor? p)
        {
            var frm = new FrmProveedor(p);
            frm.Saved += () => LoadData();
            frm.ShowDialog(this);
        }

        private void DeleteSelected()
        {
            if (MessageBox.Show($"¿Eliminar proveedor \"{_selected!.Nombre}\"?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _service.Eliminar(_selected.Id);
                _selected = null;
                LoadData();
            }
        }

        private void ShowWarn() => MessageBox.Show("Selecciona un proveedor de la lista.", "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ═══════════════════════════════════════════════════════════════
    // FrmProveedor — Supplier detail / create / edit
    // ═══════════════════════════════════════════════════════════════
    public class FrmProveedor : Form
    {
        public event Action? Saved;
        private readonly Proveedor? _proveedor;
        private readonly bool _isEdit;
        private readonly ProveedorService _service = new();

        private TextBox txtNombre = null!, txtTelefono = null!, txtCorreo = null!, txtDireccion = null!;

        public FrmProveedor(Proveedor? proveedor = null)
        {
            _proveedor = proveedor;
            _isEdit = proveedor != null;
            Size = new Size(540, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;
            BuildUI();
            if (_isEdit) FillData();
        }

        private void BuildUI()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = AppColors.Primary };
            var lblTitle = new Label { Text = _isEdit ? "✏️  Editar Proveedor" : "➕  Nuevo Proveedor", Font = AppFonts.SubHeading, ForeColor = Color.White, Location = new Point(20, 18), AutoSize = true };
            var btnX = new Label { Text = "✕", Font = new Font("Segoe UI", 16f), ForeColor = Color.White, Cursor = Cursors.Hand, Location = new Point(500, 16), AutoSize = true };
            btnX.Click += (s, e) => Close();
            header.Controls.AddRange(new Control[] { lblTitle, btnX });
            Controls.Add(header);

            int x = 30, y = 80, w = 460;

            AddField("NOMBRE DEL PROVEEDOR *", out txtNombre, x, y, w);      y += 80;
            AddField("TELÉFONO", out txtTelefono, x, y, w);                  y += 80;
            AddField("CORREO ELECTRÓNICO", out txtCorreo, x, y, w);          y += 80;
            AddField("DIRECCIÓN", out txtDireccion, x, y, w);                y += 80;

            var btnGuardar = UIHelper.CreatePrimaryButton("💾  Guardar Proveedor", new Size(200, 44), new Point(x, y + 10));
            btnGuardar.Click += BtnGuardar_Click;
            Controls.Add(btnGuardar);

            var btnCancelar = UIHelper.CreateSecondaryButton("✕  Cancelar", new Size(130, 44), new Point(x + 210, y + 10));
            btnCancelar.Click += (s, e) => Close();
            Controls.Add(btnCancelar);
        }

        private void AddField(string label, out TextBox txt, int x, int y, int width)
        {
            var lbl = new Label { Text = label, Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true };
            txt = new TextBox { Location = new Point(x, y + 22), Size = new Size(width, 36), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(lbl);
            Controls.Add(txt);
        }

        private void FillData()
        {
            txtNombre.Text = _proveedor!.Nombre;
            txtTelefono.Text = _proveedor.Telefono;
            txtCorreo.Text = _proveedor.Correo;
            txtDireccion.Text = _proveedor.Direccion;
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del proveedor es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var p = _isEdit ? _proveedor! : new Proveedor();
            p.Nombre = txtNombre.Text.Trim();
            p.Telefono = txtTelefono.Text.Trim();
            p.Correo = txtCorreo.Text.Trim();
            p.Direccion = txtDireccion.Text.Trim();
            p.Activo = true;

            _service.Guardar(p);
            MessageBox.Show("Proveedor guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Saved?.Invoke();
            Close();
        }
    }
}
