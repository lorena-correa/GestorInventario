using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;

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
        private Button btnEditar = null!;
        private Button btnBorrar = null!;

        private readonly List<EmpleadoItem> _empleados = new()
        {
            new EmpleadoItem { Id = 1, Nombre = "Lorena Correa", Documento = "1020456789", Rol = "Cajero / Facturación", Telefono = "3004567890", Email = "lorena.correa@empresa.com", Direccion = "Calle 45 #30-10, Medellín", FechaIngreso = new DateTime(2023, 2, 1), DatosAdicionales = "Encargada del módulo de caja y facturación principal." },
            new EmpleadoItem { Id = 2, Nombre = "Ana María Torres", Documento = "1035987654", Rol = "Almacenista", Telefono = "3116549870", Email = "ana.torres@empresa.com", Direccion = "Carrera 80 #25-40, Robledo", FechaIngreso = new DateTime(2022, 6, 15), DatosAdicionales = "Supervisión de recepciones y control de stock físico." },
            new EmpleadoItem { Id = 3, Nombre = "Javier Saldarriaga", Documento = "71234567", Rol = "Administrador del Sistema", Telefono = "3159988776", Email = "javier.saldarriaga@empresa.com", Direccion = "Av. Poblado #10-50, Medellín", FechaIngreso = new DateTime(2021, 1, 10), DatosAdicionales = "Administración general, gestión de usuarios y parámetros." },
            new EmpleadoItem { Id = 4, Nombre = "Carlos Andrés Vendedor", Documento = "1017554433", Rol = "Vendedor / Mostrador", Telefono = "3201122334", Email = "carlos.vendedor@empresa.com", Direccion = "Circular 1ra #70-30, Laureles", FechaIngreso = new DateTime(2024, 1, 15), DatosAdicionales = "Atención al público en mostrador y cotizaciones." }
        };

        private EmpleadoItem? _empleadoSeleccionado;

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
                Size = new Size(1140, 72),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            var lblTitulo = new Label
            {
                Text = "👨‍💼  ADMINISTRACIÓN DE EMPLEADOS",
                Font = AppFonts.Heading,
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblTitulo);

            var lblSubtitulo = new Label
            {
                Text = "Gestión del personal de la empresa, roles laborales y control de accesos",
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

            UIHelper.CreateSearchInput(actionsPanel, out txtBuscar, 0, 0, 220, 38, "Buscar empleado...");
            txtBuscar.TextChanged += (s, e) => CargarDatos(txtBuscar.Text);

            btnNuevo = UIHelper.CreatePrimaryButton("＋ NUEVO", new Size(105, 38), new Point(0, 0));
            btnNuevo.Margin = new Padding(6, 0, 0, 0);
            btnNuevo.Click += (s, e) => AbrirFormularioEmpleado(null);
            actionsPanel.Controls.Add(btnNuevo);

            btnEditar = UIHelper.CreateSecondaryButton("✏️ EDITAR", new Size(95, 38), new Point(0, 0));
            btnEditar.Margin = new Padding(6, 0, 0, 0);
            btnEditar.Click += (s, e) =>
            {
                if (_empleadoSeleccionado == null)
                {
                    MessageBox.Show("Por favor seleccione un empleado de la lista.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                AbrirFormularioEmpleado(_empleadoSeleccionado);
            };
            actionsPanel.Controls.Add(btnEditar);

            btnBorrar = UIHelper.CreateDangerButton("🗑 BORRAR", new Size(95, 38), new Point(0, 0));
            btnBorrar.Margin = new Padding(6, 0, 0, 0);
            btnBorrar.Click += (s, e) =>
            {
                if (_empleadoSeleccionado == null)
                {
                    MessageBox.Show("Por favor seleccione un empleado de la lista para eliminar.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (MessageBox.Show($"¿Eliminar al empleado {_empleadoSeleccionado.Nombre}?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _empleados.Remove(_empleadoSeleccionado);
                    _empleadoSeleccionado = null;
                    CargarDatos(txtBuscar.Text);
                    MessageBox.Show("Empleado eliminado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            dgvEmpleados.Columns["ID"].Width = 60;
            dgvEmpleados.Columns["Nombre"].Width = 220;
            dgvEmpleados.Columns["Documento"].Width = 130;
            dgvEmpleados.Columns["Rol"].Width = 190;
            dgvEmpleados.Columns["FechaIngreso"].Width = 120;

            dgvEmpleados.SelectionChanged += (s, e) =>
            {
                if (dgvEmpleados.SelectedRows.Count > 0)
                    _empleadoSeleccionado = dgvEmpleados.SelectedRows[0].Tag as EmpleadoItem;
            };

            dgvEmpleados.CellDoubleClick += (s, e) =>
            {
                if (_empleadoSeleccionado != null)
                    AbrirFormularioEmpleado(_empleadoSeleccionado);
            };

            cardGrid.Controls.Add(dgvEmpleados);
            Controls.Add(cardGrid);

            ResumeLayout();
        }

        private void CargarDatos(string filtro = "")
        {
            dgvEmpleados.Rows.Clear();
            foreach (var emp in _empleados)
            {
                if (!string.IsNullOrEmpty(filtro) &&
                    !emp.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase) &&
                    !emp.Documento.Contains(filtro, StringComparison.OrdinalIgnoreCase) &&
                    !emp.Rol.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    continue;

                int r = dgvEmpleados.Rows.Add(
                    emp.Id,
                    emp.Nombre,
                    emp.Documento,
                    emp.Rol,
                    emp.Telefono,
                    emp.Email,
                    emp.FechaIngreso.ToString("dd/MM/yyyy"),
                    "Activo"
                );
                dgvEmpleados.Rows[r].Tag = emp;
            }
        }

        private void AbrirFormularioEmpleado(EmpleadoItem? emp)
        {
            var form = new frmEmpleados(emp);
            form.EmpleadoGuardado += (nuevoEmp) =>
            {
                if (emp == null)
                {
                    nuevoEmp.Id = _empleados.Count + 1;
                    _empleados.Add(nuevoEmp);
                }
                else
                {
                    emp.Nombre = nuevoEmp.Nombre;
                    emp.Documento = nuevoEmp.Documento;
                    emp.Rol = nuevoEmp.Rol;
                    emp.Telefono = nuevoEmp.Telefono;
                    emp.Email = nuevoEmp.Email;
                    emp.Direccion = nuevoEmp.Direccion;
                    emp.FechaIngreso = nuevoEmp.FechaIngreso;
                    emp.FechaRetiro = nuevoEmp.FechaRetiro;
                    emp.DatosAdicionales = nuevoEmp.DatosAdicionales;
                }
                CargarDatos(txtBuscar.Text);
            };
            form.ShowDialog(this);
        }
    }

    // =========================================================================
    // frmEmpleados — Formulario de Empleado (Con DateTimePicker y ErrorProvider)
    // =========================================================================
    public class frmEmpleados : Form
    {
        public event Action<EmpleadoItem>? EmpleadoGuardado;
        private readonly EmpleadoItem? _empleado;
        private readonly bool _esEdicion;

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

        public frmEmpleados(EmpleadoItem? empleado = null)
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
            CrearCampoTexto(panelForm, "Nombre Empleado *", out txtNombreEmpleado, col1, y, widthCol);
            CrearComboBox(panelForm, "Rol Empleado *", out cboRolEmpleado, col2, y, widthCol);
            cboRolEmpleado.Items.AddRange(new[] { "Administrador del Sistema", "Cajero / Facturación", "Almacenista", "Vendedor / Mostrador", "Supervisor de Inventario" });
            cboRolEmpleado.SelectedIndex = 1;
            y += rowH;

            // Fila 2: Documento | F. Ingreso (DateTimePicker)
            CrearCampoTexto(panelForm, "Documento *", out txtDocumento, col1, y, widthCol);

            var lblFI = new Label { Text = "F. Ingreso *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(col2, y), AutoSize = true };
            dtpFechaIngreso = new DateTimePicker { Location = new Point(col2, y + 20), Size = new Size(widthCol, 32), Font = AppFonts.Body, Format = DateTimePickerFormat.Short };
            panelForm.Controls.Add(lblFI);
            panelForm.Controls.Add(dtpFechaIngreso);
            y += rowH;

            // Fila 3: Dirección | F. Retiro
            CrearCampoTexto(panelForm, "Dirección", out txtDireccion, col1, y, widthCol);

            var lblFR = new Label { Text = "F. Retiro (Si aplica)", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(col2, y), AutoSize = true };
            dtpFechaRetiro = new DateTimePicker { Location = new Point(col2, y + 20), Size = new Size(widthCol, 32), Font = AppFonts.Body, Format = DateTimePickerFormat.Short, Checked = false, ShowCheckBox = true };
            panelForm.Controls.Add(lblFR);
            panelForm.Controls.Add(dtpFechaRetiro);
            y += rowH;

            // Fila 4: Teléfono | Email
            CrearCampoTexto(panelForm, "Teléfono *", out txtTelefono, col1, y, widthCol);
            CrearCampoTexto(panelForm, "Email *", out txtEmail, col2, y, widthCol);
            y += rowH;

            // Fila 5: DATOS ADICIONALES (Multiline)
            var lblObs = new Label { Text = "DATOS ADICIONALES", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(col1, y), AutoSize = true };
            txtDatosAdicionales = new TextBox
            {
                Location = new Point(col1, y + 20),
                Size = new Size(744, 70),
                Font = AppFonts.Body,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ForeColor = AppColors.TextPrimary
            };
            panelForm.Controls.Add(lblObs);
            panelForm.Controls.Add(txtDatosAdicionales);
            y += 105;

            // Botones: ACTUALIZAR y SALIR
            btnActualizar = UIHelper.CreatePrimaryButton("ACTUALIZAR", new Size(180, 44), new Point(col1, y));
            btnActualizar.Click += BtnActualizar_Click;
            panelForm.Controls.Add(btnActualizar);

            btnSalir = UIHelper.CreateSecondaryButton("SALIR", new Size(140, 44), new Point(col1 + 195, y));
            btnSalir.Click += (s, e) => Close();
            panelForm.Controls.Add(btnSalir);

            Controls.Add(panelForm);
        }

        private void CrearCampoTexto(Panel panel, string etiqueta, out TextBox txt, int x, int y, int width)
        {
            var lbl = new Label { Text = etiqueta, Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true };
            txt = new TextBox
            {
                Location = new Point(x, y + 20),
                Size = new Size(width, 32),
                Font = AppFonts.Body,
                BorderStyle = BorderStyle.FixedSingle,
                ForeColor = AppColors.TextPrimary
            };
            panel.Controls.Add(lbl);
            panel.Controls.Add(txt);
        }

        private void CrearComboBox(Panel panel, string etiqueta, out ComboBox cbo, int x, int y, int width)
        {
            var lbl = new Label { Text = etiqueta, Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true };
            cbo = new ComboBox
            {
                Location = new Point(x, y + 20),
                Size = new Size(width, 32),
                Font = AppFonts.Body,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            panel.Controls.Add(lbl);
            panel.Controls.Add(cbo);
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
                MessageBox.Show("Por favor complete los campos obligatorios señalados con error.",
                    "Validación con ErrorProvider", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var emp = new EmpleadoItem
            {
                Nombre = txtNombreEmpleado.Text.Trim(),
                Rol = cboRolEmpleado.SelectedItem?.ToString() ?? "Cajero",
                Documento = txtDocumento.Text.Trim(),
                Direccion = txtDireccion.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                FechaIngreso = dtpFechaIngreso.Value,
                FechaRetiro = dtpFechaRetiro.Checked ? dtpFechaRetiro.Value : null,
                DatosAdicionales = txtDatosAdicionales.Text.Trim()
            };

            EmpleadoGuardado?.Invoke(emp);
            MessageBox.Show("¡Empleado guardado exitosamente!", "Operación Exitosa",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }

    public class EmpleadoItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaRetiro { get; set; }
        public string DatosAdicionales { get; set; } = string.Empty;
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
                Size = new Size(1140, 72),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            var lblTitulo = new Label
            {
                Text = "🛡️  ROLES DE EMPLEADOS",
                Font = AppFonts.Heading,
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblTitulo);

            var lblSubtitulo = new Label
            {
                Text = "Definición de perfiles de usuario y asignación de niveles de privilegio en el sistema",
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
                Location = new Point(830, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent,
                Height = 44
            };

            btnNuevo = UIHelper.CreatePrimaryButton("＋ NUEVO ROL", new Size(130, 38), new Point(0, 0));
            btnNuevo.Click += (s, e) => AbrirFormularioRol(null);
            actionsPanel.Controls.Add(btnNuevo);

            btnEditar = UIHelper.CreateSecondaryButton("✏️ EDITAR", new Size(95, 38), new Point(0, 0));
            btnEditar.Margin = new Padding(6, 0, 0, 0);
            btnEditar.Click += (s, e) =>
            {
                if (_rolSeleccionado == null)
                {
                    MessageBox.Show("Seleccione un rol de la lista para editar.", "Selección", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                AbrirFormularioRol(_rolSeleccionado);
            };
            actionsPanel.Controls.Add(btnEditar);

            toolbarCard.Controls.Add(actionsPanel);
            Controls.Add(toolbarCard);

            var cardGrid = new CardPanel
            {
                Location = new Point(20, 100),
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
            Controls.Add(cardGrid);

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
            var lblNombre = new Label { Text = "Nombre Rol *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true };
            txtNombreRol = new TextBox { Location = new Point(x, y + 22), Size = new Size(width, 34), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle, ForeColor = AppColors.TextPrimary };
            panelForm.Controls.Add(lblNombre);
            panelForm.Controls.Add(txtNombreRol);
            y += 72;

            // Descripción Detallada Rol
            var lblDesc = new Label { Text = "Descripción detallada Rol *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true };
            txtDescripcionDetalladaRol = new TextBox
            {
                Location = new Point(x, y + 22),
                Size = new Size(width, 85),
                Font = AppFonts.Body,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ForeColor = AppColors.TextPrimary
            };
            panelForm.Controls.Add(lblDesc);
            panelForm.Controls.Add(txtDescripcionDetalladaRol);
            y += 120;

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
                MessageBox.Show("Por favor complete los campos obligatorios.", "Validación con ErrorProvider",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rol = new RolItem
            {
                NombreRol = txtNombreRol.Text.Trim(),
                Descripcion = txtDescripcionDetalladaRol.Text.Trim()
            };

            RolGuardado?.Invoke(rol);
            MessageBox.Show("¡Rol guardado exitosamente!", "Operación Exitosa",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        public frmAdminSeguridad()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
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
            var lblEmp = new Label { Text = "Empleado *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true };
            cboEmpleado = new ComboBox { Location = new Point(x, y + 22), Size = new Size(width, 34), Font = AppFonts.Body, DropDownStyle = ComboBoxStyle.DropDownList };
            cboEmpleado.Items.AddRange(new[] { "Lorena Correa (Cajera)", "Ana María Torres (Almacenista)", "Javier Saldarriaga (Admin)", "Carlos Andrés Vendedor (Ventas)" });
            cboEmpleado.SelectedIndex = 0;
            cboEmpleado.SelectedIndexChanged += (s, e) =>
            {
                if (cboEmpleado.SelectedIndex == 0) txtUsuario.Text = "lorena.correa@empresa.com";
                else if (cboEmpleado.SelectedIndex == 1) txtUsuario.Text = "ana.torres@empresa.com";
                else if (cboEmpleado.SelectedIndex == 2) txtUsuario.Text = "admin@empresa.com";
                else if (cboEmpleado.SelectedIndex == 3) txtUsuario.Text = "carlos.vendedor@empresa.com";
            };
            cardForm.Controls.Add(lblEmp);
            cardForm.Controls.Add(cboEmpleado);

            // Usuario
            var lblUsr = new Label { Text = "Usuario / Correo *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x + width + 30, y), AutoSize = true };
            txtUsuario = new TextBox { Location = new Point(x + width + 30, y + 22), Size = new Size(width, 34), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle, Text = "lorena.correa@empresa.com" };
            cardForm.Controls.Add(lblUsr);
            cardForm.Controls.Add(txtUsuario);

            // Clave
            var lblPass = new Label { Text = "Clave *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x + (width * 2) + 60, y), AutoSize = true };
            txtClave = new TextBox { Location = new Point(x + (width * 2) + 60, y + 22), Size = new Size(width, 34), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true };
            cardForm.Controls.Add(lblPass);
            cardForm.Controls.Add(txtClave);

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

            dgvUsuarios.Columns.Add("Empleado", "EMPLEADO VINCULADO");
            dgvUsuarios.Columns.Add("Usuario", "NOMBRE DE USUARIO / LOGIN");
            dgvUsuarios.Columns.Add("Rol", "ROL Y PERMISOS");
            dgvUsuarios.Columns.Add("UltimoIngreso", "ÚLTIMO ACCESO");
            dgvUsuarios.Columns.Add("Estado", "ESTADO");

            cardGrid.Controls.Add(dgvUsuarios);
            Controls.Add(cardGrid);

            ResumeLayout();
        }

        private void CargarUsuarios()
        {
            dgvUsuarios.Rows.Clear();
            dgvUsuarios.Rows.Add("Javier Saldarriaga", "admin@empresa.com", "Administrador del Sistema", DateTime.Now.ToString("dd/MM/yyyy HH:mm"), "Activo");
            dgvUsuarios.Rows.Add("Lorena Correa", "lorena.correa@empresa.com", "Cajero / Facturación", DateTime.Now.AddHours(-2).ToString("dd/MM/yyyy HH:mm"), "Activo");
            dgvUsuarios.Rows.Add("Ana María Torres", "ana.torres@empresa.com", "Almacenista", DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy HH:mm"), "Activo");
            dgvUsuarios.Rows.Add("Carlos Andrés Vendedor", "carlos.vendedor@empresa.com", "Vendedor / Mostrador", DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy HH:mm"), "Activo");
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
                MessageBox.Show("Por favor complete los campos obligatorios marcados con error.",
                    "Validación con ErrorProvider", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvUsuarios.Rows.Add(
                cboEmpleado.SelectedItem?.ToString()?.Split('(')[0].Trim(),
                txtUsuario.Text.Trim(),
                "Asignado",
                DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                "Activo"
            );

            MessageBox.Show("¡Credenciales de seguridad actualizadas con éxito!", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            txtClave.Clear();
        }
    }
}
