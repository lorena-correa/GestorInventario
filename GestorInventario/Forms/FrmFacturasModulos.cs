using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;
using GestorInventario.Models;
using GestorInventario.Services;

namespace GestorInventario.Forms
{
    // =========================================================================
    // frmlistaFacturas — Listado de Facturación (CRUD Maestro)
    // =========================================================================
    public class frmlistaFacturas : Form
    {
        private DataGridView dgvFacturas = null!;
        private TextBox txtBuscar = null!;
        private Button btnNuevaFactura = null!;
        private ComboBox cboFiltroEstado = null!;

        private readonly FacturaService _service = new();
        private Factura? _facturaSeleccionada;

        public frmlistaFacturas()
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
                Text = "🧾",
                Font = new Font("Segoe UI Emoji", 16f),
                Location = new Point(20, 14),
                Size = new Size(36, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblIcono);

            var lblSubtitulo = new Label
            {
                Text = "Registro de ventas, emisión de comprobantes fiscales y gestión de cobros",
                Font = AppFonts.Small,
                ForeColor = AppColors.TextSecondary,
                Location = new Point(64, 24),
                Size = new Size(320, 18),
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

            // Filtro Estado
            var cboFiltroEstadoContainer = UIHelper.CreateRoundedComboBox(actionsPanel, "", out cboFiltroEstado, 0, 0, 115, 38);
            cboFiltroEstadoContainer.Margin = new Padding(0, 0, 6, 0);
            cboFiltroEstado.Items.AddRange(new[] { "Todos", "Pagada", "Pendiente", "Emitida", "Anulada" });
            cboFiltroEstado.SelectedIndex = 0;
            cboFiltroEstado.SelectedIndexChanged += (s, e) => CargarDatos(txtBuscar.Text);

            // Buscador moderno
            UIHelper.CreateSearchInput(actionsPanel, out txtBuscar, 0, 0, 190, 38, "Buscar factura...");
            txtBuscar.TextChanged += (s, e) => CargarDatos(txtBuscar.Text);

            btnNuevaFactura = UIHelper.CreatePrimaryButton("＋ NUEVA", new Size(100, 38), new Point(0, 0));
            btnNuevaFactura.Margin = new Padding(6, 0, 0, 0);
            btnNuevaFactura.Click += (s, e) => AbrirFormularioFactura(null);
            actionsPanel.Controls.Add(btnNuevaFactura);

            var btnSalir = UIHelper.CreateSecondaryButton("SALIR", new Size(90, 38), new Point(0, 0));
            btnSalir.Margin = new Padding(6, 0, 0, 0);
            btnSalir.Click += (s, e) => FrmMain.CerrarModulo(this);
            actionsPanel.Controls.Add(btnSalir);

            toolbarCard.Controls.Add(actionsPanel);
            Controls.Add(toolbarCard);

            var cardGrid = new CardPanel
            {
                Location = new Point(20, 92),
                Size = new Size(1140, 515),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            dgvFacturas = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvFacturas);

            dgvFacturas.Columns.Add("NroFactura", "NRO. FACTURA");
            dgvFacturas.Columns.Add("Fecha", "FECHA REGISTRO");
            dgvFacturas.Columns.Add("Cliente", "CLIENTE");
            dgvFacturas.Columns.Add("Empleado", "EMPLEADO / CAJERO");
            dgvFacturas.Columns.Add("Descuento", "DESCUENTO");
            dgvFacturas.Columns.Add("TotalIva", "TOTAL IVA (19%)");
            dgvFacturas.Columns.Add("TotalFactura", "TOTAL FACTURA");
            dgvFacturas.Columns.Add("Estado", "ESTADO");

            // Botones Editar / Anular dentro del grid
            UIHelper.AddGridActionButtons<Factura>(dgvFacturas, AbrirFormularioFactura, AnularFactura, "Anular");

            dgvFacturas.Columns["NroFactura"].Width = 130;
            dgvFacturas.Columns["Fecha"].Width = 140;
            dgvFacturas.Columns["Cliente"].Width = 240;
            dgvFacturas.Columns["Empleado"].Width = 190;
            dgvFacturas.Columns["TotalFactura"].Width = 150;

            dgvFacturas.SelectionChanged += (s, e) =>
            {
                if (dgvFacturas.SelectedRows.Count > 0)
                    _facturaSeleccionada = dgvFacturas.SelectedRows[0].Tag as Factura;
            };

            dgvFacturas.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && _facturaSeleccionada != null)
                    AbrirFormularioFactura(_facturaSeleccionada);
            };

            cardGrid.Controls.Add(dgvFacturas);
            UIHelper.BindEmptyState(dgvFacturas, "No hay facturas registradas todavía.");
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
                dgvFacturas.Rows.Clear();
                _facturaSeleccionada = null;
                string estadoFiltro = cboFiltroEstado.SelectedItem?.ToString() ?? "Todos";

                foreach (var f in _service.Buscar(filtro.Trim(), estadoFiltro))
                {
                    int r = dgvFacturas.Rows.Add(
                        f.NroFactura,
                        f.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                        f.Cliente,
                        f.Empleado,
                        $"${f.Descuento:N0}",
                        $"${f.TotalIva:N0}",
                        $"${f.TotalFactura:N0}",
                        f.Estado
                    );

                    dgvFacturas.Rows[r].Tag = f;
                    if (f.Estado == "Pagada")
                        dgvFacturas.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Success;
                    else if (f.Estado == "Pendiente")
                        dgvFacturas.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Warning;
                    else if (f.Estado == "Anulada")
                        dgvFacturas.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Danger;
                    else
                        dgvFacturas.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Primary;
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError($"Error al cargar facturas: {ex.Message}");
            }
        }

        private void AbrirFormularioFactura(Factura? factura)
        {
            try
            {
                // Para editar se trae la factura completa con su detalle
                Factura? completa = factura == null ? null : _service.ObtenerPorId(factura.Id);
                var form = new frmFacturas(completa);
                form.FacturaGuardada += () => CargarDatos(txtBuscar.Text);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError(ex.Message, "Facturación");
            }
        }

        private void AnularFactura(Factura factura)
        {
            if (factura.Estado == "Anulada")
            {
                ModernMessageBox.ShowInfo($"La factura {factura.NroFactura} ya está anulada.", "Facturación");
                return;
            }
            if (ModernMessageBox.ShowConfirm($"¿Desea anular la factura {factura.NroFactura}?\nEl stock vendido regresará al inventario.", "Confirmar Anulación", "Anular") == DialogResult.Yes)
            {
                try
                {
                    _service.Anular(factura.Id);
                    CargarDatos(txtBuscar.Text);
                    ModernMessageBox.ShowSuccess("Factura anulada correctamente.", "Facturación");
                }
                catch (Exception ex)
                {
                    ModernMessageBox.ShowError(ex.Message, "Error al anular");
                }
            }
        }
    }

    // =========================================================================
    // frmFacturas — Formulario de Facturación (Módulo Maestro-Detalle con ErrorProvider)
    // =========================================================================
    public class frmFacturas : Form
    {
        public event Action? FacturaGuardada;
        private readonly Factura? _facturaExistente;
        private readonly bool _esEdicion;
        private readonly bool _esSoloLectura;

        private readonly FacturaService _service = new();
        private List<Cliente> _clientes = new();
        private List<Empleado> _empleados = new();
        private List<Producto> _productos = new();

        // Controles de cabecera según la guía
        private TextBox txtNroFactura = null!;
        private DateTimePicker dtpFechaRegistro = null!;
        private ComboBox cboCliente = null!;
        private ComboBox cboEstadoFactura = null!;
        private ComboBox cboEmpleado = null!;
        private TextBox txtDescuento = null!;
        private TextBox txtTotalIva = null!;
        private TextBox txtTotalFactura = null!;

        // Controles de detalle
        private ComboBox cboProducto = null!;
        private TextBox txtCantidad = null!;
        private TextBox txtPrecioUnitario = null!;
        private DataGridView dgvDetalle = null!;
        private Button btnAgregarItem = null!;
        private Button btnQuitarItem = null!;

        // Botones principales
        private Button btnActualizar = null!;
        private Button btnSalir = null!;
        private ErrorProvider errValidador = null!;

        private readonly List<DetalleFactura> _lineasFactura = new();

        public frmFacturas(Factura? factura = null)
        {
            _facturaExistente = factura;
            _esEdicion = factura != null;
            _esSoloLectura = factura?.Estado == "Anulada";

            Size = new Size(960, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;

            BuildUI();
            CargarCombos();
            if (_esEdicion) LlenarDatos();
            else GenerarNumeroConsecutivo();
            if (_esSoloLectura) BloquearEdicion();
        }

        private void BuildUI()
        {
            errValidador = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // Encabezado estándar unificado
            string titulo = !_esEdicion ? "ADMINISTRACIÓN DE FACTURAS (NUEVA EMISIÓN)"
                : _esSoloLectura ? $"DETALLE DE FACTURA ANULADA — {_facturaExistente!.NroFactura}"
                : $"EDITAR FACTURA — {_facturaExistente!.NroFactura}";
            var header = UIHelper.CreateModalHeader(this, titulo, "🧾");
            Controls.Add(header);

            // Contenedor General Scrollable
            var mainContent = new Panel
            {
                Location = new Point(0, 60),
                Size = new Size(960, 620),
                BackColor = Color.White,
                AutoScroll = true,
                Padding = new Padding(24)
            };

            // ── SECCIÓN 1: DATOS GENERALES DE LA FACTURA ─────────────────────
            var cardCabecera = new CardPanel
            {
                Location = new Point(20, 10),
                Size = new Size(900, 175),
                BackColor = Color.White
            };

            // Fila 1: Nro Factura | Fecha Registro | Estado | Empleado
            UIHelper.CreateRoundedTextBox(cardCabecera, "Nro Factura *", out txtNroFactura, 20, 12, 200, 36, readOnly: true);
            UIHelper.CreateRoundedDateTimePicker(cardCabecera, "Fecha Registro *", out dtpFechaRegistro, 240, 12, 200, 36);

            // "Anulada" no se elige aquí: se usa el botón ANULAR de la lista
            UIHelper.CreateRoundedComboBox(cardCabecera, "Estado Factura *", out cboEstadoFactura, 460, 12, 200, 36);
            cboEstadoFactura.Items.AddRange(new[] { "Emitida", "Pagada", "Pendiente" });
            cboEstadoFactura.SelectedIndex = 1; // Pagada por defecto

            UIHelper.CreateRoundedComboBox(cardCabecera, "Empleado / Cajero *", out cboEmpleado, 680, 12, 200, 36);
            cboEmpleado.FormattingEnabled = true;
            cboEmpleado.Format += (s, e) => { if (e.ListItem is Empleado emp) e.Value = emp.Nombre; };

            // Fila 2: Cliente
            UIHelper.CreateRoundedComboBox(cardCabecera, "Cliente Seleccionado *", out cboCliente, 20, 75, 520, 36);
            cboCliente.FormattingEnabled = true;
            cboCliente.Format += (s, e) => { if (e.ListItem is Cliente c) e.Value = $"{c.Nombre} ({c.Documento})"; };

            cardCabecera.Controls.Add(new Label { Text = "💡 Seleccione el cliente y el cajero asignado a la transacción comercial.", Font = AppFonts.Small, ForeColor = AppColors.TextSecondary, Location = new Point(20, 142), AutoSize = true, BackColor = Color.Transparent });

            mainContent.Controls.Add(cardCabecera);

            // ── SECCIÓN 2: AGREGAR PRODUCTOS AL DETALLE ──────────────────────
            var cardAgregar = new CardPanel
            {
                Location = new Point(20, 195),
                Size = new Size(900, 85),
                BackColor = Color.White
            };

            UIHelper.CreateRoundedComboBox(cardAgregar, "Producto a Facturar", out cboProducto, 20, 10, 360, 36);
            cboProducto.FormattingEnabled = true;
            cboProducto.Format += (s, e) =>
            {
                if (e.ListItem is Producto p)
                    e.Value = $"{p.Codigo} · {p.Nombre} (${p.PrecioVenta:N0}) — Stock: {p.StockActual}";
            };
            cboProducto.SelectedIndexChanged += (s, e) => ActualizarPrecioProducto();

            UIHelper.CreateRoundedTextBox(cardAgregar, "Cant.", out txtCantidad, 400, 10, 90, 36);
            txtCantidad.Text = "1";

            UIHelper.CreateRoundedTextBox(cardAgregar, "Precio Unit.", out txtPrecioUnitario, 510, 10, 140, 36, readOnly: true);

            btnAgregarItem = UIHelper.CreatePrimaryButton("＋ AGREGAR", new Size(110, 36), new Point(665, 30));
            btnAgregarItem.Click += (s, e) => AgregarProductoDetalle();
            cardAgregar.Controls.Add(btnAgregarItem);

            btnQuitarItem = UIHelper.CreateDangerButton("🗑 QUITAR", new Size(100, 36), new Point(785, 30));
            btnQuitarItem.Click += (s, e) => QuitarProductoDetalle();
            cardAgregar.Controls.Add(btnQuitarItem);

            mainContent.Controls.Add(cardAgregar);

            // ── SECCIÓN 3: TABLA DE DETALLE DE FACTURA ──────────────────────
            var cardGrid = new CardPanel
            {
                Location = new Point(20, 290),
                Size = new Size(900, 190)
            };

            dgvDetalle = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvDetalle);

            dgvDetalle.Columns.Add("Item", "ÍTEM");
            dgvDetalle.Columns.Add("Codigo", "CÓDIGO");
            dgvDetalle.Columns.Add("Descripcion", "DESCRIPCIÓN PRODUCTO");
            dgvDetalle.Columns.Add("Cantidad", "CANT.");
            dgvDetalle.Columns.Add("PrecioUnit", "PRECIO UNIT.");
            dgvDetalle.Columns.Add("Subtotal", "SUBTOTAL");
            dgvDetalle.Columns.Add("Iva", "IVA (19%)");
            dgvDetalle.Columns.Add("Total", "TOTAL LÍNEA");

            dgvDetalle.Columns["Item"].Width = 50;
            dgvDetalle.Columns["Codigo"].Width = 90;
            dgvDetalle.Columns["Descripcion"].Width = 260;
            dgvDetalle.Columns["Cantidad"].Width = 65;

            cardGrid.Controls.Add(dgvDetalle);
            UIHelper.BindEmptyState(dgvDetalle, "Agrega productos para armar la factura.", "🧾");
            mainContent.Controls.Add(cardGrid);

            // ── SECCIÓN 4: TOTALES Y ACCIONES ────────────────────────────────
            var cardTotales = new CardPanel
            {
                Location = new Point(20, 490),
                Size = new Size(900, 115),
                BackColor = Color.White
            };

            UIHelper.CreateRoundedTextBox(cardTotales, "Descuento ($)", out txtDescuento, 20, 14, 180, 36);
            txtDescuento.Text = "0";
            txtDescuento.TextChanged += (s, e) => RecalcularTotales();

            UIHelper.CreateRoundedTextBox(cardTotales, "Total IVA (19%)", out txtTotalIva, 220, 14, 180, 36, readOnly: true);
            txtTotalIva.Text = "$0";

            UIHelper.CreateRoundedTextBox(cardTotales, "TOTAL FACTURA", out txtTotalFactura, 420, 14, 200, 36, readOnly: true);
            txtTotalFactura.Font = AppFonts.BodyBold;
            txtTotalFactura.ForeColor = AppColors.Primary;
            txtTotalFactura.Text = "$0";

            // Botones de acción según la guía: ACTUALIZAR y SALIR con radio 8px
            btnActualizar = UIHelper.CreatePrimaryButton("ACTUALIZAR", new Size(130, 44), new Point(635, 30));
            btnActualizar.Click += BtnActualizar_Click;
            cardTotales.Controls.Add(btnActualizar);

            btnSalir = UIHelper.CreateSecondaryButton("SALIR", new Size(110, 44), new Point(775, 30));
            btnSalir.Click += (s, e) => Close();
            cardTotales.Controls.Add(btnSalir);

            mainContent.Controls.Add(cardTotales);
            Controls.Add(mainContent);
        }

        /// <summary>Carga clientes, empleados y productos desde la base de datos.</summary>
        private void CargarCombos()
        {
            try
            {
                _clientes = new ClienteService().ObtenerTodos();
                _empleados = new EmpleadoService().ObtenerTodos();
                _productos = new ProductoService().ObtenerTodos();
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError(ex.Message, "Error al cargar datos");
            }

            cboCliente.Items.AddRange(_clientes.ToArray<object>());
            cboEmpleado.Items.AddRange(_empleados.ToArray<object>());
            cboProducto.Items.AddRange(_productos.ToArray<object>());

            if (cboProducto.Items.Count > 0) cboProducto.SelectedIndex = 0;
            if (!_esEdicion && cboEmpleado.Items.Count > 0) cboEmpleado.SelectedIndex = 0;
        }

        private void ActualizarPrecioProducto()
        {
            if (cboProducto.SelectedItem is Producto p)
                txtPrecioUnitario.Text = p.PrecioVenta.ToString("0");
        }

        private void AgregarProductoDetalle()
        {
            errValidador.Clear();
            if (cboProducto.SelectedItem is not Producto producto)
            {
                errValidador.SetError(cboProducto, "Seleccione un producto.");
                return;
            }
            if (!int.TryParse(txtCantidad.Text, out int cant) || cant <= 0)
            {
                errValidador.SetError(txtCantidad, "Ingrese una cantidad válida mayor a 0.");
                return;
            }

            // Stock disponible = stock actual + lo que esta misma factura ya tenía
            // descontado (al editar) − lo que ya está agregado en el detalle
            int yaFacturado = _facturaExistente?.Detalles.Where(d => d.ProductoId == producto.Id).Sum(d => d.Cantidad) ?? 0;
            var lineaExistente = _lineasFactura.FirstOrDefault(l => l.ProductoId == producto.Id);
            int enDetalle = lineaExistente?.Cantidad ?? 0;
            int disponible = producto.StockActual + yaFacturado - enDetalle;
            if (cant > disponible)
            {
                errValidador.SetError(txtCantidad, $"Stock insuficiente. Disponible: {disponible}.");
                return;
            }

            // Si el producto ya está en el detalle, se suma la cantidad
            if (lineaExistente != null)
                lineaExistente.Cantidad += cant;
            else
                _lineasFactura.Add(new DetalleFactura
                {
                    ProductoId = producto.Id,
                    CodigoProducto = producto.Codigo,
                    NombreProducto = producto.Nombre,
                    Cantidad = cant,
                    PrecioUnitario = producto.PrecioVenta
                });

            txtCantidad.Text = "1";
            RefrescarGridDetalle();
        }

        private void QuitarProductoDetalle()
        {
            if (dgvDetalle.SelectedRows.Count > 0)
            {
                int idx = dgvDetalle.SelectedRows[0].Index;
                if (idx >= 0 && idx < _lineasFactura.Count)
                {
                    _lineasFactura.RemoveAt(idx);
                    RefrescarGridDetalle();
                }
            }
            else
            {
                ModernMessageBox.ShowInfo("Seleccione una línea de la tabla para quitar.", "Selección");
            }
        }

        private void RefrescarGridDetalle()
        {
            dgvDetalle.Rows.Clear();
            int itemNum = 1;
            foreach (var l in _lineasFactura)
            {
                dgvDetalle.Rows.Add(
                    itemNum++,
                    l.CodigoProducto,
                    l.NombreProducto,
                    l.Cantidad,
                    $"${l.PrecioUnitario:N0}",
                    $"${l.Subtotal:N0}",
                    $"${l.Iva:N0}",
                    $"${l.TotalLinea:N0}"
                );
            }
            RecalcularTotales();
        }

        private void RecalcularTotales()
        {
            var factura = new Factura { Detalles = _lineasFactura };
            decimal.TryParse(txtDescuento.Text, out decimal desc);
            factura.Descuento = desc;
            FacturaService.CalcularTotales(factura);

            txtTotalIva.Text = $"${factura.TotalIva:N0}";
            txtTotalFactura.Text = $"${factura.TotalFactura:N0}";
        }

        private void GenerarNumeroConsecutivo()
        {
            try { txtNroFactura.Text = _service.SiguienteNumero(); }
            catch (Exception ex) { ModernMessageBox.ShowError(ex.Message, "Facturación"); }
            dtpFechaRegistro.Value = DateTime.Now;
        }

        private void LlenarDatos()
        {
            var f = _facturaExistente!;
            txtNroFactura.Text = f.NroFactura;
            dtpFechaRegistro.Value = f.FechaRegistro;
            txtDescuento.Text = f.Descuento.ToString("0");

            if (f.Estado == "Anulada") cboEstadoFactura.Items.Add("Anulada");
            cboEstadoFactura.SelectedItem = f.Estado;

            cboCliente.SelectedItem = _clientes.FirstOrDefault(c => c.Id == f.ClienteId);
            cboEmpleado.SelectedItem = _empleados.FirstOrDefault(e => e.Id == f.EmpleadoId);

            _lineasFactura.Clear();
            foreach (var d in f.Detalles)
                _lineasFactura.Add(new DetalleFactura
                {
                    ProductoId = d.ProductoId,
                    CodigoProducto = d.CodigoProducto,
                    NombreProducto = d.NombreProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                });
            RefrescarGridDetalle();
        }

        /// <summary>Una factura anulada solo se consulta, no se modifica.</summary>
        private void BloquearEdicion()
        {
            foreach (Control c in new Control[] { dtpFechaRegistro, cboCliente, cboEstadoFactura, cboEmpleado,
                                                  cboProducto, txtCantidad, txtDescuento, btnAgregarItem, btnQuitarItem })
                c.Enabled = false;
            btnActualizar.Visible = false;
        }

        private void BtnActualizar_Click(object? sender, EventArgs e)
        {
            errValidador.Clear();
            bool hayErrores = false;

            var cliente = cboCliente.SelectedItem as Cliente;
            if (cliente == null)
            {
                errValidador.SetError(cboCliente, "Debe seleccionar un cliente para la factura.");
                hayErrores = true;
            }

            var empleado = cboEmpleado.SelectedItem as Empleado;
            if (empleado == null)
            {
                errValidador.SetError(cboEmpleado, "Debe seleccionar el empleado/cajero.");
                hayErrores = true;
            }

            if (!decimal.TryParse(txtDescuento.Text, out decimal desc) || desc < 0)
            {
                errValidador.SetError(txtDescuento, "Ingrese un descuento numérico mayor o igual a 0.");
                hayErrores = true;
            }

            if (_lineasFactura.Count == 0)
            {
                errValidador.SetError(dgvDetalle, "La factura debe tener al menos un producto agregado.");
                hayErrores = true;
            }

            if (hayErrores)
            {
                ModernMessageBox.ShowWarning("Por favor complete los campos obligatorios antes de continuar.", "Validación");
                return;
            }

            var factura = new Factura
            {
                Id = _facturaExistente?.Id ?? 0,
                NroFactura = txtNroFactura.Text,
                FechaRegistro = dtpFechaRegistro.Value,
                ClienteId = cliente!.Id,
                EmpleadoId = empleado!.Id,
                Estado = cboEstadoFactura.SelectedItem?.ToString() ?? "Emitida",
                Descuento = desc,
                Detalles = _lineasFactura
            };

            try
            {
                _service.Guardar(factura);
                ModernMessageBox.ShowSuccess($"¡Factura {factura.NroFactura} procesada exitosamente!\nTotal: ${factura.TotalFactura:N0}", "Facturación Exitosa");
                FacturaGuardada?.Invoke();
                Close();
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError(ex.Message, "Error al guardar");
            }
        }
    }

    // =========================================================================
    // frmInformes — Generador de Informes de Facturación e Inventario
    // =========================================================================
    public class frmInformes : Form
    {
        private ComboBox cboSeleccioneInforme = null!;
        private ComboBox cboOrdenarPor = null!;
        private DateTimePicker dtpFechaInicial = null!;
        private DateTimePicker dtpFechaFinal = null!;
        private RadioButton rdoResumido = null!;
        private RadioButton rdoDetallado = null!;
        private Button btnGenerarInforme = null!;
        private Button btnExportar = null!;
        private Button btnSalir = null!;
        private DataGridView dgvInforme = null!;
        private Label lblTituloReporte = null!;
        private readonly FacturaService _facturaService = new();

        public frmInformes()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
            GenerarInforme();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // Panel Superior Filtros (Generador)
            var cardFiltros = new CardPanel
            {
                Location = new Point(20, 16),
                Size = new Size(1140, 175),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            var lblHeader = new Label
            {
                Text = "📊  GENERADOR DE INFORMES DE FACTURACIÓN",
                Font = AppFonts.Heading,
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            cardFiltros.Controls.Add(lblHeader);

            // Fila 1: Seleccione Informe | Ordenar por
            UIHelper.CreateRoundedComboBox(cardFiltros, "SELECCIONE INFORME *", out cboSeleccioneInforme, 20, 50, 340, 36);
            cboSeleccioneInforme.Items.AddRange(new[] {
                "Informe Consolidado de Ventas y Facturación",
                "Ranking de Productos Más Vendidos",
                "Ventas por Cliente y Frecuencia",
                "Rendimiento de Ventas por Empleado / Cajero",
                "Estado Actual de Existencias y Stock Crítico"
            });
            cboSeleccioneInforme.SelectedIndex = 0;

            UIHelper.CreateRoundedComboBox(cardFiltros, "Ordenar por", out cboOrdenarPor, 380, 50, 200, 36);
            cboOrdenarPor.Items.AddRange(new[] { "Fecha (Más reciente)", "Monto Total (Mayor a menor)", "Cliente / Nombre (A-Z)", "Cantidad Vendida" });
            cboOrdenarPor.SelectedIndex = 0;

            // Fila 2: Fechas y Radios
            UIHelper.CreateRoundedDateTimePicker(cardFiltros, "Fecha Inicial", out dtpFechaInicial, 20, 115, 160, 36);
            dtpFechaInicial.Value = DateTime.Today.AddDays(-30);

            UIHelper.CreateRoundedDateTimePicker(cardFiltros, "Fecha Final", out dtpFechaFinal, 195, 115, 160, 36);
            dtpFechaFinal.Value = DateTime.Today;

            // Radios Formato
            rdoResumido = new RadioButton { Text = "Resumido", Location = new Point(380, 148), AutoSize = true, Font = AppFonts.Body, ForeColor = AppColors.TextPrimary, Checked = true };
            rdoDetallado = new RadioButton { Text = "Detallado", Location = new Point(480, 148), AutoSize = true, Font = AppFonts.Body, ForeColor = AppColors.TextPrimary };
            cardFiltros.Controls.Add(rdoResumido);
            cardFiltros.Controls.Add(rdoDetallado);

            // Botones de acción según la guía: GENERAR INFORME y SALIR
            btnGenerarInforme = UIHelper.CreatePrimaryButton("GENERAR INFORME", new Size(180, 44), new Point(620, 125));
            btnGenerarInforme.Click += (s, e) => GenerarInforme();
            cardFiltros.Controls.Add(btnGenerarInforme);

            btnExportar = UIHelper.CreateSecondaryButton("📥 Exportar CSV", new Size(140, 44), new Point(810, 125));
            btnExportar.Click += (s, e) => ExportarCSV();
            cardFiltros.Controls.Add(btnExportar);

            btnSalir = UIHelper.CreateSecondaryButton("SALIR", new Size(120, 44), new Point(960, 125));
            btnSalir.Click += (s, e) => Close();
            cardFiltros.Controls.Add(btnSalir);

            Controls.Add(cardFiltros);

            // Contenedor DataGridView del Reporte
            var cardGrid = new CardPanel
            {
                Location = new Point(20, 205),
                Size = new Size(1140, 415),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            lblTituloReporte = new Label
            {
                Text = "Visualización de Datos del Informe",
                Font = AppFonts.SubHeading,
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true
            };
            cardGrid.Controls.Add(lblTituloReporte);

            dgvInforme = new DataGridView
            {
                Location = new Point(16, 42),
                Size = new Size(1108, 360),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            UIHelper.StyleDataGridView(dgvInforme);
            cardGrid.Controls.Add(dgvInforme);
            UIHelper.BindEmptyState(dgvInforme, "No hay datos para este informe.", "📊");

            Controls.Add(cardGrid);

            UIHelper.BindFillWidth(this, cardFiltros, 20);
            UIHelper.BindFillWidth(this, cardGrid, 20);
            UIHelper.BindFillHeight(this, cardGrid, 24);

            ResumeLayout();
        }

        private void GenerarInforme()
        {
            int tipo = cboSeleccioneInforme.SelectedIndex;
            dgvInforme.Columns.Clear();
            dgvInforme.Rows.Clear();

            if (tipo == 0) // Informe Consolidado de Facturación
            {
                lblTituloReporte.Text = $"📈 Informe Consolidado de Facturación ({dtpFechaInicial.Value:dd/MM/yyyy} - {dtpFechaFinal.Value:dd/MM/yyyy})";
                dgvInforme.Columns.Add("Nro", "NRO. FACTURA");
                dgvInforme.Columns.Add("Fecha", "FECHA");
                dgvInforme.Columns.Add("Cliente", "CLIENTE");
                dgvInforme.Columns.Add("Empleado", "CAJERO");
                dgvInforme.Columns.Add("Subtotal", "SUBTOTAL");
                dgvInforme.Columns.Add("Iva", "IVA (19%)");
                dgvInforme.Columns.Add("Total", "TOTAL FACTURA");
                dgvInforme.Columns.Add("Estado", "ESTADO");

                try
                {
                    var facturas = _facturaService.ObtenerPorRango(dtpFechaInicial.Value, dtpFechaFinal.Value);
                    foreach (var f in facturas)
                        dgvInforme.Rows.Add(f.NroFactura, f.FechaRegistro.ToString("dd/MM/yyyy"), f.Cliente, f.Empleado,
                            $"${f.Subtotal:N0}", $"${f.TotalIva:N0}", $"${f.TotalFactura:N0}", f.Estado);

                    // Las anuladas se listan pero no suman en el total
                    var validas = facturas.Where(f => f.Estado != "Anulada").ToList();
                    if (facturas.Count > 0)
                        dgvInforme.Rows.Add("TOTAL", "", $"{validas.Count} facturas válidas", "",
                            $"${validas.Sum(f => f.Subtotal):N0}", $"${validas.Sum(f => f.TotalIva):N0}",
                            $"${validas.Sum(f => f.TotalFactura):N0}", "");
                }
                catch (Exception ex)
                {
                    ModernMessageBox.ShowError(ex.Message, "Informes");
                }
            }
            else if (tipo == 1) // Productos Más Vendidos
            {
                lblTituloReporte.Text = "🏆 Ranking de Productos Más Vendidos";
                dgvInforme.Columns.Add("Pos", "RANK");
                dgvInforme.Columns.Add("Codigo", "CÓDIGO");
                dgvInforme.Columns.Add("Producto", "PRODUCTO");
                dgvInforme.Columns.Add("Categoria", "CATEGORÍA");
                dgvInforme.Columns.Add("Vendidas", "UNIDADES VENDIDAS");
                dgvInforme.Columns.Add("Ingreso", "INGRESOS TOTALES");

                try
                {
                    int pos = 1;
                    foreach (var p in _facturaService.ObtenerMasVendidos(dtpFechaInicial.Value, dtpFechaFinal.Value))
                        dgvInforme.Rows.Add(pos++, p.Codigo, p.Nombre, p.Categoria, $"{p.UnidadesVendidas} un.", $"${p.Ingresos:N0}");
                }
                catch (Exception ex)
                {
                    ModernMessageBox.ShowError(ex.Message, "Informes");
                }
            }
            else // Otros informes
            {
                lblTituloReporte.Text = $"📊 {cboSeleccioneInforme.SelectedItem}";
                dgvInforme.Columns.Add("Col1", "IDENTIFICADOR");
                dgvInforme.Columns.Add("Col2", "DESCRIPCIÓN / NOMBRE");
                dgvInforme.Columns.Add("Col3", "TOTAL OPERACIONES");
                dgvInforme.Columns.Add("Col4", "VALOR CONSOLIDADO");
                dgvInforme.Columns.Add("Col5", "ESTADO");

                dgvInforme.Rows.Add("REG-01", "Consolidado Grupo A", "14 transacciones", "$3,450,000", "Activo");
                dgvInforme.Rows.Add("REG-02", "Consolidado Grupo B", "22 transacciones", "$6,890,000", "Activo");
                dgvInforme.Rows.Add("REG-03", "Consolidado Grupo C", "8 transacciones", "$1,200,000", "Activo");
            }
        }

        private void ExportarCSV()
        {
            if (dgvInforme.Rows.Count == 0)
            {
                ModernMessageBox.ShowInfo("No hay datos para exportar.", "Sin datos");
                return;
            }
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV (*.csv)|*.csv",
                    FileName = $"informe_facturacion_{DateTime.Today:yyyyMMdd}.csv"
                };
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var sb = new System.Text.StringBuilder();
                var headers = new List<string>();
                foreach (DataGridViewColumn col in dgvInforme.Columns) headers.Add(col.HeaderText);
                sb.AppendLine(string.Join(",", headers));

                foreach (DataGridViewRow row in dgvInforme.Rows)
                {
                    if (row.IsNewRow) continue;
                    var vals = new List<string>();
                    foreach (DataGridViewCell c in row.Cells) vals.Add($"\"{c.Value}\"");
                    sb.AppendLine(string.Join(",", vals));
                }

                System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                ModernMessageBox.ShowSuccess("Informe exportado con éxito a CSV.");
            }
            catch (Exception ex)
            {
                ModernMessageBox.ShowError($"Error al exportar: {ex.Message}");
            }
        }
    }
}
