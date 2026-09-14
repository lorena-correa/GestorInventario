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
    // frmlista_CategoriaProductos — Listado de Categorías
    // =========================================================================
    public class frmlista_CategoriaProductos : Form
    {
        private DataGridView dgvCategorias = null!;
        private TextBox txtBuscar = null!;
        private Button btnNuevo = null!;
        private Button btnEditar = null!;
        private Button btnBorrar = null!;
        private readonly CategoriaService _service = new();

        private Categoria? _categoriaSeleccionada;

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
                Size = new Size(1140, 64),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Padding = new Padding(0, 13, 20, 13)
            };

            var lblIcono = new Label
            {
                Text = "🏷️",
                Font = new Font("Segoe UI Emoji", 16f),
                Location = new Point(20, 14),
                Size = new Size(36, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblIcono);

            var lblSubtitulo = new Label
            {
                Text = "Administración y clasificación del catálogo de inventario por familias",
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

            UIHelper.CreateSearchInput(actionsPanel, out txtBuscar, 0, 0, 220, 38, "Buscar categoría...");
            txtBuscar.TextChanged += (s, e) => CargarDatos(txtBuscar.Text);

            btnNuevo = UIHelper.CreatePrimaryButton("＋ NUEVA", new Size(105, 38), new Point(0, 0));
            btnNuevo.Margin = new Padding(6, 0, 0, 0);
            btnNuevo.Click += (s, e) => AbrirFormularioCategoria(null);
            actionsPanel.Controls.Add(btnNuevo);

            btnEditar = UIHelper.CreateEditButton("✏️ EDITAR", new Size(95, 38), new Point(0, 0));
            btnEditar.Margin = new Padding(6, 0, 0, 0);
            btnEditar.Click += (s, e) =>
            {
                if (_categoriaSeleccionada == null)
                {
                    ModernMessageBox.ShowWarning("Seleccione una categoría para editar.", "Selección Requerida");
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
                    ModernMessageBox.ShowWarning("Seleccione una categoría para eliminar.", "Selección Requerida");
                    return;
                }
                if (ModernMessageBox.ShowConfirm($"¿Eliminar categoría {_categoriaSeleccionada.Nombre}?", "Confirmar", "Eliminar") == DialogResult.Yes)
                {
                    try
                    {
                        _service.Eliminar(_categoriaSeleccionada.Id);
                        _categoriaSeleccionada = null;
                        CargarDatos(txtBuscar.Text);
                        ModernMessageBox.ShowSuccess("Categoría eliminada con éxito.");
                    }
                    catch (Exception ex)
                    {
                        ModernMessageBox.ShowError(ex.Message, "Error al eliminar");
                    }
                }
            };
            actionsPanel.Controls.Add(btnBorrar);

            toolbarCard.Controls.Add(actionsPanel);
            Controls.Add(toolbarCard);

            var cardGrid = new CardPanel
            {
                Location = new Point(20, 92),
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
                    _categoriaSeleccionada = dgvCategorias.SelectedRows[0].Tag as Categoria;
            };

            dgvCategorias.CellDoubleClick += (s, e) =>
            {
                if (_categoriaSeleccionada != null)
                    AbrirFormularioCategoria(_categoriaSeleccionada);
            };

            cardGrid.Controls.Add(dgvCategorias);
            UIHelper.BindEmptyState(dgvCategorias, "No hay categorías registradas todavía.");
            Controls.Add(cardGrid);

            UIHelper.BindFillWidth(this, toolbarCard, 20);
            UIHelper.BindFillWidth(this, cardGrid, 20);
            UIHelper.BindFillHeight(this, cardGrid, 24);

            ResumeLayout();
        }

        private void CargarDatos(string filtro = "")
        {
            try
            {
                dgvCategorias.Rows.Clear();
                var categorias = string.IsNullOrEmpty(filtro) ? _service.ObtenerTodos() : _service.Buscar(filtro);
                foreach (var c in categorias)
                {
                    int r = dgvCategorias.Rows.Add(c.Id, c.Nombre, c.Descripcion, $"{c.TotalProductos} ítems",
                        c.Activo ? "Activa" : "Inactiva");
                    dgvCategorias.Rows[r].Tag = c;
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError($"Error al cargar categorías: {ex.Message}");
            }
        }

        private void AbrirFormularioCategoria(Categoria? cat)
        {
            var form = new frmCategoriaProductos(cat);
            form.CategoriaGuardada += () => CargarDatos(txtBuscar.Text);
            form.ShowDialog(this);
        }
    }

    // =========================================================================
    // frmCategoriaProductos — Formulario de Categoría (Con ErrorProvider)
    // =========================================================================
    public class frmCategoriaProductos : Form
    {
        public event Action? CategoriaGuardada;
        private readonly Categoria? _categoria;
        private readonly bool _esEdicion;
        private readonly CategoriaService _service = new();

        // Controles de la guía
        private TextBox txtNombreCategoria = null!;
        private TextBox txtDescripcion = null!;
        private Button btnActualizar = null!;
        private Button btnSalir = null!;
        private ErrorProvider errValidador = null!;

        public frmCategoriaProductos(Categoria? categoria = null)
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
                ModernMessageBox.ShowWarning("Por favor complete el nombre de la categoría.", "Validación");
                return;
            }

            var cat = _esEdicion ? _categoria! : new Categoria();
            cat.Nombre = txtNombreCategoria.Text.Trim();
            cat.Descripcion = txtDescripcion.Text.Trim();

            try
            {
                _service.Guardar(cat);
                ModernMessageBox.ShowSuccess("¡Categoría guardada exitosamente!", "Operación Exitosa");
                CategoriaGuardada?.Invoke();
                Close();
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError(ex.Message, "Error al guardar");
            }
        }
    }
}
