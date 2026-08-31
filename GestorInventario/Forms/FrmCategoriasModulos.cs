using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;

namespace GestorInventario.Forms
{
    // =========================================================================
    // frmlista_CategoriaProductos — Listado de Categorías
    // =========================================================================
    public class frmlista_CategoriaProductos : Form
    {
        private DataGridView dgvCategorias = null!;
        private TextBox txtBuscar = null!;
        private Button btnNuevo = null!;
        private Button btnEditar = null!;
        private Button btnBorrar = null!;

        private readonly List<CategoriaItem> _listaCategorias = new()
        {
            new CategoriaItem { Id = 1, Nombre = "Electrónica y Pantallas", Descripcion = "Monitores, televisores, pantallas LED y accesorios visuales", TotalProductos = 14 },
            new CategoriaItem { Id = 2, Nombre = "Periféricos y Controles", Descripcion = "Teclados, mouse, audífonos, micrófonos y gamepads", TotalProductos = 28 },
            new CategoriaItem { Id = 3, Nombre = "Cables y Conectores", Descripcion = "Cables HDMI, DisplayPort, USB-C, adaptadores y patch cords", TotalProductos = 42 },
            new CategoriaItem { Id = 4, Nombre = "Almacenamiento Digital", Descripcion = "Discos SSD, discos duros externos, memorias RAM y tarjetas SD", TotalProductos = 19 },
            new CategoriaItem { Id = 5, Nombre = "Redes y Conectividad", Descripcion = "Routers Wi-Fi 6, switches, access points y tarjetas de red", TotalProductos = 11 },
            new CategoriaItem { Id = 6, Nombre = "Componentes de PC", Descripcion = "Fuentes de poder, tarjetas de video, procesadores y disipadores", TotalProductos = 9 }
        };

        private CategoriaItem? _categoriaSeleccionada;

        public frmlista_CategoriaProductos()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            SuspendLayout();

            var toolbarCard = new CardPanel
            {
                Location = new Point(20, 16),
                Size = new Size(1140, 72),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            var lblTitulo = new Label
            {
                Text = "🏷️  CATEGORÍAS DE PRODUCTOS",
                Font = AppFonts.Heading,
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblTitulo);

            var lblSubtitulo = new Label
            {
                Text = "Administración y clasificación del catálogo de inventario por familias",
                Font = AppFonts.Small,
                ForeColor = AppColors.TextSecondary,
                Location = new Point(18, 42),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblSubtitulo);

            // Contenedor de acciones alineado a la derecha sin superposiciones
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

            UIHelper.CreateSearchInput(actionsPanel, out txtBuscar, 0, 0, 220, 38, "Buscar categoría...");
            txtBuscar.TextChanged += (s, e) => CargarDatos(txtBuscar.Text);

            btnNuevo = UIHelper.CreatePrimaryButton("＋ NUEVA", new Size(105, 38), new Point(0, 0));
            btnNuevo.Margin = new Padding(6, 0, 0, 0);
            btnNuevo.Click += (s, e) => AbrirFormularioCategoria(null);
            actionsPanel.Controls.Add(btnNuevo);

            btnEditar = UIHelper.CreateSecondaryButton("✏️ EDITAR", new Size(95, 38), new Point(0, 0));
            btnEditar.Margin = new Padding(6, 0, 0, 0);
            btnEditar.Click += (s, e) =>
            {
                if (_categoriaSeleccionada == null)
                {
                    MessageBox.Show("Seleccione una categoría para editar.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                AbrirFormularioCategoria(_categoriaSeleccionada);
            };
            actionsPanel.Controls.Add(btnEditar);

            btnBorrar = UIHelper.CreateDangerButton("🗑 BORRAR", new Size(95, 38), new Point(0, 0));
            btnBorrar.Margin = new Padding(6, 0, 0, 0);
            btnBorrar.Click += (s, e) =>
            {
                if (_categoriaSeleccionada == null)
                {
                    MessageBox.Show("Seleccione una categoría para eliminar.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (MessageBox.Show($"¿Eliminar categoría {_categoriaSeleccionada.Nombre}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _listaCategorias.Remove(_categoriaSeleccionada);
                    _categoriaSeleccionada = null;
                    CargarDatos(txtBuscar.Text);
                    MessageBox.Show("Categoría eliminada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            actionsPanel.Controls.Add(btnBorrar);

            toolbarCard.Controls.Add(actionsPanel);
            Controls.Add(toolbarCard);

            var cardGrid = new CardPanel
            {
                Location = new Point(20, 100),
                Size = new Size(1140, 520),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            dgvCategorias = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvCategorias);

            dgvCategorias.Columns.Add("ID", "ID");
            dgvCategorias.Columns.Add("Nombre", "NOMBRE CATEGORÍA");
            dgvCategorias.Columns.Add("Descripcion", "DESCRIPCIÓN");
            dgvCategorias.Columns.Add("Total", "TOTAL PRODUCTOS");
            dgvCategorias.Columns.Add("Estado", "ESTADO");

            dgvCategorias.Columns["ID"].Width = 70;
            dgvCategorias.Columns["Nombre"].Width = 280;
            dgvCategorias.Columns["Descripcion"].Width = 450;
            dgvCategorias.Columns["Total"].Width = 150;

            dgvCategorias.SelectionChanged += (s, e) =>
            {
                if (dgvCategorias.SelectedRows.Count > 0)
                    _categoriaSeleccionada = dgvCategorias.SelectedRows[0].Tag as CategoriaItem;
            };

            dgvCategorias.CellDoubleClick += (s, e) =>
            {
                if (_categoriaSeleccionada != null)
                    AbrirFormularioCategoria(_categoriaSeleccionada);
            };

            cardGrid.Controls.Add(dgvCategorias);
            Controls.Add(cardGrid);

            ResumeLayout();
        }

        private void CargarDatos(string filtro = "")
        {
            dgvCategorias.Rows.Clear();
            foreach (var c in _listaCategorias)
            {
                if (!string.IsNullOrEmpty(filtro) &&
                    !c.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase) &&
                    !c.Descripcion.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    continue;

                int r = dgvCategorias.Rows.Add(c.Id, c.Nombre, c.Descripcion, $"{c.TotalProductos} ítems", "Activa");
                dgvCategorias.Rows[r].Tag = c;
            }
        }

        private void AbrirFormularioCategoria(CategoriaItem? cat)
        {
            var form = new frmCategoriaProductos(cat);
            form.CategoriaGuardada += (nuevaCat) =>
            {
                if (cat == null)
                {
                    nuevaCat.Id = _listaCategorias.Count + 1;
                    _listaCategorias.Add(nuevaCat);
                }
                else
                {
                    cat.Nombre = nuevaCat.Nombre;
                    cat.Descripcion = nuevaCat.Descripcion;
                }
                CargarDatos(txtBuscar.Text);
            };
            form.ShowDialog(this);
        }
    }

    // =========================================================================
    // frmCategoriaProductos — Formulario de Categoría (Con ErrorProvider)
    // =========================================================================
    public class frmCategoriaProductos : Form
    {
        public event Action<CategoriaItem>? CategoriaGuardada;
        private readonly CategoriaItem? _categoria;
        private readonly bool _esEdicion;

        // Controles de la guía
        private TextBox txtNombreCategoria = null!;
        private TextBox txtDescripcion = null!;
        private Button btnActualizar = null!;
        private Button btnSalir = null!;
        private ErrorProvider errValidador = null!;

        public frmCategoriaProductos(CategoriaItem? categoria = null)
        {
            _categoria = categoria;
            _esEdicion = categoria != null;

            Size = new Size(540, 430);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;

            BuildUI();
            if (_esEdicion) LlenarDatos();
        }

        private void BuildUI()
        {
            errValidador = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // Encabezado estándar unificado con la paleta de la aplicación
            var header = UIHelper.CreateModalHeader(this,
                _esEdicion ? "CATEGORÍA DE PRODUCTOS (EDITAR)" : "CATEGORÍA DE PRODUCTOS",
                "🏷️");
            Controls.Add(header);

            var panelForm = new Panel
            {
                Location = new Point(0, 60),
                Size = new Size(540, 370),
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            int x = 36, y = 15, width = 460;

            // Nombre Categoría (8px radius)
            UIHelper.CreateRoundedTextBox(panelForm, "Nombre Categoría *", out txtNombreCategoria, x, y, width);
            y += 72;

            // Descripción (8px radius)
            UIHelper.CreateRoundedTextBox(panelForm, "Descripción de la Categoría", out txtDescripcion, x, y, width, 85, multiline: true);
            y += 115;

            // Botones: ACTUALIZAR y SALIR con 8px radius
            btnActualizar = UIHelper.CreatePrimaryButton("ACTUALIZAR", new Size(180, 42), new Point(x, y));
            btnActualizar.Click += BtnActualizar_Click;
            panelForm.Controls.Add(btnActualizar);

            btnSalir = UIHelper.CreateSecondaryButton("SALIR", new Size(140, 42), new Point(x + 195, y));
            btnSalir.Click += (s, e) => Close();
            panelForm.Controls.Add(btnSalir);

            Controls.Add(panelForm);
        }

        private void LlenarDatos()
        {
            txtNombreCategoria.Text = _categoria!.Nombre;
            txtDescripcion.Text = _categoria.Descripcion;
        }

        private void BtnActualizar_Click(object? sender, EventArgs e)
        {
            errValidador.Clear();
            if (string.IsNullOrWhiteSpace(txtNombreCategoria.Text))
            {
                errValidador.SetError(txtNombreCategoria, "El Nombre de la Categoría es obligatorio.");
                MessageBox.Show("Por favor complete el nombre de la categoría.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cat = new CategoriaItem
            {
                Nombre = txtNombreCategoria.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim(),
                TotalProductos = _categoria?.TotalProductos ?? 0
            };

            CategoriaGuardada?.Invoke(cat);
            MessageBox.Show("¡Categoría guardada exitosamente!", "Operación Exitosa",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }

    public class CategoriaItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int TotalProductos { get; set; }
    }
}
