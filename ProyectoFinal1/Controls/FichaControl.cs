using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LoteriaGame.Controls
{
    public class FichaControl : Label
    {
        private bool _arrastrandose;
        private Point _puntoInicio;

        public FichaControl()
        {
            Size = new Size(44, 44);
            Cursor = Cursors.Hand;
            DoubleBuffered = true;

            MouseDown += FichaControl_MouseDown;
            MouseMove += FichaControl_MouseMove;
            MouseUp += (s, e) => _arrastrandose = false;
        }

        private void FichaControl_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _arrastrandose = true;
                _puntoInicio = e.Location;
            }
        }

        private void FichaControl_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_arrastrandose) return;
            if (Math.Abs(e.X - _puntoInicio.X) > 5 || Math.Abs(e.Y - _puntoInicio.Y) > 5)
            {
                _arrastrandose = false;
                DoDragDrop("FICHA", DragDropEffects.Copy);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int d = Math.Min(Width, Height) - 4;
            int x = (Width - d) / 2;
            int y = (Height - d) / 2;

            using var shadow = new SolidBrush(Color.FromArgb(60, 0, 0, 0));
            g.FillEllipse(shadow, x + 2, y + 3, d, d);

            using var grad = new LinearGradientBrush(
                new Rectangle(x, y, d, d),
                Color.FromArgb(180, 110, 50),
                Color.FromArgb(100, 55, 15),
                LinearGradientMode.ForwardDiagonal);
            g.FillEllipse(grad, x, y, d, d);

            using var brillo = new SolidBrush(Color.FromArgb(80, 255, 255, 200));
            g.FillEllipse(brillo, x + d / 5, y + d / 6, d / 3, d / 4);

            using var borde = new Pen(Color.FromArgb(70, 35, 10), 1.5f);
            g.DrawEllipse(borde, x, y, d, d);
        }
    }
}
