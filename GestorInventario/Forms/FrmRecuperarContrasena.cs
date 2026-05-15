using System;
using System.Drawing;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;

namespace GestorInventario.Forms
{
    public class FrmRecuperarContrasena : Form
    {
        private TextBox txtCorreo = null!;

        public FrmRecuperarContrasena()
        {
            Size = new Size(440, 380);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BuildUI();
        }

        private void BuildUI()
        {
            // Header bar
            var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = AppColors.Primary };
            var lblTitle = new Label { Text = "Recuperar Contraseña", Font = AppFonts.SubHeading, ForeColor = Color.White, Location = new Point(20, 16), AutoSize = true };
            var btnClose = new Label { Text = "✕", Font = new Font("Segoe UI", 14f), ForeColor = Color.White, Cursor = Cursors.Hand, Location = new Point(395, 14), AutoSize = true };
            btnClose.Click += (s, e) => Close();
            header.Controls.Add(lblTitle);
            header.Controls.Add(btnClose);
            Controls.Add(header);

            // Icon
            var lblIcon = new Label { Text = "📧", Font = new Font("Segoe UI Emoji", 40f), Location = new Point(170, 80), AutoSize = true };
            Controls.Add(lblIcon);

            var lblDesc = new Label
            {
                Text = "Ingresa tu correo electrónico y te enviaremos\ninstrucciones para restablecer tu contraseña.",
                Font = AppFonts.Body,
                ForeColor = AppColors.TextSecondary,
                Location = new Point(30, 166),
                Size = new Size(380, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblDesc);

            var lblEmail = new Label { Text = "CORREO ELECTRÓNICO", Font = AppFonts.SmallBold, ForeColor = AppColors.TextSecondary, Location = new Point(30, 218), AutoSize = true };
            Controls.Add(lblEmail);

            txtCorreo = new TextBox
            {
                Location = new Point(30, 238),
                Size = new Size(380, 36),
                Font = AppFonts.Body,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "correo@empresa.com"
            };
            Controls.Add(txtCorreo);

            var btnEnviar = UIHelper.CreatePrimaryButton("Enviar instrucciones", new Size(380, 44), new Point(30, 290));
            btnEnviar.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtCorreo.Text))
                {
                    MessageBox.Show("Ingresa tu correo electrónico.", "Campo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                MessageBox.Show($"Se han enviado instrucciones a:\n{txtCorreo.Text}", "Correo enviado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            };
            Controls.Add(btnEnviar);
        }
    }
}
