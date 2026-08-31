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

            var toolbarCard = new CardPanel
            {
                Location = new Point(24, 16),
                Size = new Size(1140, 72),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            var lblTitulo = new Label
            {
                Text = "🏢  ADMINISTRACIÓN DE PROVEEDORES",
                Font = AppFonts.Heading,
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblTitulo);

            var lblSubtitulo = new Label
            {
                Text = "Gestión de aliados comerciales, contactos y suministros",
                Font = AppFonts.Small,
                ForeColor = AppColors.TextSecondary,
                Location = new Point(18, 42),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblSubtitulo);

            // Contenedor de acciones alineado a la derecha
            var actionsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Location = new Point(480, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent,
                Height = 44
            };

            UIHelper.CreateSearchInput(actionsPanel, out txtBuscar, 0, 0, 220, 38, "Buscar proveedor...");
            txtBuscar.TextChanged += (s, e) => LoadData(txtBuscar.Text);

            var btnNuevo = UIHelper.CreatePrimaryButton("＋ NUEVO", new Size(105, 38), new Point(0, 0));
            btnNuevo.Margin = new Padding(6, 0, 0, 0);
            btnNuevo.Click += (s, e) => OpenForm(null);
            actionsPanel.Controls.Add(btnNuevo);

            var btnEditar = UIHelper.CreateSecondaryButton("✏️ EDITAR", new Size(95, 38), new Point(0, 0));
            btnEditar.Margin = new Padding(6, 0, 0, 0);
            btnEditar.Click += (s, e) => { if (_selected == null) { ShowWarn(); return; } OpenForm(_selected); };
            actionsPanel.Controls.Add(btnEditar);

            var btnEliminar = UIHelper.CreateDangerButton("🗑 BORRAR", new Size(95, 38), new Point(0, 0));
            btnEliminar.Margin = new Padding(6, 0, 0, 0);
            btnEliminar.Click += (s, e) => { if (_selected == null) { ShowWarn(); return; } DeleteSelected(); };
            actionsPanel.Controls.Add(btnEliminar);

            toolbarCard.Controls.Add(actionsPanel);
            Controls.Add(toolbarCard);

            var card = new CardPanel { Location = new Point(24, 100), Size = new Size(1140, 520), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom };
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
            try
            {
                var proveedores = string.IsNullOrEmpty(search)
                    ? _service.ObtenerTodos()
                    : _service.Buscar(search);

                dgvProveedores.Rows.Clear();
                foreach (var p in proveedores)
                {
                    int r = dgvProveedores.Rows.Add(
                        p.Nombre, p.Telefono, p.Correo,
                        p.Direccion, p.Activo ? "Activo" : "Inactivo");
                    dgvProveedores.Rows[r].Tag = p;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar proveedores: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (MessageBox.Show($"¿Eliminar proveedor \"{_selected!.Nombre}\"?",
                "Confirmar", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _service.Eliminar(_selected.Id);
                    _selected = null;
                    LoadData();
                    MessageBox.Show("Proveedor eliminado correctamente.",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowWarn() => MessageBox.Show("Selecciona un proveedor de la lista.", "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ═══════════════════════════════════════════════════════════════
    // FrmProveedor — Supplier detail / create / edit (Modal Estándar)
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
            Size = new Size(540, 500);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;
            BuildUI();
            if (_isEdit) FillData();
        }

        private void BuildUI()
        {
            var header = UIHelper.CreateModalHeader(this, _isEdit ? "Editar Proveedor" : "Nuevo Proveedor", _isEdit ? "✏️" : "➕");
            Controls.Add(header);

            var panelForm = new Panel
            {
                Location = new Point(0, 60),
                Size = new Size(540, 440),
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            int x = 30, y = 15, w = 470, rowH = 72;

            UIHelper.CreateRoundedTextBox(panelForm, "NOMBRE DEL PROVEEDOR *", out txtNombre, x, y, w);      y += rowH;
            UIHelper.CreateRoundedTextBox(panelForm, "TELÉFONO", out txtTelefono, x, y, w);                  y += rowH;
            UIHelper.CreateRoundedTextBox(panelForm, "CORREO ELECTRÓNICO", out txtCorreo, x, y, w);          y += rowH;
            UIHelper.CreateRoundedTextBox(panelForm, "DIRECCIÓN", out txtDireccion, x, y, w);                y += rowH + 10;

            var btnGuardar = UIHelper.CreatePrimaryButton("💾 Guardar Proveedor", new Size(190, 42), new Point(x, y));
            btnGuardar.Click += BtnGuardar_Click;
            panelForm.Controls.Add(btnGuardar);

            var btnCancelar = UIHelper.CreateSecondaryButton("✕ Cancelar", new Size(130, 42), new Point(x + 205, y));
            btnCancelar.Click += (s, e) => Close();
            panelForm.Controls.Add(btnCancelar);

            Controls.Add(panelForm);
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
                MessageBox.Show("El nombre del proveedor es obligatorio.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            if (!string.IsNullOrEmpty(txtCorreo.Text) && !txtCorreo.Text.Contains('@'))
            {
                MessageBox.Show("El correo electrónico no es válido.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCorreo.Focus();
                return;
            }

            var p = _isEdit ? _proveedor! : new Proveedor();
            p.Nombre = txtNombre.Text.Trim();
            p.Telefono = txtTelefono.Text.Trim();
            p.Correo = txtCorreo.Text.Trim();
            p.Direccion = txtDireccion.Text.Trim();
            p.Activo = true;

            try
            {
                _service.Guardar(p);
                MessageBox.Show("Proveedor guardado correctamente.",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Saved?.Invoke();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
