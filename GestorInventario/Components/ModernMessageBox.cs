using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GestorInventario.Helpers;

namespace GestorInventario.Components
{
    public enum ModalIconType { Info, Success, Warning, Error, Question }

    /// <summary>
    /// Reemplazo de MessageBox.Show con la identidad visual morada de la aplicación
    /// (encabezado morado, iconos de acento y botones redondeados).
    /// </summary>
    public class ModernMessageBox : Form
    {
        private DialogResult _dialogResult = DialogResult.None;
        private readonly bool _isConfirm;

        private ModernMessageBox(string message, string title, ModalIconType iconType, bool isConfirm, string confirmText, string cancelText)
        {
            _isConfirm = isConfirm;
            Size = new Size(440, 232);
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.White;
            ShowInTaskbar = false;

            BuildUI(message, title, iconType, isConfirm, confirmText, cancelText);

            FormClosing += (s, e) =>
            {
                if (_dialogResult == DialogResult.None)
                    _dialogResult = _isConfirm ? DialogResult.No : DialogResult.OK;
            };
        }

        private void BuildUI(string message, string title, ModalIconType iconType, bool isConfirm, string confirmText, string cancelText)
        {
            var header = UIHelper.CreateModalHeader(this, title, HeaderEmoji(iconType));
            Controls.Add(header);
            StartPosition = FormStartPosition.CenterScreen;

            var (emoji, accent) = IconStyle(iconType);

            var iconPanel = new Panel { Location = new Point(28, 84), Size = new Size(52, 52), BackColor = Color.Transparent };
            iconPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var bg = new SolidBrush(Color.FromArgb(28, accent));
                g.FillEllipse(bg, 0, 0, 51, 51);
                using var f = new Font("Segoe UI Emoji", 20f);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(emoji, f, new SolidBrush(accent), new RectangleF(0, 0, 51, 51), sf);
            };
            Controls.Add(iconPanel);

            var lblMsg = new Label
            {
                Text = message,
                Font = AppFonts.Body,
                ForeColor = AppColors.TextPrimary,
                Location = new Point(96, 82),
                Size = new Size(316, 68),
                BackColor = Color.Transparent
            };
            Controls.Add(lblMsg);

            int btnY = Height - 76;
            if (isConfirm)
            {
                var btnCancel = UIHelper.CreateSecondaryButton(cancelText, new Size(120, 40), new Point(Width - 260, btnY));
                btnCancel.Click += (s, e) => { _dialogResult = DialogResult.No; Close(); };
                Controls.Add(btnCancel);

                var btnOk = UIHelper.CreatePrimaryButton(confirmText, new Size(120, 40), new Point(Width - 132, btnY));
                btnOk.Click += (s, e) => { _dialogResult = DialogResult.Yes; Close(); };
                Controls.Add(btnOk);
                AcceptButton = btnOk;
            }
            else
            {
                var btnOk = UIHelper.CreatePrimaryButton(confirmText, new Size(120, 40), new Point(Width - 132, btnY));
                btnOk.Click += (s, e) => { _dialogResult = DialogResult.OK; Close(); };
                Controls.Add(btnOk);
                AcceptButton = btnOk;
            }
        }

        private static string HeaderEmoji(ModalIconType t) => t switch
        {
            ModalIconType.Success => "✅",
            ModalIconType.Warning => "⚠️",
            ModalIconType.Error => "⛔",
            ModalIconType.Question => "❓",
            _ => "ℹ️"
        };

        private static (string emoji, Color color) IconStyle(ModalIconType t) => t switch
        {
            ModalIconType.Success => ("✔", AppColors.Success),
            ModalIconType.Warning => ("!", AppColors.Warning),
            ModalIconType.Error => ("✕", AppColors.Danger),
            ModalIconType.Question => ("?", AppColors.Primary),
            _ => ("i", AppColors.Info)
        };

        private static DialogResult ShowInternal(string message, string title, ModalIconType icon, bool isConfirm, string confirmText, string cancelText)
        {
            using var frm = new ModernMessageBox(message, title, icon, isConfirm, confirmText, cancelText);
            frm.ShowDialog();
            return frm._dialogResult;
        }

        public static void ShowInfo(string message, string title = "Información") =>
            ShowInternal(message, title, ModalIconType.Info, false, "Aceptar", "");

        public static void ShowSuccess(string message, string title = "Éxito") =>
            ShowInternal(message, title, ModalIconType.Success, false, "Aceptar", "");

        public static void ShowWarning(string message, string title = "Atención") =>
            ShowInternal(message, title, ModalIconType.Warning, false, "Aceptar", "");

        public static void ShowError(string message, string title = "Error") =>
            ShowInternal(message, title, ModalIconType.Error, false, "Aceptar", "");

        public static DialogResult ShowConfirm(string message, string title = "Confirmar", string confirmText = "Confirmar", string cancelText = "Cancelar") =>
            ShowInternal(message, title, ModalIconType.Question, true, confirmText, cancelText);
    }
}
