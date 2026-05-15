using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GestorInventario.Components;
using GestorInventario.Helpers;

namespace GestorInventario.Forms
{
    // ═══════════════════════════════════════════════════════════════
    // FrmUsuarios
    // ═══════════════════════════════════════════════════════════════
    public class FrmUsuarios : Form
    {
        public FrmUsuarios()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = AppColors.BackgroundGeneral;
            BuildUI();
        }

        private void BuildUI()
        {
            SuspendLayout();

            var toolbar = new Panel { Location = new Point(24, 24), Size = new Size(1100, 52), BackColor = Color.Transparent };

            var txtBuscar = new TextBox { Location = new Point(28, 8), Size = new Size(280, 36), Font = AppFonts.Body, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Buscar usuario..." };
            toolbar.Controls.Add(txtBuscar);

            var btnNuevo = UIHelper.CreatePrimaryButton("＋  Nuevo Usuario", new Size(160, 40), new Point(320, 6));
            toolbar.Controls.Add(btnNuevo);
            Controls.Add(toolbar);

            var card = new CardPanel { Location = new Point(24, 92), Size = new Size(1100, 480) };
            var dgv = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgv);
            dgv.Columns.Add("Nombre", "Nombre");
            dgv.Columns.Add("Email", "Correo");
            dgv.Columns.Add("Rol", "Rol");
            dgv.Columns.Add("Estado", "Estado");
            dgv.Columns.Add("CreadoEn", "Creado");

            // Demo user
            dgv.Rows.Add("Administrador", "admin@empresa.com", "Administrador", "Activo", DateTime.Now.AddMonths(-6).ToString("dd/MM/yyyy"));
            dgv.Rows[0].Cells["Estado"].Style.ForeColor = AppColors.Success;

            card.Controls.Add(dgv);
            Controls.Add(card);

            ResumeLayout();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // FrmConfiguracion
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

            // DB Config card
            var dbCard = new CardPanel { Location = new Point(24, 24), Size = new Size(560, 340) };

            var lblTitle = new Label { Text = "⚙️  Configuración de Base de Datos", Font = AppFonts.SubHeading, ForeColor = AppColors.TextPrimary, Location = new Point(20, 16), AutoSize = true, BackColor = Color.Transparent };
            dbCard.Controls.Add(lblTitle);
            var div = new Panel { Location = new Point(20, 48), Size = new Size(520, 1), BackColor = AppColors.Border };
            dbCard.Controls.Add(div);

            int y = 64;
            var fields = new[] { ("HOST", Config.DatabaseConfig.Host), ("PUERTO", Config.DatabaseConfig.Port.ToString()), ("BASE DE DATOS", Config.DatabaseConfig.Database), ("USUARIO", Config.DatabaseConfig.Username) };
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
            btnTest.Click += (s, e) => MessageBox.Show("⚠️ Conecta con una base de datos PostgreSQL real para probar.\n\nActualiza DatabaseConfig.cs con tus credenciales.", "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dbCard.Controls.Add(btnTest);

            var btnGuardar = UIHelper.CreatePrimaryButton("💾  Guardar", new Size(140, 40), new Point(210, y + 10));
            btnGuardar.Click += (s, e) =>
            {
                Config.DatabaseConfig.Host = textBoxes[0].Text;
                int.TryParse(textBoxes[1].Text, out int port);
                Config.DatabaseConfig.Port = port;
                Config.DatabaseConfig.Database = textBoxes[2].Text;
                Config.DatabaseConfig.Username = textBoxes[3].Text;
                MessageBox.Show("Configuración guardada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            dbCard.Controls.Add(btnGuardar);

            Controls.Add(dbCard);

            // App info card
            var infoCard = new CardPanel { Location = new Point(608, 24), Size = new Size(400, 200) };
            infoCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var tf = AppFonts.SubHeading;
                g.DrawString("ℹ️  Información del Sistema", tf, new SolidBrush(AppColors.TextPrimary), new Point(20, 16));
                using var div2 = new Pen(AppColors.Border, 1);
                g.DrawLine(div2, 20, 48, infoCard.Width - 20, 48);
                using var bf = AppFonts.Body;
                using var bb = new SolidBrush(AppColors.TextSecondary);
                string[] info = { "Versión: 1.0.0 MVP", "Framework: .NET 8 Windows Forms", "Base de datos: PostgreSQL", "Proyecto: Universidad", "Arquitectura: N-Capas" };
                int iy = 60;
                foreach (var line in info)
                {
                    g.DrawString(line, bf, bb, new Point(20, iy));
                    iy += 24;
                }
            };
            Controls.Add(infoCard);

            ResumeLayout();
        }
    }
}
