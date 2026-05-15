using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GestorInventario.Helpers;

namespace GestorInventario.Components
{
    public class RoundedButton : Button
    {
        private Color _buttonColor = AppColors.Primary;
        private Color _hoverColor = AppColors.PrimaryHover;
        private Color _textColor = Color.White;
        private Color _borderColor = Color.Transparent;
        private int _radius = 8;
        private bool _isHovered = false;
        private bool _isPressed = false;

        public Color ButtonColor { get => _buttonColor; set { _buttonColor = value; Invalidate(); } }
        public Color HoverColor { get => _hoverColor; set { _hoverColor = value; Invalidate(); } }
        public Color TextColor { get => _textColor; set { _textColor = value; Invalidate(); } }
        public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        public int Radius { get => _radius; set { _radius = value; Invalidate(); } }

        public RoundedButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; _isPressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _isPressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _isPressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(1, 1, Width - 2, Height - 2);
            Color bg = _isPressed ? Color.FromArgb(200, _isHovered ? _hoverColor : _buttonColor)
                      : _isHovered ? _hoverColor : _buttonColor;

            using var path = UIHelper.RoundedRect(rect, _radius);
            using var brush = new SolidBrush(bg);
            g.FillPath(brush, path);

            if (_borderColor != Color.Transparent)
            {
                using var pen = new Pen(_borderColor, 1.5f);
                g.DrawPath(pen, path);
            }

            // Text
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var textBrush = new SolidBrush(_textColor);
            g.DrawString(Text, Font, textBrush, new RectangleF(0, 0, Width, Height), sf);
        }
    }
}
