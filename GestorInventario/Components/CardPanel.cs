using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GestorInventario.Helpers;

namespace GestorInventario.Components
{
    public class CardPanel : Panel
    {
        private int _radius = 12;
        private Color _shadowColor = Color.FromArgb(30, 0, 0, 0);
        private bool _showShadow = true;

        public int Radius { get => _radius; set { _radius = value; Invalidate(); } }
        public bool ShowShadow { get => _showShadow; set { _showShadow = value; Invalidate(); } }

        public CardPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Padding = new Padding(16);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(2, 2, Width - 5, Height - 5);

            // Shadow
            if (_showShadow)
            {
                for (int i = 4; i >= 1; i--)
                {
                    var sRect = new Rectangle(rect.X - 1, rect.Y + i, rect.Width + 2, rect.Height);
                    using var sPath = UIHelper.RoundedRect(sRect, _radius);
                    using var sBrush = new SolidBrush(Color.FromArgb(8 * i, 100, 100, 120));
                    g.FillPath(sBrush, sPath);
                }
            }

            // Card background
            using var path = UIHelper.RoundedRect(rect, _radius);
            using var brush = new SolidBrush(BackColor);
            g.FillPath(brush, path);

            // Border
            using var pen = new Pen(AppColors.Border, 1f);
            g.DrawPath(pen, path);

            // Paint children
            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }
    }
}
