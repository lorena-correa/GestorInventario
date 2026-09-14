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
    // frmLista_Clientes — Listado de Clientes (CRUD Maestro)
    // =========================================================================
    public class frmLista_Clientes : Form
    {
        private DataGridView dgvClientes = null!;
        private TextBox txtBuscar = null!;
        private Button btnNuevo = null!;
        private Button btnEditar = null!;
        private Button btnBorrar = null!;
        private readonly ClienteService _service = new();

        private Cliente? _clienteSeleccionado;

        public frmLista_Clientes()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
            CargarDatos();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // Panel Superior / Barra de Herramientas
            var toolbarCard = new CardPanel
            {
                Location = new Point(20, 16),
                Size = new Size(1140, 64),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Padding = new Padding(0, 13, 20, 13)
            };

            var lblIcono = new Label
            {
                Text = "👥",
                Font = new Font("Segoe UI Emoji", 16f),
                Location = new Point(20, 14),
                Size = new Size(36, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblIcono);

            var lblSubtitulo = new Label
            {
                Text = "Consulte, agregue, modifique o elimine clientes registrados en el sistema",
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

            UIHelper.CreateSearchInput(actionsPanel, out txtBuscar, 0, 0, 220, 38, "Buscar cliente...");
            txtBuscar.TextChanged += (s, e) => CargarDatos(txtBuscar.Text);

            btnNuevo = UIHelper.CreatePrimaryButton("＋ NUEVO", new Size(105, 38), new Point(0, 0));
            btnNuevo.Margin = new Padding(6, 0, 0, 0);
            btnNuevo.Click += (s, e) => AbrirFormularioCliente(null);
            actionsPanel.Controls.Add(btnNuevo);

            btnEditar = UIHelper.CreateEditButton("✏️ EDITAR", new Size(95, 38), new Point(0, 0));
            btnEditar.Margin = new Padding(6, 0, 0, 0);
            btnEditar.Click += (s, e) =>
            {
                if (_clienteSeleccionado == null)
                {
                    ModernMessageBox.ShowWarning("Por favor seleccione un cliente de la lista para editar.", "Selección Requerida");
                    return;
                }
                AbrirFormularioCliente(_clienteSeleccionado);
            };
            actionsPanel.Controls.Add(btnEditar);

            btnBorrar = UIHelper.CreateDangerButton("🗑 BORRAR", new Size(95, 38), new Point(0, 0));
            btnBorrar.Margin = new Padding(6, 0, 0, 0);
            btnBorrar.Click += (s, e) =>
            {
                if (_clienteSeleccionado == null)
                {
                    ModernMessageBox.ShowWarning("Por favor seleccione un cliente de la lista para eliminar.", "Selección Requerida");
                    return;
                }
                if (ModernMessageBox.ShowConfirm($"¿Desea eliminar al cliente {_clienteSeleccionado.Nombre}?", "Confirmar Eliminación", "Eliminar") == DialogResult.Yes)
                {
                    try
                    {
                        _service.Eliminar(_clienteSeleccionado.Id);
                        _clienteSeleccionado = null;
                        CargarDatos(txtBuscar.Text);
                        ModernMessageBox.ShowSuccess("Cliente eliminado con éxito.");
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

            // Contenedor DataGridView
            var cardGrid = new CardPanel
            {
                Location = new Point(20, 92),
                Size = new Size(1140, 520),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            dgvClientes = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvClientes);

            dgvClientes.Columns.Add("ID", "ID");
            dgvClientes.Columns.Add("Cliente", "CLIENTE");
            dgvClientes.Columns.Add("Documento", "DOCUMENTO");
            dgvClientes.Columns.Add("Telefono", "TELÉFONO");
            dgvClientes.Columns.Add("Email", "EMAIL");
            dgvClientes.Columns.Add("Direccion", "DIRECCIÓN");
            dgvClientes.Columns.Add("Estado", "ESTADO");

            dgvClientes.Columns["ID"].Width = 60;
            dgvClientes.Columns["Cliente"].Width = 240;
            dgvClientes.Columns["Documento"].Width = 140;
            dgvClientes.Columns["Telefono"].Width = 130;
            dgvClientes.Columns["Email"].Width = 220;

            dgvClientes.SelectionChanged += (s, e) =>
            {
                if (dgvClientes.SelectedRows.Count > 0)
                    _clienteSeleccionado = dgvClientes.SelectedRows[0].Tag as Cliente;
            };

            dgvClientes.CellDoubleClick += (s, e) =>
            {
                if (_clienteSeleccionado != null)
                    AbrirFormularioCliente(_clienteSeleccionado);
            };

            cardGrid.Controls.Add(dgvClientes);
            UIHelper.BindEmptyState(dgvClientes, "No hay clientes registrados todavía.");

            UIHelper.BindFillWidth(this, toolbarCard, 20);
            UIHelper.BindFillWidth(this, cardGrid, 20);
            UIHelper.BindFillHeight(this, cardGrid, 24);
            Controls.Add(cardGrid);

            ResumeLayout();
        }

        private void CargarDatos(string filtro = "")
        {
            try
            {
                dgvClientes.Rows.Clear();
                var clientes = string.IsNullOrEmpty(filtro) ? _service.ObtenerTodos() : _service.Buscar(filtro);
                foreach (var c in clientes)
                {
                    int rowIndex = dgvClientes.Rows.Add(c.Id, c.Nombre, c.Documento, c.Telefono, c.Email, c.Direccion,
                        c.Activo ? "Activo" : "Inactivo");
                    dgvClientes.Rows[rowIndex].Tag = c;
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError($"Error al cargar clientes: {ex.Message}");
            }
        }

        private void AbrirFormularioCliente(Cliente? cliente)
        {
            var form = new frmClientes(cliente);
            form.ClienteGuardado += () => CargarDatos(txtBuscar.Text);
            form.ShowDialog(this);
        }
    }

    // =========================================================================
    // frmClientes — Formulario de Edición / Nuevo Registro Cliente (Con ErrorProvider)
    // =========================================================================
    public class frmClientes : Form
    {
        public event Action? ClienteGuardado;
        private readonly Cliente? _cliente;
        private readonly bool _esEdicion;
        private readonly ClienteService _service = new();

        // Controles con nomenclatura estándar de la guía
        private TextBox txtNombreCliente = null!;
        private TextBox txtDocumento = null!;
        private TextBox txtDireccion = null!;
        private TextBox txtTelefono = null!;
        private TextBox txtEmail = null!;
        private Button btnActualizar = null!;
        private Button btnSalir = null!;
        private ErrorProvider errValidador = null!;

        public frmClientes(Cliente? cliente = null)
        {
            _cliente = cliente;
            _esEdicion = cliente != null;

            Size = new Size(580, 580);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;

            BuildUI();
            if (_esEdicion) LlenarDatos();
        }

        private void BuildUI()
        {
            errValidador = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // Encabezado moderno unificado
            var header = UIHelper.CreateModalHeader(this,
                _esEdicion ? "EDITAR REGISTRO CLIENTE" : "NUEVO REGISTRO CLIENTE",
                _esEdicion ? "✏️" : "👤");
            Controls.Add(header);

            // Contenedor campos
            var panelForm = new Panel
            {
                Location = new Point(0, 60),
                Size = new Size(580, 520),
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            int x = 36, y = 15, width = 500, rowSpacing = 72;

            // Campos del formulario con radio 8px consistente
            UIHelper.CreateRoundedTextBox(panelForm, "Nombre Cliente *", out txtNombreCliente, x, y, width);
            y += rowSpacing;

            UIHelper.CreateRoundedTextBox(panelForm, "Documento / Cédula / NIT *", out txtDocumento, x, y, width);
            y += rowSpacing;

            UIHelper.CreateRoundedTextBox(panelForm, "Dirección", out txtDireccion, x, y, width);
            y += rowSpacing;

            UIHelper.CreateRoundedTextBox(panelForm, "Teléfono / Celular *", out txtTelefono, x, y, width);
            y += rowSpacing;

            UIHelper.CreateRoundedTextBox(panelForm, "Email / Correo Electrónico *", out txtEmail, x, y, width);
            y += rowSpacing + 10;

            // Botones según la guía: ACTUALIZAR y SALIR con 8px radio
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
            txtNombreCliente.Text = _cliente!.Nombre;
            txtDocumento.Text = _cliente.Documento;
            txtDireccion.Text = _cliente.Direccion;
            txtTelefono.Text = _cliente.Telefono;
            txtEmail.Text = _cliente.Email;
        }

        private void BtnActualizar_Click(object? sender, EventArgs e)
        {
            // Limpiar errores previos
            errValidador.Clear();
            bool hayErrores = false;

            // Validación estricta con ErrorProvider requerida por la guía
            if (string.IsNullOrWhiteSpace(txtNombreCliente.Text))
            {
                errValidador.SetError(txtNombreCliente, "El Nombre del Cliente es obligatorio.");
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                errValidador.SetError(txtDocumento, "El Documento es obligatorio.");
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(txtTelefono.Text))
            {
                errValidador.SetError(txtTelefono, "El Teléfono es obligatorio.");
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                errValidador.SetError(txtEmail, "El Email es obligatorio.");
                hayErrores = true;
            }
            else if (!txtEmail.Text.Contains('@') || !txtEmail.Text.Contains('.'))
            {
                errValidador.SetError(txtEmail, "Ingrese un formato de correo electrónico válido (ej: usuario@empresa.com).");
                hayErrores = true;
            }

            if (hayErrores)
            {
                ModernMessageBox.ShowWarning("Por favor verifique los campos marcados con error antes de continuar.", "Validación de Campos");
                return;
            }

            var item = _esEdicion ? _cliente! : new Cliente();
            item.Nombre = txtNombreCliente.Text.Trim();
            item.Documento = txtDocumento.Text.Trim();
            item.Direccion = txtDireccion.Text.Trim();
            item.Telefono = txtTelefono.Text.Trim();
            item.Email = txtEmail.Text.Trim();

            try
            {
                _service.Guardar(item);
                ModernMessageBox.ShowSuccess("¡Registro de cliente procesado exitosamente!", "Operación Exitosa");
                ClienteGuardado?.Invoke();
                Close();
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError(ex.Message, "Error al guardar");
            }
        }
    }
}
