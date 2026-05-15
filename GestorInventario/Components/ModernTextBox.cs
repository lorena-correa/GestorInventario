using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GestorInventario.Helpers;

namespace GestorInventario.Components
{
    /// <summary>
    /// A labeled text input with a modern rounded border and focus highlight.
    /// </summary>
    public class ModernTextBox : UserControl
    {
        private readonly Label _label;
        private readonly TextBox _textBox;
        private bool _isFocused = false;

        public string LabelText { get => _label.Text; set => _label.Text = value; }
        public string Value { get => _textBox.Text; set => _textBox.Text = value; }
        public bool PasswordMode { set { _textBox.UseSystemPasswordChar = value; } }
        public new event EventHandler? TextChanged;

        public ModernTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(280, 68);
            BackColor = Color.Transparent;

            _label = new Label
            {
                Font = AppFonts.SmallBold,
                ForeColor = AppColors.TextSecondary,
                Location = new Point(0, 0),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            _textBox = new TextBox
            {
                Font = AppFonts.Body,
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Location = new Point(14, 36),
                Width = Width - 28,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };

            _textBox.GotFocus += (s, e) => { _isFocused = true; Invalidate(); };
            _textBox.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };
            _textBox.TextChanged += (s, e) => TextChanged?.Invoke(this, e);

            Controls.Add(_label);
            Controls.Add(_textBox);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 20, Width - 1, Height - 22);
            using var path = UIHelper.RoundedRect(rect, 8);
            using var bgBrush = new SolidBrush(Color.White);
            g.FillPath(bgBrush, path);

            Color borderColor = _isFocused ? AppColors.BorderFocus : AppColors.Border;
            float borderWidth = _isFocused ? 2f : 1f;
            using var pen = new Pen(borderColor, borderWidth);
            g.DrawPath(pen, path);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _textBox.Width = Width - 28;
        }
    }
}
