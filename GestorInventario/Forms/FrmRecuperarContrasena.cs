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
            var header = UIHelper.CreateModalHeader(this, "Recuperar Contraseña", "📧");
            Controls.Add(header);

            // Icon
            var lblIcon = new Label { Text = "🔑", Font = new Font("Segoe UI Emoji", 32f), Location = new Point(190, 75), AutoSize = true, BackColor = Color.Transparent };
            Controls.Add(lblIcon);

            var lblDesc = new Label
            {
                Text = "Ingresa tu correo electrónico y te enviaremos\ninstrucciones para restablecer tu contraseña.",
                Font = AppFonts.Body,
                ForeColor = AppColors.TextSecondary,
                Location = new Point(30, 140),
                Size = new Size(380, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            Controls.Add(lblDesc);

            int x = 30, y = 195, w = 380;
            UIHelper.CreateRoundedTextBox(this, "CORREO ELECTRÓNICO *", out txtCorreo, x, y, w);

            var btnEnviar = UIHelper.CreatePrimaryButton("Enviar instrucciones", new Size(w, 42), new Point(x, 285));
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
