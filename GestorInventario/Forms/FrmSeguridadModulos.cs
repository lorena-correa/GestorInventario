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
    // =========================================================================
    // frmlista_Empleados — Listado de Empleados (CRUD Maestro)
    // =========================================================================
    public class frmlista_Empleados : Form
    {
        private DataGridView dgvEmpleados = null!;
        private TextBox txtBuscar = null!;
        private Button btnNuevo = null!;
        private readonly EmpleadoService _service = new();

        private Empleado? _empleadoSeleccionado;

        public frmlista_Empleados()
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
                Text = "👨‍💼",
                Font = new Font("Segoe UI Emoji", 16f),
                Location = new Point(20, 14),
                Size = new Size(36, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblIcono);

            var lblSubtitulo = new Label
            {
                Text = "Gestión del personal de la empresa, roles laborales y control de accesos",
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

            UIHelper.CreateSearchInput(actionsPanel, out txtBuscar, 0, 0, 220, 38, "Buscar empleado...");
            txtBuscar.TextChanged += (s, e) => CargarDatos(txtBuscar.Text);

            btnNuevo = UIHelper.CreatePrimaryButton("＋ NUEVO", new Size(105, 38), new Point(0, 0));
            btnNuevo.Margin = new Padding(6, 0, 0, 0);
            btnNuevo.Click += (s, e) => AbrirFormularioEmpleado(null);
            actionsPanel.Controls.Add(btnNuevo);

            var btnSalir = UIHelper.CreateSecondaryButton("SALIR", new Size(90, 38), new Point(0, 0));
            btnSalir.Margin = new Padding(6, 0, 0, 0);
            btnSalir.Click += (s, e) => FrmMain.CerrarModulo(this);
            actionsPanel.Controls.Add(btnSalir);

            toolbarCard.Controls.Add(actionsPanel);
            Controls.Add(toolbarCard);

            var cardGrid = new CardPanel
            {
                Location = new Point(20, 92),
                Size = new Size(1140, 520),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            dgvEmpleados = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvEmpleados);

            dgvEmpleados.Columns.Add("ID", "ID");
            dgvEmpleados.Columns.Add("Nombre", "NOMBRE EMPLEADO");
            dgvEmpleados.Columns.Add("Documento", "DOCUMENTO");
            dgvEmpleados.Columns.Add("Rol", "ROL ASIGNADO");
            dgvEmpleados.Columns.Add("Telefono", "TELÉFONO");
            dgvEmpleados.Columns.Add("Email", "EMAIL");
            dgvEmpleados.Columns.Add("FechaIngreso", "F. INGRESO");
            dgvEmpleados.Columns.Add("Estado", "ESTADO");

            dgvEmpleados.Columns["ID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvEmpleados.Columns["ID"].Width = 60;
            dgvEmpleados.Columns["Nombre"].Width = 220;
            dgvEmpleados.Columns["Documento"].Width = 130;
            dgvEmpleados.Columns["Rol"].Width = 190;
            dgvEmpleados.Columns["FechaIngreso"].Width = 120;

            // Botones Editar / Borrar dentro del grid
            UIHelper.AddGridActionButtons<Empleado>(dgvEmpleados, AbrirFormularioEmpleado, BorrarEmpleado);

            dgvEmpleados.SelectionChanged += (s, e) =>
            {
                if (dgvEmpleados.SelectedRows.Count > 0)
                    _empleadoSeleccionado = dgvEmpleados.SelectedRows[0].Tag as Empleado;
            };

            dgvEmpleados.CellDoubleClick += (s, e) =>
            {
                if (_empleadoSeleccionado != null)
                    AbrirFormularioEmpleado(_empleadoSeleccionado);
            };

            cardGrid.Controls.Add(dgvEmpleados);
            UIHelper.BindEmptyState(dgvEmpleados, "No hay empleados registrados todavía.");
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
                dgvEmpleados.Rows.Clear();
                var empleados = string.IsNullOrEmpty(filtro) ? _service.ObtenerTodos() : _service.Buscar(filtro);
                foreach (var emp in empleados)
                {
                    int r = dgvEmpleados.Rows.Add(
                        emp.Id,
                        emp.Nombre,
                        emp.Documento,
                        emp.Rol,
                        emp.Telefono,
                        emp.Email,
                        emp.FechaIngreso.ToString("dd/MM/yyyy"),
                        emp.Activo ? "Activo" : "Inactivo"
                    );
                    dgvEmpleados.Rows[r].Tag = emp;
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError($"Error al cargar empleados: {ex.Message}");
            }
        }

        private void AbrirFormularioEmpleado(Empleado? emp)
        {
            var form = new frmEmpleados(emp);
            form.EmpleadoGuardado += () => CargarDatos(txtBuscar.Text);
            form.ShowDialog(this);
        }

        private void BorrarEmpleado(Empleado emp)
        {
            if (ModernMessageBox.ShowConfirm($"¿Eliminar al empleado {emp.Nombre}?", "Confirmar Eliminación", "Eliminar") == DialogResult.Yes)
            {
                try
                {
                    _service.Eliminar(emp.Id);
                    _empleadoSeleccionado = null;
                    CargarDatos(txtBuscar.Text);
                    ModernMessageBox.ShowSuccess("Empleado eliminado con éxito.");
                }
                catch (Exception ex)
                {
                    ModernMessageBox.ShowError(ex.Message, "Error al eliminar");
                }
            }
        }
    }

    // =========================================================================
    // frmEmpleados — Formulario de Empleado (Con DateTimePicker y ErrorProvider)
    // =========================================================================
    public class frmEmpleados : Form
    {
        public event Action? EmpleadoGuardado;
        private readonly Empleado? _empleado;
        private readonly bool _esEdicion;
        private readonly EmpleadoService _service = new();

        // Controles de la guía
        private TextBox txtNombreEmpleado = null!;
        private ComboBox cboRolEmpleado = null!;
        private TextBox txtDocumento = null!;
        private DateTimePicker dtpFechaIngreso = null!;
        private TextBox txtDireccion = null!;
        private DateTimePicker dtpFechaRetiro = null!;
        private TextBox txtTelefono = null!;
        private TextBox txtEmail = null!;
        private TextBox txtDatosAdicionales = null!;
        private Button btnActualizar = null!;
        private Button btnSalir = null!;
        private ErrorProvider errValidador = null!;

        public frmEmpleados(Empleado? empleado = null)
        {
            _empleado = empleado;
            _esEdicion = empleado != null;

            Size = new Size(820, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;

            BuildUI();
            if (_esEdicion) LlenarDatos();
        }

        private void BuildUI()
        {
            errValidador = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // Encabezado estándar unificado con paleta morada de la aplicación
            var header = UIHelper.CreateModalHeader(this,
                _esEdicion ? "ADMINISTRACIÓN DE EMPLEADOS (EDITAR)" : "ADMINISTRACIÓN DE EMPLEADOS (NUEVO)",
                _esEdicion ? "✏️" : "👨‍💼");
            Controls.Add(header);

            var panelForm = new Panel
            {
                Location = new Point(0, 60),
                Size = new Size(820, 560),
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };

            int col1 = 36, col2 = 420, widthCol = 360, y = 15, rowH = 68;

            // Fila 1: Nombre Empleado | Rol Empleado
            UIHelper.CreateRoundedTextBox(panelForm, "Nombre Empleado *", out txtNombreEmpleado, col1, y, widthCol);
            UIHelper.CreateRoundedComboBox(panelForm, "Rol Empleado *", out cboRolEmpleado, col2, y, widthCol);
            cboRolEmpleado.Items.AddRange(new[] { "Administrador del Sistema", "Cajero / Facturación", "Almacenista", "Vendedor / Mostrador", "Supervisor de Inventario" });
            cboRolEmpleado.SelectedIndex = 1;
            y += rowH;

            // Fila 2: Documento | F. Ingreso (DateTimePicker)
            UIHelper.CreateRoundedTextBox(panelForm, "Documento *", out txtDocumento, col1, y, widthCol);
            UIHelper.CreateRoundedDateTimePicker(panelForm, "F. Ingreso *", out dtpFechaIngreso, col2, y, widthCol);
            y += rowH;

            // Fila 3: Dirección | F. Retiro
            UIHelper.CreateRoundedTextBox(panelForm, "Dirección", out txtDireccion, col1, y, widthCol);
            UIHelper.CreateRoundedDateTimePicker(panelForm, "F. Retiro (Si aplica)", out dtpFechaRetiro, col2, y, widthCol, showCheckBox: true);
            dtpFechaRetiro.Checked = false;
            y += rowH;

            // Fila 4: Teléfono | Email
            UIHelper.CreateRoundedTextBox(panelForm, "Teléfono *", out txtTelefono, col1, y, widthCol);
            UIHelper.CreateRoundedTextBox(panelForm, "Email *", out txtEmail, col2, y, widthCol);
            y += rowH;

            // Fila 5: DATOS ADICIONALES (Multiline)
            UIHelper.CreateRoundedTextBox(panelForm, "DATOS ADICIONALES", out txtDatosAdicionales, col1, y, 744, 90, multiline: true);
            y += 125;

            // Botones: ACTUALIZAR y SALIR
            btnActualizar = UIHelper.CreatePrimaryButton("ACTUALIZAR", new Size(180, 44), new Point(col1, y));
            btnActualizar.Click += BtnActualizar_Click;
            panelForm.Controls.Add(btnActualizar);

            btnSalir = UIHelper.CreateSecondaryButton("SALIR", new Size(140, 44), new Point(col1 + 195, y));
            btnSalir.Click += (s, e) => Close();
            panelForm.Controls.Add(btnSalir);

            Controls.Add(panelForm);
        }

        private void LlenarDatos()
        {
            var e = _empleado!;
            txtNombreEmpleado.Text = e.Nombre;
            txtDocumento.Text = e.Documento;
            txtDireccion.Text = e.Direccion;
            txtTelefono.Text = e.Telefono;
            txtEmail.Text = e.Email;
            dtpFechaIngreso.Value = e.FechaIngreso;
            if (e.FechaRetiro.HasValue)
            {
                dtpFechaRetiro.Checked = true;
                dtpFechaRetiro.Value = e.FechaRetiro.Value;
            }
            txtDatosAdicionales.Text = e.DatosAdicionales;

            if (cboRolEmpleado.Items.Contains(e.Rol))
                cboRolEmpleado.SelectedItem = e.Rol;
        }

        private void BtnActualizar_Click(object? sender, EventArgs e)
        {
            errValidador.Clear();
            bool hayErrores = false;

            if (string.IsNullOrWhiteSpace(txtNombreEmpleado.Text))
            {
                errValidador.SetError(txtNombreEmpleado, "El Nombre del Empleado es obligatorio.");
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
            else if (!txtEmail.Text.Contains('@'))
            {
                errValidador.SetError(txtEmail, "El formato de correo es incorrecto.");
                hayErrores = true;
            }

            if (hayErrores)
            {
                ModernMessageBox.ShowWarning("Por favor complete los campos obligatorios señalados con error.", "Validación");
                return;
            }

            var emp = _esEdicion ? _empleado! : new Empleado();
            emp.Nombre = txtNombreEmpleado.Text.Trim();
            emp.Rol = cboRolEmpleado.SelectedItem?.ToString() ?? "Cajero";
            emp.Documento = txtDocumento.Text.Trim();
            emp.Direccion = txtDireccion.Text.Trim();
            emp.Telefono = txtTelefono.Text.Trim();
            emp.Email = txtEmail.Text.Trim();
            emp.FechaIngreso = dtpFechaIngreso.Value;
            emp.FechaRetiro = dtpFechaRetiro.Checked ? dtpFechaRetiro.Value : null;
            emp.DatosAdicionales = txtDatosAdicionales.Text.Trim();

            try
            {
                _service.Guardar(emp);
                ModernMessageBox.ShowSuccess("¡Empleado guardado exitosamente!", "Operación Exitosa");
                EmpleadoGuardado?.Invoke();
                Close();
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError(ex.Message, "Error al guardar");
            }
        }
    }

    // =========================================================================
    // frmlista_RolEmpleados — Listado de Roles
    // =========================================================================
    public class frmlista_RolEmpleados : Form
    {
        private DataGridView dgvRoles = null!;
        private Button btnNuevo = null!;
        private Button btnEditar = null!;

        private readonly List<RolItem> _roles = new()
        {
            new RolItem { Id = 1, NombreRol = "Administrador del Sistema", Descripcion = "Acceso total a todos los módulos de facturación, inventario, usuarios y configuración global." },
            new RolItem { Id = 2, NombreRol = "Cajero / Facturación", Descripcion = "Permiso para emitir facturas, consultar clientes, registrar cobros y generar reportes de caja." },
            new RolItem { Id = 3, NombreRol = "Almacenista", Descripcion = "Registro de entradas, salidas, traslados de inventario y recepción de órdenes de compra." },
            new RolItem { Id = 4, NombreRol = "Vendedor / Mostrador", Descripcion = "Consulta de productos, verificación de existencias, cotizaciones y atención al cliente." },
            new RolItem { Id = 5, NombreRol = "Supervisor de Inventario", Descripcion = "Aprobación de ajustes de stock, auditorías, revisión de alertas y monitoreo de inventario crítico." }
        };

        private RolItem? _rolSeleccionado;

        public frmlista_RolEmpleados()
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
                Text = "🛡️",
                Font = new Font("Segoe UI Emoji", 16f),
                Location = new Point(20, 14),
                Size = new Size(36, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblIcono);

            var lblSubtitulo = new Label
            {
                Text = "Definición de perfiles de usuario y niveles de privilegio en el sistema",
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

            btnNuevo = UIHelper.CreatePrimaryButton("＋ NUEVO ROL", new Size(130, 38), new Point(0, 0));
            btnNuevo.Click += (s, e) => AbrirFormularioRol(null);
            actionsPanel.Controls.Add(btnNuevo);

            btnEditar = UIHelper.CreateEditButton("✏️ EDITAR", new Size(95, 38), new Point(0, 0));
            btnEditar.Margin = new Padding(6, 0, 0, 0);
            btnEditar.Click += (s, e) =>
            {
                if (_rolSeleccionado == null)
                {
                    ModernMessageBox.ShowWarning("Seleccione un rol de la lista para editar.", "Selección");
                    return;
                }
                AbrirFormularioRol(_rolSeleccionado);
            };
            actionsPanel.Controls.Add(btnEditar);

            toolbarCard.Controls.Add(actionsPanel);
            Controls.Add(toolbarCard);

            var cardGrid = new CardPanel
            {
                Location = new Point(20, 92),
                Size = new Size(1140, 520),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            dgvRoles = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvRoles);

            dgvRoles.Columns.Add("ID", "ID");
            dgvRoles.Columns.Add("Nombre", "NOMBRE ROL");
            dgvRoles.Columns.Add("Descripcion", "DESCRIPCIÓN DETALLADA DEL ROL");
            dgvRoles.Columns.Add("Estado", "ESTADO");

            dgvRoles.Columns["ID"].Width = 60;
            dgvRoles.Columns["Nombre"].Width = 260;
            dgvRoles.Columns["Descripcion"].Width = 600;

            dgvRoles.SelectionChanged += (s, e) =>
            {
                if (dgvRoles.SelectedRows.Count > 0)
                    _rolSeleccionado = dgvRoles.SelectedRows[0].Tag as RolItem;
            };

            cardGrid.Controls.Add(dgvRoles);
            UIHelper.BindEmptyState(dgvRoles, "No hay roles registrados todavía.");
            Controls.Add(cardGrid);

            UIHelper.BindFillWidth(this, toolbarCard, 20);
            UIHelper.BindFillWidth(this, cardGrid, 20);
            UIHelper.BindFillHeight(this, cardGrid, 24);

            ResumeLayout();
        }

        private void CargarDatos()
        {
            dgvRoles.Rows.Clear();
            foreach (var r in _roles)
            {
                int idx = dgvRoles.Rows.Add(r.Id, r.NombreRol, r.Descripcion, "Activo");
                dgvRoles.Rows[idx].Tag = r;
            }
        }

        private void AbrirFormularioRol(RolItem? rol)
        {
            var form = new frmRoleEmpleados(rol);
            form.RolGuardado += (nuevoRol) =>
            {
                if (rol == null)
                {
                    nuevoRol.Id = _roles.Count + 1;
                    _roles.Add(nuevoRol);
                }
                else
                {
                    rol.NombreRol = nuevoRol.NombreRol;
                    rol.Descripcion = nuevoRol.Descripcion;
                }
                CargarDatos();
            };
            form.ShowDialog(this);
        }
    }

    // =========================================================================
    // frmRoleEmpleados — Formulario de Rol (Con ErrorProvider)
    // =========================================================================
    public class frmRoleEmpleados : Form
    {
        public event Action<RolItem>? RolGuardado;
        private readonly RolItem? _rol;
        private readonly bool _esEdicion;

        private TextBox txtNombreRol = null!;
        private TextBox txtDescripcionDetalladaRol = null!;
        private Button btnActualizar = null!;
        private Button btnSalir = null!;
        private ErrorProvider errValidador = null!;

        public frmRoleEmpleados(RolItem? rol = null)
        {
            _rol = rol;
            _esEdicion = rol != null;

            Size = new Size(560, 430);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;

            BuildUI();
            if (_esEdicion) LlenarDatos();
        }

        private void BuildUI()
        {
            errValidador = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // Encabezado estándar unificado con paleta morada de la aplicación
            var header = UIHelper.CreateModalHeader(this,
                _esEdicion ? "ROL DE EMPLEADOS (EDITAR)" : "ROL DE EMPLEADOS",
                "🛡️");
            Controls.Add(header);

            var panelForm = new Panel
            {
                Location = new Point(0, 60),
                Size = new Size(560, 370),
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            int x = 36, y = 20, width = 480;

            // Nombre Rol
            UIHelper.CreateRoundedTextBox(panelForm, "Nombre Rol *", out txtNombreRol, x, y, width, 36);
            y += 72;

            // Descripción Detallada Rol
            UIHelper.CreateRoundedTextBox(panelForm, "Descripción detallada Rol *", out txtDescripcionDetalladaRol, x, y, width, 100, multiline: true);
            y += 135;

            // Botones: ACTUALIZAR y SALIR
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
            txtNombreRol.Text = _rol!.NombreRol;
            txtDescripcionDetalladaRol.Text = _rol.Descripcion;
        }

        private void BtnActualizar_Click(object? sender, EventArgs e)
        {
            errValidador.Clear();
            bool hayErrores = false;

            if (string.IsNullOrWhiteSpace(txtNombreRol.Text))
            {
                errValidador.SetError(txtNombreRol, "El Nombre del Rol es obligatorio.");
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(txtDescripcionDetalladaRol.Text))
            {
                errValidador.SetError(txtDescripcionDetalladaRol, "La descripción del rol es obligatoria.");
                hayErrores = true;
            }

            if (hayErrores)
            {
                ModernMessageBox.ShowWarning("Por favor complete los campos obligatorios.", "Validación");
                return;
            }

            var rol = new RolItem
            {
                NombreRol = txtNombreRol.Text.Trim(),
                Descripcion = txtDescripcionDetalladaRol.Text.Trim()
            };

            RolGuardado?.Invoke(rol);
            ModernMessageBox.ShowSuccess("¡Rol guardado exitosamente!", "Operación Exitosa");
            Close();
        }
    }

    public class RolItem
    {
        public int Id { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    // =========================================================================
    // frmAdminSeguridad — Administración de Usuarios del Sistema (Con ErrorProvider)
    // =========================================================================
    public class frmAdminSeguridad : Form
    {
        private ComboBox cboEmpleado = null!;
        private TextBox txtUsuario = null!;
        private TextBox txtClave = null!;
        private Button btnActualizar = null!;
        private Button btnSalir = null!;
        private DataGridView dgvUsuarios = null!;
        private ErrorProvider errValidador = null!;
        private readonly EmpleadoService _empleadoService = new();
        private readonly UsuarioService _usuarioService = new();
        private List<Empleado> _empleados = new();
        private Usuario? _usuarioActual;

        public frmAdminSeguridad()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
            CargarEmpleados();
            CargarUsuarios();
        }

        private void BuildUI()
        {
            SuspendLayout();
            errValidador = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // Panel Superior: Formulario de Alta/Asignación de Usuario
            var cardForm = new CardPanel
            {
                Location = new Point(20, 16),
                Size = new Size(1140, 200),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            var lblHeader = new Label
            {
                Text = "🔐  ADMINISTRACIÓN DE USUARIOS DEL SISTEMA",
                Font = AppFonts.Heading,
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            cardForm.Controls.Add(lblHeader);

            int x = 24, y = 55, width = 340;

            // Empleado ComboBox
            UIHelper.CreateRoundedComboBox(cardForm, "Empleado *", out cboEmpleado, x, y, width, 36);
            cboEmpleado.SelectedIndexChanged += (s, e) =>
            {
                if (cboEmpleado.SelectedIndex < 0 || cboEmpleado.SelectedIndex >= _empleados.Count) return;
                var empleado = _empleados[cboEmpleado.SelectedIndex];
                txtUsuario.Text = empleado.Email;
                txtClave.Clear();
                // Se recuerda el usuario ya existente (si lo hay) para este
                // empleado, para actualizar ese mismo registro aunque se
                // cambie el correo — si se buscara solo por el correo nuevo
                // escrito en la caja, no lo encontraría y crearía uno duplicado.
                _usuarioActual = _usuarioService.ObtenerPorEmail(empleado.Email);
            };

            // Usuario
            UIHelper.CreateRoundedTextBox(cardForm, "Usuario / Correo *", out txtUsuario, x + width + 30, y, width, 36);

            // Clave
            UIHelper.CreateRoundedTextBox(cardForm, "Clave *", out txtClave, x + (width * 2) + 60, y, width, 36);
            txtClave.UseSystemPasswordChar = true;

            // Botones: ACTUALIZAR y SALIR
            btnActualizar = UIHelper.CreatePrimaryButton("ACTUALIZAR", new Size(180, 42), new Point(x, y + 75));
            btnActualizar.Click += BtnActualizar_Click;
            cardForm.Controls.Add(btnActualizar);

            btnSalir = UIHelper.CreateSecondaryButton("SALIR", new Size(140, 42), new Point(x + 195, y + 75));
            btnSalir.Click += (s, e) => Close();
            cardForm.Controls.Add(btnSalir);

            Controls.Add(cardForm);

            // Panel Inferior: DataGridView de Usuarios
            var cardGrid = new CardPanel
            {
                Location = new Point(20, 230),
                Size = new Size(1140, 390),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            var lblGridTitle = new Label { Text = "Usuarios con Acceso Habilitado al Software", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(16, 12), AutoSize = true };
            cardGrid.Controls.Add(lblGridTitle);

            dgvUsuarios = new DataGridView
            {
                Location = new Point(16, 42),
                Size = new Size(1108, 335),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            UIHelper.StyleDataGridView(dgvUsuarios);

            dgvUsuarios.Columns.Add("Nombre", "NOMBRE DE USUARIO");
            dgvUsuarios.Columns.Add("Usuario", "CORREO / LOGIN");
            dgvUsuarios.Columns.Add("Rol", "ROL Y PERMISOS");
            dgvUsuarios.Columns.Add("Estado", "ESTADO");

            cardGrid.Controls.Add(dgvUsuarios);
            UIHelper.BindEmptyState(dgvUsuarios, "No hay usuarios con acceso habilitado todavía.");
            Controls.Add(cardGrid);

            UIHelper.BindFillWidth(this, cardForm, 20);
            UIHelper.BindFillWidth(this, cardGrid, 20);
            UIHelper.BindFillHeight(this, cardGrid, 24);

            ResumeLayout();
        }

        private void CargarEmpleados()
        {
            try
            {
                _empleados = _empleadoService.ObtenerTodos();
                cboEmpleado.Items.Clear();
                foreach (var emp in _empleados)
                    cboEmpleado.Items.Add($"{emp.Nombre} ({emp.Rol})");
                if (cboEmpleado.Items.Count > 0) cboEmpleado.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError($"Error al cargar empleados: {ex.Message}");
            }
        }

        private void CargarUsuarios()
        {
            try
            {
                dgvUsuarios.Rows.Clear();
                foreach (var u in _usuarioService.ObtenerTodos())
                    dgvUsuarios.Rows.Add(u.Nombre, u.Email, u.Rol, u.Activo ? "Activo" : "Inactivo");
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError($"Error al cargar usuarios: {ex.Message}");
            }
        }

        private void BtnActualizar_Click(object? sender, EventArgs e)
        {
            errValidador.Clear();
            bool hayErrores = false;

            if (cboEmpleado.SelectedIndex < 0)
            {
                errValidador.SetError(cboEmpleado, "Seleccione un empleado.");
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                errValidador.SetError(txtUsuario, "El Usuario es obligatorio.");
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(txtClave.Text))
            {
                errValidador.SetError(txtClave, "La Clave es obligatoria.");
                hayErrores = true;
            }

            if (hayErrores)
            {
                ModernMessageBox.ShowWarning("Por favor complete los campos obligatorios marcados con error.", "Validación");
                return;
            }

            try
            {
                var empleado = _empleados[cboEmpleado.SelectedIndex];
                string email = txtUsuario.Text.Trim();

                // Se actualiza el usuario ya vinculado a este empleado (si existe),
                // aunque el correo haya cambiado; solo se crea uno nuevo cuando
                // el empleado todavía no tenía acceso al sistema.
                var usuario = _usuarioActual ?? new Usuario
                {
                    Rol = empleado.Rol.Contains("Administrador") ? "Administrador" : "Operador"
                };
                usuario.Nombre = empleado.Nombre;
                usuario.Email = email;
                usuario.Activo = true;
                usuario.PasswordHash = AuthService.HashPassword(txtClave.Text);

                _usuarioService.Guardar(usuario);
                // Vuelve a resolver el usuario (ahora con su Id real si era nuevo)
                // para que un segundo clic sin cambiar de empleado actualice en
                // vez de intentar crear otro registro duplicado.
                _usuarioActual = _usuarioService.ObtenerPorEmail(email);
                CargarUsuarios();
                txtClave.Clear();
                ModernMessageBox.ShowSuccess("¡Credenciales de seguridad actualizadas con éxito!");
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError(ex.Message, "Error al guardar");
            }
        }
    }

    /// <summary>
    /// Alias para cumplir con el nombre exacto de la guía universitaria (frmSeguridad)
    /// </summary>
    public class frmSeguridad : frmAdminSeguridad
    {
    }
}
