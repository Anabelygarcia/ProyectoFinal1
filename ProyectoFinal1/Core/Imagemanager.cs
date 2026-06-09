using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using ProyectoFinal1.Models;

namespace ProyectoFinal1.Core
{
    public static class ImageManager
    {
        private static readonly Dictionary<string, Image> _cache = new();

        private static Image? _reversoCache;

        private static readonly Color[] _fondos =
        {
            Color.FromArgb(180, 30, 30),   // rojo
            Color.FromArgb(20, 90, 180),   // azul
            Color.FromArgb(20, 110, 40),   // verde
            Color.FromArgb(200, 100, 10),  // naranja
            Color.FromArgb(100, 20, 150),  // morado
            Color.FromArgb(10, 110, 110),  // verde azulado
            Color.FromArgb(150, 20, 90),   // rosa fuerte
            Color.FromArgb(80, 55, 20),    // café
            Color.FromArgb(30, 80, 140),   // azul marino
            Color.FromArgb(160, 60, 10),   // naranja oscuro
        };

        public static Image ObtenerImagen(Carta carta, int ancho = 100, int alto = 130)
        {
            string clave = $"{carta.Id}_{ancho}_{alto}";
            if (_cache.TryGetValue(clave, out var cached)) return cached;

            Image? img = CargarImagenReal(carta.ImagenPath, ancho, alto);

            img ??= GenerarPlaceholder(carta, ancho, alto);

            _cache[clave] = img;
            return img;
        }

        public static Image ObtenerReverso(int ancho = 100, int alto = 130)
        {
            string clave = $"reverso_{ancho}_{alto}";
            if (_cache.TryGetValue(clave, out var cached)) return cached;

            var img = GenerarReverso(ancho, alto);
            _cache[clave] = img;
            return img;
        }

        public static void LimpiarCache()
        {
            foreach (var img in _cache.Values) img.Dispose();
            _cache.Clear();
        }


        private static Image? CargarImagenReal(string nombreArchivo, int ancho, int alto)
        {
            Image? original = CargarDesdeRecurso(nombreArchivo)
                           ?? CargarDesdeArchivoExterno(nombreArchivo);

            if (original == null) return null;

            try
            {
                var bmp = new Bitmap(ancho, alto);
                using var g = Graphics.FromImage(bmp);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(original, 0, 0, ancho, alto);
                original.Dispose();
                return bmp;
            }
            catch { original.Dispose(); return null; }
        }

        private static Image? CargarDesdeRecurso(string nombreArchivo)
        {
            try
            {
                string nombre = $"ProyectoFinal1.Resources.Images.{nombreArchivo}";
                var asm = Assembly.GetExecutingAssembly();
                using var stream = asm.GetManifestResourceStream(nombre);
                if (stream == null) return null;
                var ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                return Image.FromStream(ms);
            }
            catch { return null; }
        }

        private static Image? CargarDesdeArchivoExterno(string nombreArchivo)
        {
            try
            {
                string carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                string ruta = Path.Combine(carpeta, nombreArchivo);
                if (!File.Exists(ruta)) return null;
                return Image.FromFile(ruta);
            }
            catch { return null; }
        }

        public static Image GenerarPlaceholder(Carta carta, int ancho = 100, int alto = 130)
        {
            var bmp = new Bitmap(ancho, alto, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.Clear(Color.Transparent);

            Color fondoColor = _fondos[(carta.Id - 1) % _fondos.Length];

            using var gradFondo = new LinearGradientBrush(
                new Rectangle(0, 0, ancho, alto),
                fondoColor,
                ControlPaint.Dark(fondoColor, 0.25f),
                LinearGradientMode.ForwardDiagonal);
            FillRoundRect(g, gradFondo, 0, 0, ancho - 1, alto - 1, 12);

            using var penOro = new Pen(Color.FromArgb(255, 215, 0), 2.5f);
            DrawRoundRect(g, penOro, 1, 1, ancho - 3, alto - 3, 11);
            
            using var penOroFino = new Pen(Color.FromArgb(180, 255, 215, 0), 1f);
            DrawRoundRect(g, penOroFino, 5, 5, ancho - 11, alto - 11, 8);

            using var fNum = new Font("Arial", Math.Max(7f, ancho * 0.09f), FontStyle.Bold);
            using var bNum = new SolidBrush(Color.FromArgb(255, 255, 200));
            g.DrawString($"#{carta.Id}", fNum, bNum, new PointF(6, 4));

            float tamEmoji = Math.Max(16f, ancho * 0.30f);
            using var fEmoji = new Font("Segoe UI Emoji", tamEmoji);
            string emoji = carta.Emoji;
            var szEmoji = g.MeasureString(emoji, fEmoji);
            float exEmoji = (ancho - szEmoji.Width) / 2f;
            float eyEmoji = (alto * 0.35f) - szEmoji.Height / 2f;
            g.DrawString(emoji, fEmoji, bNum, new PointF(exEmoji, eyEmoji));

            float altNombre = alto * 0.28f;
            float yNombre = alto - altNombre;
            using var brNombre = new SolidBrush(Color.FromArgb(130, 0, 0, 0));
            using var pathNombre = BuildRoundRectPath(6, yNombre, ancho - 13, altNombre - 5, 7);
            g.FillPath(brNombre, pathNombre);

            float tamFuente = Math.Max(6f, ancho * 0.075f);
            using var fNombre = new Font("Arial", tamFuente, FontStyle.Bold);
            var sfCenter = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisWord
            };
            var rectNombre = new RectangleF(4, yNombre, ancho - 8, altNombre - 4);
            g.DrawString(carta.Nombre, fNombre, bNum, rectNombre, sfCenter);

            return bmp;
        }

        public static Image GenerarReverso(int ancho = 100, int alto = 130)
        {
            var bmp = new Bitmap(ancho, alto, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.Clear(Color.Transparent);

            using var grad = new LinearGradientBrush(
                new Rectangle(0, 0, ancho, alto),
                Color.FromArgb(15, 55, 15),
                Color.FromArgb(5, 30, 5),
                LinearGradientMode.ForwardDiagonal);
            FillRoundRect(g, grad, 0, 0, ancho - 1, alto - 1, 12);

            using var penOro = new Pen(Color.FromArgb(255, 215, 0), 2.5f);
            DrawRoundRect(g, penOro, 1, 1, ancho - 3, alto - 3, 11);

            using var penOroFino = new Pen(Color.FromArgb(150, 255, 215, 0), 1f);
            DrawRoundRect(g, penOroFino, 5, 5, ancho - 11, alto - 11, 8);

            using var penPat = new Pen(Color.FromArgb(30, 255, 215, 0), 0.8f);
            for (int y = 10; y < alto - 10; y += 12)
                for (int x = 10; x < ancho - 10; x += 12)
                    g.DrawRectangle(penPat, x, y, 6, 6);

            float tamEmoji = Math.Max(18f, ancho * 0.32f);
            using var fEmoji = new Font("Segoe UI Emoji", tamEmoji);
            using var bOro = new SolidBrush(Color.FromArgb(255, 215, 0));
            string emoji = "🌵";
            var sz = g.MeasureString(emoji, fEmoji);
            g.DrawString(emoji, fEmoji, bOro,
                new PointF((ancho - sz.Width) / 2f, (alto - sz.Height) / 2f));

            return bmp;
        }

        public static void FillRoundRect(Graphics g, Brush brush,
            float x, float y, float w, float h, float r)
        {
            using var path = BuildRoundRectPath(x, y, w, h, r);
            g.FillPath(brush, path);
        }

        public static void DrawRoundRect(Graphics g, Pen pen,
            float x, float y, float w, float h, float r)
        {
            using var path = BuildRoundRectPath(x, y, w, h, r);
            g.DrawPath(pen, path);
        }

        public static GraphicsPath BuildRoundRectPath(float x, float y, float w, float h, float r)
        {
            var path = new GraphicsPath();
            float d = r * 2;
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}