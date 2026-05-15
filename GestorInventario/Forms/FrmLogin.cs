using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;
using GestorInventario.Services;

namespace GestorInventario.Forms
{
    public class FrmLogin : Form
    {
        private TextBox txtUsuario = null!;
        private TextBox txtPassword = null!;
        private Label lblError = null!;
        private readonly AuthService _authService = new();

        public FrmLogin()
        {
            Size = new Size(980, 640);
            MinimumSize = new Size(800, 560);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;
            Text = "Gestor de Inventario";
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            // TableLayoutPanel divide el formulario en 2 columnas
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 440));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(table);

            // ── Panel izquierdo ────────────────────────────────────────
            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppColors.Sidebar };
            leftPanel.Paint += PaintLeftPanel;
            table.Controls.Add(leftPanel, 0, 0);

            // ── Panel derecho ──────────────────────────────────────────
            var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            table.Controls.Add(rightPanel, 1, 0);

            // Botón X cerrar
            var btnClose = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 14f),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Cursor = Cursors.Hand,
                Location = new Point(450, 14),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClose.Click += (s, e) => Application.Exit();
            rightPanel.Controls.Add(btnClose);

            // Bienvenida
            rightPanel.Controls.Add(new Label { Text = "¡Bienvenido!", Font = AppFonts.Title, ForeColor = AppColors.TextPrimary, Location = new Point(60, 110), AutoSize = true });
            rightPanel.Controls.Add(new Label { Text = "Ingresa tus credenciales para continuar.", Font = AppFonts.Body, ForeColor = AppColors.TextSecondary, Location = new Point(60, 155), AutoSize = true });

            // Usuario
            rightPanel.Controls.Add(new Label { Text = "USUARIO", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(60, 210), AutoSize = true });
            txtUsuario = new TextBox { Location = new Point(60, 232), Size = new Size(370, 38), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle, ForeColor = AppColors.TextPrimary, Text = "admin" };
            rightPanel.Controls.Add(txtUsuario);

            // Contraseña
            rightPanel.Controls.Add(new Label { Text = "CONTRASEÑA", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(60, 292), AutoSize = true });
            txtPassword = new TextBox { Location = new Point(60, 314), Size = new Size(370, 38), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true, ForeColor = AppColors.TextPrimary, Text = "admin" };
            rightPanel.Controls.Add(txtPassword);

            // Error
            lblError = new Label { Text = "", Font = AppFonts.Small, ForeColor = AppColors.Danger, Location = new Point(60, 365), Size = new Size(370, 20) };
            rightPanel.Controls.Add(lblError);

            // Botón ingresar
            var btnIngresar = UIHelper.CreatePrimaryButton("Iniciar Sesión", new Size(370, 48), new Point(60, 388));
            btnIngresar.Click += BtnIngresar_Click;
            rightPanel.Controls.Add(btnIngresar);

            // Recuperar contraseña
            var lnkRecuperar = new LinkLabel { Text = "¿Olvidaste tu contraseña?", Font = AppFonts.Small, Location = new Point(60, 452), AutoSize = true, LinkColor = AppColors.Primary };
            lnkRecuperar.LinkClicked += (s, e) => new FrmRecuperarContrasena().ShowDialog(this);
            rightPanel.Controls.Add(lnkRecuperar);

            // Hint
            rightPanel.Controls.Add(new Label { Text = "Demo: usuario=admin / contraseña=admin", Font = AppFonts.Small, ForeColor = Color.FromArgb(150, 107, 114, 128), Location = new Point(60, 476), AutoSize = true });

            // Enter key
            txtUsuario.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPassword.Focus(); };
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnIngresar_Click(s, e); };

            ResumeLayout();
        }

        private void PaintLeftPanel(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var p = (Panel)sender!;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var grad = new LinearGradientBrush(new Point(0, 0), new Point(p.Width, p.Height), AppColors.Sidebar, ColorTranslator.FromHtml("#0F172A"));
            g.FillRectangle(grad, 0, 0, p.Width, p.Height);

            using var c1 = new SolidBrush(Color.FromArgb(25, 123, 97, 255));
            g.FillEllipse(c1, -80, -80, 300, 300);
            using var c2 = new SolidBrush(Color.FromArgb(15, 123, 97, 255));
            g.FillEllipse(c2, p.Width - 150, p.Height - 150, 280, 280);

            int cx = p.Width / 2 - 40;
            using var lb = new SolidBrush(AppColors.Primary);
            g.FillEllipse(lb, cx, 100, 80, 80);
            using var lf = new Font("Segoe UI", 32f, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("G", lf, Brushes.White, new RectangleF(cx, 100, 80, 80), sf);

            var csf = new StringFormat { Alignment = StringAlignment.Center };
            using var tf = new Font("Segoe UI", 20f, FontStyle.Bold);
            g.DrawString("GestorInventario", tf, Brushes.White, new RectangleF(0, 198, p.Width, 34), csf);
            using var sf2 = new Font("Segoe UI", 10f);
            using var sb2 = new SolidBrush(Color.FromArgb(160, 255, 255, 255));
            g.DrawString("Sistema de Control de Inventario", sf2, sb2, new RectangleF(0, 236, p.Width, 24), csf);

            string[] features = { "✓  Gestión de productos", "✓  Control de stock", "✓  Proveedores y movimientos", "✓  Reportes y alertas" };
            int fy = 310;
            using var ff = new Font("Segoe UI", 10f);
            using var fb = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            foreach (var feat in features) { g.DrawString(feat, ff, fb, new RectangleF(60, fy, p.Width - 120, 28)); fy += 34; }

            using var vf = new Font("Segoe UI", 8f);
            using var vb = new SolidBrush(Color.FromArgb(80, 255, 255, 255));
            g.DrawString("v1.0.0 · MVP Universitario", vf, vb, new RectangleF(0, p.Height - 34, p.Width, 20), csf);
        }

        private void BtnIngresar_Click(object? sender, EventArgs e)
        {
            string user = txtUsuario.Text.Trim();
            string pass = txtPassword.Text;
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) { lblError.Text = "Por favor ingresa usuario y contraseña."; return; }
            if (_authService.Login(user, pass)) { var main = new FrmMain(); main.Show(); Hide(); }
            else { lblError.Text = "Usuario o contraseña incorrectos."; txtPassword.Clear(); txtPassword.Focus(); }
        }
    }
}
