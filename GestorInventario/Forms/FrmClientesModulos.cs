using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;
using GestorInventario.Models;

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

        // Lista en memoria para demostración visual inmediata
        private readonly List<ClienteItem> _listaClientes = new()
        {
            new ClienteItem { Id = 1, Nombre = "Carlos Andrés Pérez", Documento = "1020304050", Telefono = "3001234567", Email = "carlos.perez@correo.com", Direccion = "Calle 50 #40-20, Medellín" },
            new ClienteItem { Id = 2, Nombre = "María Fernanda Gómez", Documento = "1030405060", Telefono = "3109876543", Email = "maria.gomez@correo.com", Direccion = "Carrera 70 #10-15, Medellín" },
            new ClienteItem { Id = 3, Nombre = "Distribuidora Los Andes S.A.S.", Documento = "900123456-1", Telefono = "6044445566", Email = "ventas@losandes.com", Direccion = "Zona Industrial #12-30, Itagüí" },
            new ClienteItem { Id = 4, Nombre = "Juan David Morales", Documento = "1017894561", Telefono = "3154567890", Email = "juan.morales@correo.com", Direccion = "Circular 4ta #73-10, Laureles" },
            new ClienteItem { Id = 5, Nombre = "Tecnología Global S.A.", Documento = "890123987-4", Telefono = "6045558899", Email = "contacto@tecglobal.com", Direccion = "Av. Las Vegas #32-10, Envigado" },
            new ClienteItem { Id = 6, Nombre = "Laura Patricia Restrepo", Documento = "1035678912", Telefono = "3206549871", Email = "laura.restrepo@correo.com", Direccion = "Calle 10 #43E-12, Poblado" }
        };

        private ClienteItem? _clienteSeleccionado;

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
                Size = new Size(1140, 72),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            var lblTitulo = new Label
            {
                Text = "👥  ADMINISTRACIÓN DE CLIENTES",
                Font = AppFonts.Heading,
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblTitulo);

            var lblSubtitulo = new Label
            {
                Text = "Consulte, agregue, modifique o elimine clientes registrados en el sistema",
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

            UIHelper.CreateSearchInput(actionsPanel, out txtBuscar, 0, 0, 220, 38, "Buscar cliente...");
            txtBuscar.TextChanged += (s, e) => CargarDatos(txtBuscar.Text);

            btnNuevo = UIHelper.CreatePrimaryButton("＋ NUEVO", new Size(105, 38), new Point(0, 0));
            btnNuevo.Margin = new Padding(6, 0, 0, 0);
            btnNuevo.Click += (s, e) => AbrirFormularioCliente(null);
            actionsPanel.Controls.Add(btnNuevo);

            btnEditar = UIHelper.CreateSecondaryButton("✏️ EDITAR", new Size(95, 38), new Point(0, 0));
            btnEditar.Margin = new Padding(6, 0, 0, 0);
            btnEditar.Click += (s, e) =>
            {
                if (_clienteSeleccionado == null)
                {
                    MessageBox.Show("Por favor seleccione un cliente de la lista para editar.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("Por favor seleccione un cliente de la lista para eliminar.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (MessageBox.Show($"¿Desea eliminar al cliente {_clienteSeleccionado.Nombre}?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _listaClientes.Remove(_clienteSeleccionado);
                    _clienteSeleccionado = null;
                    CargarDatos(txtBuscar.Text);
                    MessageBox.Show("Cliente eliminado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            actionsPanel.Controls.Add(btnBorrar);

            toolbarCard.Controls.Add(actionsPanel);
            Controls.Add(toolbarCard);

            // Contenedor DataGridView
            var cardGrid = new CardPanel
            {
                Location = new Point(20, 100),
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
                    _clienteSeleccionado = dgvClientes.SelectedRows[0].Tag as ClienteItem;
            };

            dgvClientes.CellDoubleClick += (s, e) =>
            {
                if (_clienteSeleccionado != null)
                    AbrirFormularioCliente(_clienteSeleccionado);
            };

            cardGrid.Controls.Add(dgvClientes);
            Controls.Add(cardGrid);

            ResumeLayout();
        }

        private void CargarDatos(string filtro = "")
        {
            dgvClientes.Rows.Clear();
            foreach (var c in _listaClientes)
            {
                if (!string.IsNullOrEmpty(filtro) &&
                    !c.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase) &&
                    !c.Documento.Contains(filtro, StringComparison.OrdinalIgnoreCase) &&
                    !c.Email.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    continue;

                int rowIndex = dgvClientes.Rows.Add(c.Id, c.Nombre, c.Documento, c.Telefono, c.Email, c.Direccion, "Activo");
                dgvClientes.Rows[rowIndex].Tag = c;
            }
        }

        private void AbrirFormularioCliente(ClienteItem? cliente)
        {
            var form = new frmClientes(cliente);
            form.ClienteGuardado += (nuevoCliente) =>
            {
                if (cliente == null)
                {
                    nuevoCliente.Id = _listaClientes.Count + 1;
                    _listaClientes.Add(nuevoCliente);
                }
                else
                {
                    cliente.Nombre = nuevoCliente.Nombre;
                    cliente.Documento = nuevoCliente.Documento;
                    cliente.Telefono = nuevoCliente.Telefono;
                    cliente.Email = nuevoCliente.Email;
                    cliente.Direccion = nuevoCliente.Direccion;
                }
                CargarDatos(txtBuscar.Text);
            };
            form.ShowDialog(this);
        }
    }

    // =========================================================================
    // frmClientes — Formulario de Edición / Nuevo Registro Cliente (Con ErrorProvider)
    // =========================================================================
    public class frmClientes : Form
    {
        public event Action<ClienteItem>? ClienteGuardado;
        private readonly ClienteItem? _cliente;
        private readonly bool _esEdicion;

        // Controles con nomenclatura estándar de la guía
        private TextBox txtNombreCliente = null!;
        private TextBox txtDocumento = null!;
        private TextBox txtDireccion = null!;
        private TextBox txtTelefono = null!;
        private TextBox txtEmail = null!;
        private Button btnActualizar = null!;
        private Button btnSalir = null!;
        private ErrorProvider errValidador = null!;

        public frmClientes(ClienteItem? cliente = null)
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
                MessageBox.Show("Por favor verifique los campos marcados con error antes de continuar.",
                    "Validación de Campos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var item = new ClienteItem
            {
                Nombre = txtNombreCliente.Text.Trim(),
                Documento = txtDocumento.Text.Trim(),
                Direccion = txtDireccion.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                Email = txtEmail.Text.Trim()
            };

            ClienteGuardado?.Invoke(item);
            MessageBox.Show("¡Registro de cliente procesado exitosamente!", "Operación Exitosa",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }

    public class ClienteItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
    }
}
