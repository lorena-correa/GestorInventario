using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;

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
        private Button btnVerDetalle = null!;
        private Button btnAnular = null!;
        private ComboBox cboFiltroEstado = null!;

        private readonly List<FacturaItem> _facturas = new()
        {
            new FacturaItem { Id = 1, NroFactura = "FAC-00101", FechaRegistro = DateTime.Today.AddDays(-3), Cliente = "Carlos Andrés Pérez", Empleado = "Lorena Correa (Cajera)", Estado = "Pagada", Descuento = 0, TotalIva = 95000, TotalFactura = 595000 },
            new FacturaItem { Id = 2, NroFactura = "FAC-00102", FechaRegistro = DateTime.Today.AddDays(-2), Cliente = "Distribuidora Los Andes S.A.S.", Empleado = "Ana María Torres", Estado = "Pagada", Descuento = 50000, TotalIva = 247000, TotalFactura = 1547000 },
            new FacturaItem { Id = 3, NroFactura = "FAC-00103", FechaRegistro = DateTime.Today.AddDays(-1), Cliente = "María Fernanda Gómez", Empleado = "Lorena Correa (Cajera)", Estado = "Pendiente", Descuento = 10000, TotalIva = 43700, TotalFactura = 273700 },
            new FacturaItem { Id = 4, NroFactura = "FAC-00104", FechaRegistro = DateTime.Today, Cliente = "Tecnología Global S.A.", Empleado = "Javier Saldarriaga", Estado = "Pagada", Descuento = 0, TotalIva = 380000, TotalFactura = 2380000 },
            new FacturaItem { Id = 5, NroFactura = "FAC-00105", FechaRegistro = DateTime.Today, Cliente = "Juan David Morales", Empleado = "Lorena Correa (Cajera)", Estado = "Emitida", Descuento = 0, TotalIva = 34200, TotalFactura = 214200 }
        };

        private FacturaItem? _facturaSeleccionada;

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
                Size = new Size(1140, 76),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            var lblTitulo = new Label
            {
                Text = "🧾  ADMINISTRACIÓN DE FACTURAS",
                Font = AppFonts.Heading,
                ForeColor = AppColors.TextPrimary,
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            toolbarCard.Controls.Add(lblTitulo);

            var lblSubtitulo = new Label
            {
                Text = "Registro de ventas, emisión de comprobantes fiscales y gestión de cobros",
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
                Location = new Point(410, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent,
                Height = 44
            };

            // Filtro Estado
            cboFiltroEstado = new ComboBox
            {
                Size = new Size(115, 38),
                Font = AppFonts.Body,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cboFiltroEstado.Items.AddRange(new[] { "Todos", "Pagada", "Pendiente", "Emitida", "Anulada" });
            cboFiltroEstado.SelectedIndex = 0;
            cboFiltroEstado.SelectedIndexChanged += (s, e) => CargarDatos(txtBuscar.Text);
            cboFiltroEstado.Margin = new Padding(0, 3, 6, 0);
            actionsPanel.Controls.Add(cboFiltroEstado);

            // Buscador moderno
            UIHelper.CreateSearchInput(actionsPanel, out txtBuscar, 0, 0, 190, 38, "Buscar factura...");
            txtBuscar.TextChanged += (s, e) => CargarDatos(txtBuscar.Text);

            btnNuevaFactura = UIHelper.CreatePrimaryButton("＋ NUEVA", new Size(100, 38), new Point(0, 0));
            btnNuevaFactura.Margin = new Padding(6, 0, 0, 0);
            btnNuevaFactura.Click += (s, e) => AbrirFormularioFactura(null);
            actionsPanel.Controls.Add(btnNuevaFactura);

            btnVerDetalle = UIHelper.CreateSecondaryButton("👁️ VER", new Size(85, 38), new Point(0, 0));
            btnVerDetalle.Margin = new Padding(6, 0, 0, 0);
            btnVerDetalle.Click += (s, e) =>
            {
                if (_facturaSeleccionada == null)
                {
                    MessageBox.Show("Por favor seleccione una factura de la lista.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                AbrirFormularioFactura(_facturaSeleccionada);
            };
            actionsPanel.Controls.Add(btnVerDetalle);

            btnAnular = UIHelper.CreateDangerButton("🚫 ANULAR", new Size(95, 38), new Point(0, 0));
            btnAnular.Margin = new Padding(6, 0, 0, 0);
            btnAnular.Click += (s, e) =>
            {
                if (_facturaSeleccionada == null)
                {
                    MessageBox.Show("Por favor seleccione una factura para anular.", "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (MessageBox.Show($"¿Desea anular la factura {_facturaSeleccionada.NroFactura}?", "Confirmar Anulación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _facturaSeleccionada.Estado = "Anulada";
                    CargarDatos(txtBuscar.Text);
                    MessageBox.Show("Factura anulada correctamente.", "Facturación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            actionsPanel.Controls.Add(btnAnular);

            toolbarCard.Controls.Add(actionsPanel);
            Controls.Add(toolbarCard);

            var cardGrid = new CardPanel
            {
                Location = new Point(20, 104),
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

            dgvFacturas.Columns["NroFactura"].Width = 130;
            dgvFacturas.Columns["Fecha"].Width = 140;
            dgvFacturas.Columns["Cliente"].Width = 240;
            dgvFacturas.Columns["Empleado"].Width = 190;
            dgvFacturas.Columns["TotalFactura"].Width = 150;

            dgvFacturas.SelectionChanged += (s, e) =>
            {
                if (dgvFacturas.SelectedRows.Count > 0)
                    _facturaSeleccionada = dgvFacturas.SelectedRows[0].Tag as FacturaItem;
            };

            dgvFacturas.CellDoubleClick += (s, e) =>
            {
                if (_facturaSeleccionada != null)
                    AbrirFormularioFactura(_facturaSeleccionada);
            };

            cardGrid.Controls.Add(dgvFacturas);
            Controls.Add(cardGrid);

            ResumeLayout();
        }

        private void CargarDatos(string filtro = "")
        {
            dgvFacturas.Rows.Clear();
            string estadoFiltro = cboFiltroEstado.SelectedItem?.ToString() ?? "Todos";

            foreach (var f in _facturas)
            {
                if (estadoFiltro != "Todos" && f.Estado != estadoFiltro)
                    continue;

                if (!string.IsNullOrEmpty(filtro) &&
                    !f.NroFactura.Contains(filtro, StringComparison.OrdinalIgnoreCase) &&
                    !f.Cliente.Contains(filtro, StringComparison.OrdinalIgnoreCase) &&
                    !f.Empleado.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    continue;

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

        private void AbrirFormularioFactura(FacturaItem? factura)
        {
            var form = new frmFacturas(factura);
            form.FacturaGuardada += (nuevaFactura) =>
            {
                if (factura == null)
                {
                    nuevaFactura.Id = _facturas.Count + 1;
                    _facturas.Insert(0, nuevaFactura);
                }
                else
                {
                    factura.Cliente = nuevaFactura.Cliente;
                    factura.Empleado = nuevaFactura.Empleado;
                    factura.Estado = nuevaFactura.Estado;
                    factura.Descuento = nuevaFactura.Descuento;
                    factura.TotalIva = nuevaFactura.TotalIva;
                    factura.TotalFactura = nuevaFactura.TotalFactura;
                }
                CargarDatos(txtBuscar.Text);
            };
            form.ShowDialog(this);
        }
    }

    // =========================================================================
    // frmFacturas — Formulario de Facturación (Módulo Maestro-Detalle con ErrorProvider)
    // =========================================================================
    public class frmFacturas : Form
    {
        public event Action<FacturaItem>? FacturaGuardada;
        private readonly FacturaItem? _facturaExistente;
        private readonly bool _esSoloLectura;

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

        private readonly List<ItemLineaFactura> _lineasFactura = new();

        public frmFacturas(FacturaItem? factura = null)
        {
            _facturaExistente = factura;
            _esSoloLectura = factura != null;

            Size = new Size(960, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;

            BuildUI();
            if (_facturaExistente != null) LlenarDatos();
            else GenerarNumeroConsecutivo();
        }

        private void BuildUI()
        {
            errValidador = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // Encabezado estándar unificado
            var header = UIHelper.CreateModalHeader(this,
                _esSoloLectura ? $"DETALLE DE FACTURA — {_facturaExistente!.NroFactura}" : "ADMINISTRACIÓN DE FACTURAS (NUEVA EMISIÓN)",
                "🧾");
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
            
            var lblFecha = new Label { Text = "Fecha Registro *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(240, 12), AutoSize = true, BackColor = Color.Transparent };
            dtpFechaRegistro = new DateTimePicker { Location = new Point(240, 32), Size = new Size(200, 32), Font = AppFonts.Body, Format = DateTimePickerFormat.Short };
            cardCabecera.Controls.Add(lblFecha);
            cardCabecera.Controls.Add(dtpFechaRegistro);

            UIHelper.CreateRoundedComboBox(cardCabecera, "Estado Factura *", out cboEstadoFactura, 460, 12, 200, 36);
            cboEstadoFactura.Items.AddRange(new[] { "Emitida", "Pagada", "Pendiente", "Anulada" });
            cboEstadoFactura.SelectedIndex = 1; // Pagada por defecto

            UIHelper.CreateRoundedComboBox(cardCabecera, "Empleado / Cajero *", out cboEmpleado, 680, 12, 200, 36);
            cboEmpleado.Items.AddRange(new[] { "Lorena Correa (Cajera)", "Ana María Torres", "Javier Saldarriaga (Admin)", "Carlos Vendedor" });
            cboEmpleado.SelectedIndex = 0;

            // Fila 2: Cliente
            UIHelper.CreateRoundedComboBox(cardCabecera, "Cliente Seleccionado *", out cboCliente, 20, 75, 520, 36);
            cboCliente.Items.AddRange(new[] {
                "Carlos Andrés Pérez (1020304050)",
                "María Fernanda Gómez (1030405060)",
                "Distribuidora Los Andes S.A.S. (900123456-1)",
                "Juan David Morales (1017894561)",
                "Tecnología Global S.A. (890123987-4)",
                "Laura Patricia Restrepo (1035678912)",
                "Cliente Mostrador / Venta Rápida"
            });
            cboCliente.SelectedIndex = 0;

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
            cboProducto.Items.AddRange(new[] {
                "PRD-001 · Monitor Samsung 24\" ($450,000)",
                "PRD-002 · Teclado Mecánico RGB ($130,000)",
                "PRD-003 · Cable HDMI 2m 4K ($22,000)",
                "PRD-004 · Mouse Inalámbrico 2.4GHz ($58,000)",
                "PRD-005 · Hub USB 3.0 4 Puertos ($48,000)",
                "PRD-006 · Disco SSD 500GB SATA ($260,000)"
            });
            cboProducto.SelectedIndex = 0;
            cboProducto.SelectedIndexChanged += (s, e) => ActualizarPrecioProducto();

            UIHelper.CreateRoundedTextBox(cardAgregar, "Cant.", out txtCantidad, 400, 10, 90, 36);
            txtCantidad.Text = "1";

            UIHelper.CreateRoundedTextBox(cardAgregar, "Precio Unit.", out txtPrecioUnitario, 510, 10, 140, 36, readOnly: true);
            ActualizarPrecioProducto();

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

            // Agregar un producto inicial por defecto para demostración
            if (_facturaExistente == null)
            {
                _lineasFactura.Add(new ItemLineaFactura { Codigo = "PRD-001", Descripcion = "Monitor Samsung 24\"", Cantidad = 1, PrecioUnitario = 450000 });
                RefrescarGridDetalle();
            }
        }

        private void ActualizarPrecioProducto()
        {
            if (cboProducto.SelectedIndex == 0) txtPrecioUnitario.Text = "450000";
            else if (cboProducto.SelectedIndex == 1) txtPrecioUnitario.Text = "130000";
            else if (cboProducto.SelectedIndex == 2) txtPrecioUnitario.Text = "22000";
            else if (cboProducto.SelectedIndex == 3) txtPrecioUnitario.Text = "58000";
            else if (cboProducto.SelectedIndex == 4) txtPrecioUnitario.Text = "48000";
            else if (cboProducto.SelectedIndex == 5) txtPrecioUnitario.Text = "260000";
        }

        private void AgregarProductoDetalle()
        {
            errValidador.Clear();
            if (!int.TryParse(txtCantidad.Text, out int cant) || cant <= 0)
            {
                errValidador.SetError(txtCantidad, "Ingrese una cantidad válida mayor a 0.");
                return;
            }

            decimal.TryParse(txtPrecioUnitario.Text, out decimal precio);
            string prodTexto = cboProducto.SelectedItem?.ToString() ?? "";
            var partes = prodTexto.Split('·');
            string codigo = partes.Length > 0 ? partes[0].Trim() : "PRD";
            string desc = partes.Length > 1 ? partes[1].Split('(')[0].Trim() : prodTexto;

            _lineasFactura.Add(new ItemLineaFactura
            {
                Codigo = codigo,
                Descripcion = desc,
                Cantidad = cant,
                PrecioUnitario = precio
            });

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
                MessageBox.Show("Seleccione una línea de la tabla para quitar.", "Selección", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RefrescarGridDetalle()
        {
            dgvDetalle.Rows.Clear();
            int itemNum = 1;
            foreach (var l in _lineasFactura)
            {
                decimal subtotal = l.Cantidad * l.PrecioUnitario;
                decimal iva = subtotal * 0.19m;
                decimal totalLinea = subtotal + iva;

                dgvDetalle.Rows.Add(
                    itemNum++,
                    l.Codigo,
                    l.Descripcion,
                    l.Cantidad,
                    $"${l.PrecioUnitario:N0}",
                    $"${subtotal:N0}",
                    $"${iva:N0}",
                    $"${totalLinea:N0}"
                );
            }
            RecalcularTotales();
        }

        private void RecalcularTotales()
        {
            decimal sumaSubtotal = _lineasFactura.Sum(l => l.Cantidad * l.PrecioUnitario);
            decimal sumaIva = sumaSubtotal * 0.19m;
            decimal.TryParse(txtDescuento.Text, out decimal desc);
            decimal totalFactura = Math.Max(0, (sumaSubtotal + sumaIva) - desc);

            txtTotalIva.Text = $"${sumaIva:N0}";
            txtTotalFactura.Text = $"${totalFactura:N0}";
        }

        private void GenerarNumeroConsecutivo()
        {
            var rand = new Random();
            txtNroFactura.Text = $"FAC-{rand.Next(10100, 99999)}";
            dtpFechaRegistro.Value = DateTime.Now;
        }

        private void LlenarDatos()
        {
            var f = _facturaExistente!;
            txtNroFactura.Text = f.NroFactura;
            dtpFechaRegistro.Value = f.FechaRegistro;
            txtDescuento.Text = f.Descuento.ToString("N0");
            txtTotalIva.Text = $"${f.TotalIva:N0}";
            txtTotalFactura.Text = $"${f.TotalFactura:N0}";

            if (cboEstadoFactura.Items.Contains(f.Estado))
                cboEstadoFactura.SelectedItem = f.Estado;

            // Mock detalle existente
            _lineasFactura.Clear();
            _lineasFactura.Add(new ItemLineaFactura { Codigo = "PRD-001", Descripcion = "Venta General Facturada", Cantidad = 1, PrecioUnitario = f.TotalFactura - f.TotalIva });
            RefrescarGridDetalle();
        }

        private void BtnActualizar_Click(object? sender, EventArgs e)
        {
            errValidador.Clear();
            bool hayErrores = false;

            if (cboCliente.SelectedIndex < 0)
            {
                errValidador.SetError(cboCliente, "Debe seleccionar un cliente para la factura.");
                hayErrores = true;
            }

            if (cboEmpleado.SelectedIndex < 0)
            {
                errValidador.SetError(cboEmpleado, "Debe seleccionar el empleado/cajero.");
                hayErrores = true;
            }

            if (_lineasFactura.Count == 0)
            {
                errValidador.SetError(dgvDetalle, "La factura debe tener al menos un producto agregado.");
                hayErrores = true;
            }

            if (hayErrores)
            {
                MessageBox.Show("Por favor complete los campos obligatorios antes de continuar.",
                    "Validación con ErrorProvider", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal.TryParse(txtDescuento.Text, out decimal desc);
            decimal sumaSubtotal = _lineasFactura.Sum(l => l.Cantidad * l.PrecioUnitario);
            decimal totalIva = sumaSubtotal * 0.19m;
            decimal totalFactura = Math.Max(0, (sumaSubtotal + totalIva) - desc);

            var facturaObj = new FacturaItem
            {
                NroFactura = txtNroFactura.Text,
                FechaRegistro = dtpFechaRegistro.Value,
                Cliente = cboCliente.SelectedItem?.ToString() ?? "",
                Empleado = cboEmpleado.SelectedItem?.ToString() ?? "",
                Estado = cboEstadoFactura.SelectedItem?.ToString() ?? "Emitida",
                Descuento = desc,
                TotalIva = totalIva,
                TotalFactura = totalFactura
            };

            FacturaGuardada?.Invoke(facturaObj);
            MessageBox.Show($"¡Factura {facturaObj.NroFactura} procesada exitosamente!\nTotal: ${facturaObj.TotalFactura:N0}",
                "Facturación Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }

    public class FacturaItem
    {
        public int Id { get; set; }
        public string NroFactura { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Empleado { get; set; } = string.Empty;
        public string Estado { get; set; } = "Emitida";
        public decimal Descuento { get; set; }
        public decimal TotalIva { get; set; }
        public decimal TotalFactura { get; set; }
    }

    public class ItemLineaFactura
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
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
            var lblSelInf = new Label { Text = "SELECCIONE INFORME *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(20, 50), AutoSize = true };
            cboSeleccioneInforme = new ComboBox { Location = new Point(20, 72), Size = new Size(340, 32), Font = AppFonts.Body, DropDownStyle = ComboBoxStyle.DropDownList };
            cboSeleccioneInforme.Items.AddRange(new[] {
                "Informe Consolidado de Ventas y Facturación",
                "Ranking de Productos Más Vendidos",
                "Ventas por Cliente y Frecuencia",
                "Rendimiento de Ventas por Empleado / Cajero",
                "Estado Actual de Existencias y Stock Crítico"
            });
            cboSeleccioneInforme.SelectedIndex = 0;
            cardFiltros.Controls.Add(lblSelInf);
            cardFiltros.Controls.Add(cboSeleccioneInforme);

            var lblOrd = new Label { Text = "Ordenar por", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(380, 50), AutoSize = true };
            cboOrdenarPor = new ComboBox { Location = new Point(380, 72), Size = new Size(200, 32), Font = AppFonts.Body, DropDownStyle = ComboBoxStyle.DropDownList };
            cboOrdenarPor.Items.AddRange(new[] { "Fecha (Más reciente)", "Monto Total (Mayor a menor)", "Cliente / Nombre (A-Z)", "Cantidad Vendida" });
            cboOrdenarPor.SelectedIndex = 0;
            cardFiltros.Controls.Add(lblOrd);
            cardFiltros.Controls.Add(cboOrdenarPor);

            // Fila 2: Fechas y Radios
            var lblFecIni = new Label { Text = "Fecha Inicial", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(20, 115), AutoSize = true };
            dtpFechaInicial = new DateTimePicker { Location = new Point(20, 135), Size = new Size(160, 32), Font = AppFonts.Body, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };
            cardFiltros.Controls.Add(lblFecIni);
            cardFiltros.Controls.Add(dtpFechaInicial);

            var lblFecFin = new Label { Text = "Fecha Final", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(195, 115), AutoSize = true };
            dtpFechaFinal = new DateTimePicker { Location = new Point(195, 135), Size = new Size(160, 32), Font = AppFonts.Body, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            cardFiltros.Controls.Add(lblFecFin);
            cardFiltros.Controls.Add(dtpFechaFinal);

            // Radios Formato
            rdoResumido = new RadioButton { Text = "Resumido", Location = new Point(380, 138), AutoSize = true, Font = AppFonts.Body, Checked = true };
            rdoDetallado = new RadioButton { Text = "Detallado", Location = new Point(480, 138), AutoSize = true, Font = AppFonts.Body };
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

            Controls.Add(cardGrid);
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

                dgvInforme.Rows.Add("FAC-00101", "25/08/2026", "Carlos Andrés Pérez", "Lorena Correa", "$500,000", "$95,000", "$595,000", "Pagada");
                dgvInforme.Rows.Add("FAC-00102", "26/08/2026", "Distribuidora Los Andes", "Ana María Torres", "$1,300,000", "$247,000", "$1,547,000", "Pagada");
                dgvInforme.Rows.Add("FAC-00103", "27/08/2026", "María Fernanda Gómez", "Lorena Correa", "$230,000", "$43,700", "$273,700", "Pendiente");
                dgvInforme.Rows.Add("FAC-00104", "28/08/2026", "Tecnología Global S.A.", "Javier Saldarriaga", "$2,000,000", "$380,000", "$2,380,000", "Pagada");
                dgvInforme.Rows.Add("TOTAL", "", "4 Facturas emitidas", "", "$4,030,000", "$765,700", "$4,795,700", "OK");
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

                dgvInforme.Rows.Add("1", "PRD-001", "Monitor Samsung 24\"", "Electrónica", "38 un.", "$17,100,000");
                dgvInforme.Rows.Add("2", "PRD-006", "Disco SSD 500GB", "Almacenamiento", "29 un.", "$7,540,000");
                dgvInforme.Rows.Add("3", "PRD-002", "Teclado Mecánico RGB", "Periféricos", "24 un.", "$3,120,000");
                dgvInforme.Rows.Add("4", "PRD-004", "Mouse Inalámbrico", "Periféricos", "21 un.", "$1,218,000");
                dgvInforme.Rows.Add("5", "PRD-003", "Cable HDMI 2m", "Cables", "45 un.", "$990,000");
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
                MessageBox.Show("No hay datos para exportar.", "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("Informe exportado con éxito a CSV.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
