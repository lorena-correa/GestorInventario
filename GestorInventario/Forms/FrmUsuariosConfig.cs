using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;
using GestorInventario.Models;
using GestorInventario.Services;

namespace GestorInventario.Forms
{
    // ═══════════════════════════════════════════════════════════════
    // Formulario de Usuarios
    // ═══════════════════════════════════════════════════════════════
    public class FrmUsuarios : Form
    {
        private DataGridView dgv = null!;
        private TextBox txtBuscar = null!;
        private readonly UsuarioService _service = new();
        private Usuario? _selected;

        public FrmUsuarios()
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

            txtBuscar = new TextBox { Location = new Point(28, 8), Size = new Size(280, 36), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Buscar usuario..." };
            txtBuscar.TextChanged += (s, e) => LoadData(txtBuscar.Text);
            toolbar.Controls.Add(txtBuscar);

            var btnNuevo = UIHelper.CreatePrimaryButton("＋  Nuevo Usuario", new Size(170, 40), new Point(320, 6));
            btnNuevo.Click += (s, e) => OpenForm(null);
            toolbar.Controls.Add(btnNuevo);

            var btnEditar = UIHelper.CreateSecondaryButton("✏️  Editar", new Size(110, 40), new Point(500, 6));
            btnEditar.Click += (s, e) => { if (_selected == null) { ShowWarn(); return; } OpenForm(_selected); };
            toolbar.Controls.Add(btnEditar);

            var btnEliminar = UIHelper.CreateDangerButton("🗑  Eliminar", new Size(110, 40), new Point(620, 6));
            btnEliminar.Click += (s, e) => { if (_selected == null) { ShowWarn(); return; } DeleteSelected(); };
            toolbar.Controls.Add(btnEliminar);

            Controls.Add(toolbar);

            var card = new CardPanel { Location = new Point(24, 92), Size = new Size(1100, 480) };
            dgv = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgv);
            dgv.Columns.Add("UserId", "ID");
            dgv.Columns.Add("Nombre", "Nombre");
            dgv.Columns.Add("Email", "Correo");
            dgv.Columns.Add("Rol", "Rol");
            dgv.Columns.Add("Estado", "Estado");
            dgv.Columns["UserId"].Visible = false;

            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.SelectedRows.Count > 0)
                    _selected = dgv.SelectedRows[0].Tag as Usuario;
            };
            dgv.CellDoubleClick += (s, e) => { if (_selected != null) OpenForm(_selected); };

            card.Controls.Add(dgv);
            Controls.Add(card);
            ResumeLayout();
        }

        private void LoadData(string search = "")
        {
            try
            {
                dgv.Rows.Clear();
                var usuarios = _service.ObtenerTodos();
                foreach (var u in usuarios)
                {
                    if (!string.IsNullOrEmpty(search) &&
                        !u.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                        !u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) continue;

                    int r = dgv.Rows.Add(u.Id, u.Nombre, u.Email, u.Rol,
                        u.Activo ? "Activo" : "Inactivo");
                    dgv.Rows[r].Tag = u;
                    dgv.Rows[r].Cells["Estado"].Style.ForeColor =
                        u.Activo ? AppColors.Success : AppColors.Danger;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenForm(Usuario? u)
        {
            var frm = new FrmUsuarioDetalle(u);
            frm.Saved += () => LoadData();
            frm.ShowDialog(this);
        }

        private void DeleteSelected()
        {
            if (_selected!.Id == Config.Session.UserId)
            {
                MessageBox.Show("No puedes eliminar tu propio usuario.",
                    "Operación no permitida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show($"¿Eliminar usuario \"{_selected.Nombre}\"?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _service.Eliminar(_selected.Id);
                    _selected = null;
                    LoadData();
                    MessageBox.Show("Usuario eliminado.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowWarn() =>
            MessageBox.Show("Selecciona un usuario de la lista.",
                "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ═══════════════════════════════════════════════════════════════
    // Formulario UsuarioDetalle — Crear y Editar usuario
    // ═══════════════════════════════════════════════════════════════
    public class FrmUsuarioDetalle : Form
    {
        public event Action? Saved;
        private readonly Usuario? _usuario;
        private readonly bool _isEdit;
        private readonly UsuarioService _service = new();
        private TextBox txtNombre = null!, txtEmail = null!, txtPassword = null!;
        private ComboBox cboRol = null!;

        public FrmUsuarioDetalle(Usuario? usuario = null)
        {
            _usuario = usuario;
            _isEdit = usuario != null;
            Size = new Size(520, 460);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;
            BuildUI();
            if (_isEdit) FillData();
        }

        private void BuildUI()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = AppColors.Primary };
            var lblTitle = new Label { Text = _isEdit ? "✏️  Editar Usuario" : "➕  Nuevo Usuario", Font = AppFonts.SubHeading, ForeColor = Color.White, Location = new Point(20, 18), AutoSize = true };
            var btnX = new Label { Text = "✕", Font = new Font("Segoe UI", 16f), ForeColor = Color.White, Cursor = Cursors.Hand, Location = new Point(480, 16), AutoSize = true };
            btnX.Click += (s, e) => Close();
            header.Controls.AddRange(new Control[] { lblTitle, btnX });
            Controls.Add(header);

            int x = 30, y = 80, w = 440;

            AddField("NOMBRE COMPLETO *", out txtNombre, x, y, w); y += 80;
            AddField("CORREO ELECTRÓNICO *", out txtEmail, x, y, w); y += 80;
            AddField(_isEdit ? "NUEVA CONTRASEÑA (dejar vacío para no cambiar)"
                             : "CONTRASEÑA *",
                     out txtPassword, x, y, w);
            txtPassword.UseSystemPasswordChar = true;
            y += 80;

            var lblRol = new Label { Text = "ROL *", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true };
            cboRol = new ComboBox { Location = new Point(x, y + 22), Size = new Size(w, 36), Font = AppFonts.Body, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cboRol.Items.AddRange(new[] { "Administrador", "Operador", "Consultor" });
            cboRol.SelectedIndex = 1;
            Controls.Add(lblRol);
            Controls.Add(cboRol);
            y += 80;

            var btnGuardar = UIHelper.CreatePrimaryButton("💾  Guardar", new Size(160, 44), new Point(x, y));
            btnGuardar.Click += BtnGuardar_Click;
            Controls.Add(btnGuardar);

            var btnCancelar = UIHelper.CreateSecondaryButton("✕  Cancelar", new Size(130, 44), new Point(x + 170, y));
            btnCancelar.Click += (s, e) => Close();
            Controls.Add(btnCancelar);
        }

        private void AddField(string label, out TextBox txt, int x, int y, int w)
        {
            var lbl = new Label { Text = label, Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(x, y), AutoSize = true };
            txt = new TextBox { Location = new Point(x, y + 22), Size = new Size(w, 36), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(lbl);
            Controls.Add(txt);
        }

        private void FillData()
        {
            txtNombre.Text = _usuario!.Nombre;
            txtEmail.Text = _usuario.Email;
            if (cboRol.Items.Contains(_usuario.Rol))
                cboRol.SelectedItem = _usuario.Rol;
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Nombre y correo son obligatorios.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!txtEmail.Text.Contains('@'))
            {
                MessageBox.Show("El correo no es válido.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!_isEdit && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("La contraseña es obligatoria para nuevos usuarios.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var u = _isEdit ? _usuario! : new Usuario();
            u.Nombre = txtNombre.Text.Trim();
            u.Email = txtEmail.Text.Trim();
            u.Rol = cboRol.SelectedItem?.ToString() ?? "Operador";
            u.Activo = true;

            if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                u.PasswordHash = AuthService.HashPassword(txtPassword.Text);

            try
            {
                _service.Guardar(u);
                MessageBox.Show("Usuario guardado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    // ═══════════════════════════════════════════════════════════════
    // Formulario de Reportes
    // ═══════════════════════════════════════════════════════════════
    public class FrmReportes : Form
    {
        private readonly ProductoService _prodService = new();
        private readonly MovimientoService _movService = new();
        private DataGridView dgv = null!;
        private Label lblTitulo = null!;

        public FrmReportes()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // Botones de tipo de reporte
            var btnStock = UIHelper.CreatePrimaryButton("📦  Stock Actual", new Size(180, 44), new Point(24, 24));
            btnStock.Click += (s, e) => GenerarReporteStock();
            Controls.Add(btnStock);

            var btnCriticos = UIHelper.CreateDangerButton("⚠️  Productos Críticos", new Size(200, 44), new Point(214, 24));
            btnCriticos.Click += (s, e) => GenerarReporteCriticos();
            Controls.Add(btnCriticos);

            var btnMovimientos = UIHelper.CreateSecondaryButton("🔄  Movimientos del Mes", new Size(210, 44), new Point(424, 24));
            btnMovimientos.Click += (s, e) => GenerarReporteMovimientos();
            Controls.Add(btnMovimientos);

            var btnExportar = UIHelper.CreateSecondaryButton("📥  Exportar CSV", new Size(160, 44), new Point(644, 24));
            btnExportar.Click += (s, e) => ExportarCSV();
            Controls.Add(btnExportar);

            lblTitulo = new Label { Text = "Selecciona un tipo de reporte", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(24, 88), AutoSize = true, BackColor = Color.Transparent };
            Controls.Add(lblTitulo);

            var tableCard = new CardPanel { Location = new Point(24, 120), Size = new Size(1200, 520), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom };
            dgv = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgv);
            tableCard.Controls.Add(dgv);
            Controls.Add(tableCard);

            GenerarReporteStock();
            ResumeLayout();
        }

        private void GenerarReporteStock()
        {
            try
            {
                lblTitulo.Text = $"📦  Reporte de Stock Actual — {DateTime.Today:dd/MM/yyyy}";
                dgv.Columns.Clear();
                dgv.Columns.Add("Codigo", "Código");
                dgv.Columns.Add("Nombre", "Producto");
                dgv.Columns.Add("Categoria", "Categoría");
                dgv.Columns.Add("Proveedor", "Proveedor");
                dgv.Columns.Add("StockActual", "Stock Actual");
                dgv.Columns.Add("StockMin", "Stock Mín.");
                dgv.Columns.Add("PrecioVenta", "Precio Venta");
                dgv.Columns.Add("ValorTotal", "Valor Total");
                dgv.Columns.Add("Estado", "Estado");
                dgv.Rows.Clear();

                decimal totalValor = 0;
                foreach (var p in _prodService.ObtenerTodos())
                {
                    decimal valorTotal = p.StockActual * p.PrecioVenta;
                    totalValor += valorTotal;
                    string estado = p.EsCritico ? "⚠️ Crítico" : "✅ Normal";
                    int r = dgv.Rows.Add(
                        p.Codigo, p.Nombre, p.Categoria, p.Proveedor,
                        p.StockActual, p.StockMinimo,
                        $"${p.PrecioVenta:N0}", $"${valorTotal:N0}", estado);

                    if (p.EsCritico)
                        dgv.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Danger;
                    else
                        dgv.Rows[r].Cells["Estado"].Style.ForeColor = AppColors.Success;
                }

                // Fila de total
                int tot = dgv.Rows.Add("", "TOTAL", "", "", "", "", "", $"${totalValor:N0}", "");
                dgv.Rows[tot].DefaultCellStyle.Font = AppFonts.BodyBold;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerarReporteCriticos()
        {
            try
            {
                lblTitulo.Text = $"⚠️  Reporte Productos con Stock Crítico — {DateTime.Today:dd/MM/yyyy}";
                dgv.Columns.Clear();
                dgv.Columns.Add("Codigo", "Código");
                dgv.Columns.Add("Nombre", "Producto");
                dgv.Columns.Add("Proveedor", "Proveedor");
                dgv.Columns.Add("StockActual", "Stock Actual");
                dgv.Columns.Add("StockMin", "Stock Mínimo");
                dgv.Columns.Add("Faltantes", "Unidades Faltantes");
                dgv.Columns.Add("PrecioVenta", "Precio Venta");
                dgv.Rows.Clear();

                var criticos = _prodService.ObtenerCriticos();
                foreach (var p in criticos)
                {
                    int faltantes = p.StockMinimo - p.StockActual;
                    int r = dgv.Rows.Add(
                        p.Codigo, p.Nombre, p.Proveedor,
                        p.StockActual, p.StockMinimo,
                        faltantes, $"${p.PrecioVenta:N0}");
                    dgv.Rows[r].DefaultCellStyle.BackColor = Color.FromArgb(10, 239, 68, 68);
                    dgv.Rows[r].Cells["StockActual"].Style.ForeColor = AppColors.Danger;
                    dgv.Rows[r].Cells["StockActual"].Style.Font = AppFonts.BodyBold;
                }

                lblTitulo.Text += $"  ({criticos.Count} productos)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerarReporteMovimientos()
        {
            try
            {
                lblTitulo.Text = $"🔄  Reporte Movimientos — {DateTime.Today:MMMM yyyy}";
                dgv.Columns.Clear();
                dgv.Columns.Add("Fecha", "Fecha");
                dgv.Columns.Add("Tipo", "Tipo");
                dgv.Columns.Add("Producto", "Producto");
                dgv.Columns.Add("Cantidad", "Cantidad");
                dgv.Columns.Add("Proveedor", "Proveedor");
                dgv.Columns.Add("Usuario", "Usuario");
                dgv.Columns.Add("Obs", "Observación");
                dgv.Rows.Clear();

                var movimientos = _movService.Filtrar(
                    desde: new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                    hasta: DateTime.Today.AddDays(1));

                int entradas = 0, salidas = 0;
                foreach (var m in movimientos)
                {
                    int r = dgv.Rows.Add(
                        m.Fecha.ToString("dd/MM/yyyy HH:mm"),
                        m.TipoMovimiento,
                        m.NombreProducto,
                        m.Cantidad,
                        m.Proveedor,
                        m.NombreUsuario,
                        m.Observacion);
                    dgv.Rows[r].Cells["Tipo"].Style.ForeColor =
                        m.TipoEntradaSalida == "entrada" ? AppColors.Success : AppColors.Danger;

                    if (m.TipoEntradaSalida == "entrada") entradas += m.Cantidad;
                    else salidas += m.Cantidad;
                }

                lblTitulo.Text += $"  |  Entradas: {entradas}  |  Salidas: {salidas}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportarCSV()
        {
            if (dgv.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Sin datos",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "CSV (*.csv)|*.csv",
                    FileName = $"reporte_{DateTime.Today:yyyyMMdd}.csv"
                };
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var sb = new System.Text.StringBuilder();

                // Encabezados
                var headers = new System.Collections.Generic.List<string>();
                foreach (DataGridViewColumn col in dgv.Columns)
                    if (col.Visible) headers.Add(col.HeaderText);
                sb.AppendLine(string.Join(",", headers));

                // Filas
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow) continue;
                    var values = new System.Collections.Generic.List<string>();
                    foreach (DataGridViewCell cell in row.Cells)
                        values.Add($"\"{cell.Value}\"");
                    sb.AppendLine(string.Join(",", values));
                }

                System.IO.File.WriteAllText(sfd.FileName, sb.ToString(),
                    System.Text.Encoding.UTF8);
                MessageBox.Show("Reporte exportado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Form de Configuracion
    // ═══════════════════════════════════════════════════════════════
    public class FrmConfiguracion : Form
    {
        public FrmConfiguracion()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            var dbCard = new CardPanel { Location = new Point(24, 24), Size = new Size(560, 340) };
            var lblTitle = new Label { Text = "⚙️  Configuración de Base de Datos", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(20, 16), AutoSize = true, BackColor = Color.Transparent };
            dbCard.Controls.Add(lblTitle);
            var div = new Panel { Location = new Point(20, 48), Size = new Size(520, 1), BackColor = AppColors.Border };
            dbCard.Controls.Add(div);

            int y = 64;
            var fields = new[] {
                ("HOST",          Config.DatabaseConfig.Host),
                ("PUERTO",        Config.DatabaseConfig.Port.ToString()),
                ("BASE DE DATOS", Config.DatabaseConfig.Database),
                ("USUARIO",       Config.DatabaseConfig.Username)
            };
            var textBoxes = new TextBox[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var (label, val) = fields[i];
                var lbl = new Label { Text = label, Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(20, y), AutoSize = true, BackColor = Color.Transparent };
                textBoxes[i] = new TextBox { Location = new Point(20, y + 22), Size = new Size(520, 36), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle, Text = val };
                dbCard.Controls.Add(lbl);
                dbCard.Controls.Add(textBoxes[i]);
                y += 64;
            }

            var btnTest = UIHelper.CreateSecondaryButton("🔌  Probar Conexión", new Size(180, 40), new Point(20, y + 10));
            btnTest.Click += (s, e) =>
            {
                if (Config.DatabaseConfig.TestConnection(out string err))
                    MessageBox.Show("✅ Conexión exitosa.", "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show($"❌ {err}", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            dbCard.Controls.Add(btnTest);

            var btnGuardar = UIHelper.CreatePrimaryButton("💾  Guardar", new Size(140, 40), new Point(210, y + 10));
            btnGuardar.Click += (s, e) =>
            {
                Config.DatabaseConfig.Host = textBoxes[0].Text;
                int.TryParse(textBoxes[1].Text, out int port);
                Config.DatabaseConfig.Port = port;
                Config.DatabaseConfig.Database = textBoxes[2].Text;
                Config.DatabaseConfig.Username = textBoxes[3].Text;
                MessageBox.Show("Configuración guardada.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            dbCard.Controls.Add(btnGuardar);
            Controls.Add(dbCard);

            // Info card
            var infoCard = new CardPanel { Location = new Point(608, 24), Size = new Size(400, 200) };
            infoCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var tf = AppFonts.SubHeading;
                g.DrawString("ℹ️  Información del Sistema", tf, new SolidBrush(AppColors.TextPrimary), new Point(20, 16));
                using var dp = new Pen(AppColors.Border, 1);
                g.DrawLine(dp, 20, 48, infoCard.Width - 20, 48);
                using var bf = AppFonts.Body;
                using var bb = new SolidBrush(AppColors.TextSecondary);
                string[] info = {
                    "Versión: 1.0.0 MVP",
                    "Framework: .NET 8 Windows Forms",
                    "Base de datos: PostgreSQL",
                    $"Usuario activo: {Config.Session.UserName}",
                    $"Rol: {Config.Session.Role}"
                };
                int iy = 60;
                foreach (var line in info) { g.DrawString(line, bf, bb, new Point(20, iy)); iy += 24; }
            };
            Controls.Add(infoCard);
            ResumeLayout();
        }
    }
}