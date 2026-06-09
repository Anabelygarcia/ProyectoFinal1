using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProyectoFinal1.Core;
using ProyectoFinal1.Models;

namespace ProyectoFinal1.Controls
{
    // ════════════════════════════════════════════════════════════════════════
    // EVENTO FICHA MARCADA
    // ════════════════════════════════════════════════════════════════════════

    public class CartaEventArgs : EventArgs
    {
        public int Fila { get; }
        public int Columna { get; }
        public Carta Carta { get; }

        public CartaEventArgs(int fila, int columna, Carta carta)
        {
            Fila = fila;
            Columna = columna;
            Carta = carta;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // CARTA CONTROL
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Control visual de una carta en el tablero del jugador.
    ///
    /// Funcionalidades:
    ///   - Muestra la imagen de la carta (real o placeholder)
    ///   - Actúa como zona de destino para Drag & Drop de fichas
    ///   - Zona segura: el drop solo cuenta si cae dentro de los límites del control
    ///   - Feedback visual verde al entrar una ficha, rojo si cae fuera
    ///   - Animación de marcado (flash dorado)
    ///   - Resaltado especial para las celdas ganadoras
    ///   - Tooltip con grito de la carta
    /// </summary>
    public class CartaControl : Control
    {
        // ── Estado ───────────────────────────────────────────────────────────
        private Carta? _carta;
        private bool _marcada;
        private bool _ganadora;          // celda que forma parte de la victoria
        private bool _enZonaDeteccion;   // ficha sobrevolando esta celda
        private bool _hover;
        private Color _flashColor = Color.Transparent;
        private bool _enFlash;

        // ── Propiedades públicas ─────────────────────────────────────────────
        public int Fila { get; set; }
        public int Columna { get; set; }

        public Carta? Carta
        {
            get => _carta;
            set { _carta = value; ActualizarTooltip(); Invalidate(); }
        }

        public bool Marcada
        {
            get => _marcada;
            set { _marcada = value; Invalidate(); }
        }

        public bool Ganadora
        {
            get => _ganadora;
            set { _ganadora = value; Invalidate(); }
        }

        // ── Colores ──────────────────────────────────────────────────────────
        private static readonly Color COLOR_BORDE_NORMAL = Color.FromArgb(139, 90, 43);
        private static readonly Color COLOR_BORDE_MARCADA = Color.FromArgb(200, 20, 20);
        private static readonly Color COLOR_BORDE_GANADORA = Color.FromArgb(255, 215, 0);
        private static readonly Color COLOR_OVERLAY_MARCADA = Color.FromArgb(170, 255, 215, 0);
        private static readonly Color COLOR_ZONA_OK = Color.FromArgb(80, 0, 200, 0);
        private static readonly Color COLOR_ZONA_BORDE_OK = Color.LimeGreen;
        private static readonly Color COLOR_HOVER = Color.FromArgb(40, 255, 255, 150);
        private static readonly Color COLOR_FLASH_MARCADA = Color.FromArgb(200, 255, 215, 0);
        private static readonly Color COLOR_FLASH_ERROR = Color.FromArgb(200, 255, 60, 60);

        // ── Evento ───────────────────────────────────────────────────────────
        /// <summary>Se dispara cuando el jugador marca correctamente esta celda con una ficha.</summary>
        public event EventHandler<CartaEventArgs>? FichaMarcada;

        // ── Tooltip ──────────────────────────────────────────────────────────
        private readonly ToolTip _tooltip = new ToolTip
        {
            AutoPopDelay = 8000,
            InitialDelay = 600,
            ReshowDelay = 200
        };

        // ── Constructor ──────────────────────────────────────────────────────
        public CartaControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Size = new Size(115, 145);
            AllowDrop = true;
            Cursor = Cursors.Default;

            // ── Eventos de Drag & Drop ──────────────────────────────────────
            DragEnter += OnDragEnterCarta;
            DragOver += OnDragOverCarta;
            DragLeave += OnDragLeaveCarta;
            DragDrop += OnDragDropCarta;

            // ── Mouse hover ─────────────────────────────────────────────────
            MouseEnter += (s, e) => { _hover = true; Invalidate(); };
            MouseLeave += (s, e) => { _hover = false; Invalidate(); };
        }

        // ── Drag & Drop ──────────────────────────────────────────────────────

        private void OnDragEnterCarta(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(typeof(string)) == true && !_marcada && _carta != null)
            {
                _enZonaDeteccion = true;
                e.Effect = DragDropEffects.Copy;
                Invalidate();
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void OnDragOverCarta(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(typeof(string)) == true && !_marcada && _carta != null)
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void OnDragLeaveCarta(object? sender, EventArgs e)
        {
            _enZonaDeteccion = false;
            Invalidate();
        }

        private void OnDragDropCarta(object? sender, DragEventArgs e)
        {
            _enZonaDeteccion = false;

            if (_carta == null || _marcada)
            {
                Invalidate();
                return;
            }

            // ── Verificar zona segura ──────────────────────────────────────
            // El drop debe caer dentro del área interior del control (excluyendo borde de 4px)
            var posicionRelativa = PointToClient(new Point(e.X, e.Y));
            var zonaSeg = new Rectangle(4, 4, Width - 8, Height - 8);

            if (!zonaSeg.Contains(posicionRelativa))
            {
                // Fuera de zona segura → NO cuenta
                _ = AnimarFlash(COLOR_FLASH_ERROR);
                AudioManager.ReproducirError();
                Invalidate();
                return;
            }

            // ── Drop válido: marcar ────────────────────────────────────────
            _marcada = true;
            FichaMarcada?.Invoke(this, new CartaEventArgs(Fila, Columna, _carta));
            AudioManager.ReproducirFicha();
            _ = AnimarFlash(COLOR_FLASH_MARCADA);
        }

        // ── Animación de flash ────────────────────────────────────────────────
        private async Task AnimarFlash(Color color)
        {
            if (_enFlash) return;
            _enFlash = true;
            for (int i = 0; i < 3; i++)
            {
                _flashColor = color;
                Invalidate();
                await Task.Delay(75);
                _flashColor = Color.Transparent;
                Invalidate();
                await Task.Delay(75);
            }
            _enFlash = false;
            Invalidate();
        }

        /// <summary>Dispara la animación de "celda ganadora" desde el exterior.</summary>
        public void AnimarVictoria()
        {
            _ganadora = true;
            _ = AnimarFlash(Color.FromArgb(200, 255, 215, 0));
        }

        // ── Tooltip ──────────────────────────────────────────────────────────
        private void ActualizarTooltip()
        {
            if (_carta != null)
                _tooltip.SetToolTip(this,
                    $"{_carta.Nombre}\n«{_carta.Grito}»");
            else
                _tooltip.SetToolTip(this, "(celda vacía)");
        }

        // ── Pintado ──────────────────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            var rect = new RectangleF(2, 2, Width - 4, Height - 4);
            float radio = 10f;

            if (_carta == null)
            {
                // ── Celda vacía ──────────────────────────────────────────────
                using var brVacio = new SolidBrush(Color.FromArgb(190, 235, 220, 190));
                ImageManager.FillRoundRect(g, brVacio, rect.X, rect.Y, rect.Width, rect.Height, radio);
                using var penVacio = new Pen(Color.FromArgb(160, 180, 150, 100), 1.5f)
                { DashStyle = DashStyle.Dash };
                ImageManager.DrawRoundRect(g, penVacio, rect.X, rect.Y, rect.Width, rect.Height, radio);

                using var fVacio = new Font("Segoe UI", 8f);
                using var sfC = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString($"({Fila + 1},{Columna + 1})", fVacio,
                    Brushes.Gray, new RectangleF(0, 0, Width, Height), sfC);
                return;
            }

            // ── Imagen de la carta ───────────────────────────────────────────
            var img = ImageManager.ObtenerImagen(_carta, (int)rect.Width, (int)rect.Height);

            // Recorte redondeado para la imagen
            using var clipPath = ImageManager.BuildRoundRectPath(rect.X, rect.Y, rect.Width, rect.Height, radio);
            g.SetClip(clipPath);
            g.DrawImage(img, rect.X, rect.Y, rect.Width, rect.Height);
            g.ResetClip();

            // ── Overlay marcada (ficha de oro) ───────────────────────────────
            if (_marcada)
            {
                using var ovMarcada = new SolidBrush(COLOR_OVERLAY_MARCADA);
                ImageManager.FillRoundRect(g, ovMarcada, rect.X, rect.Y, rect.Width, rect.Height, radio);

                // Símbolo de ficha centrado
                using var fFicha = new Font("Segoe UI Emoji", Math.Max(22f, Width * 0.22f));
                using var sfC = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("🪙", fFicha, Brushes.SaddleBrown,
                    new RectangleF(0, 0, Width, Height), sfC);
            }

            // ── Overlay ganadora (brillo dorado pulsante simulado) ───────────
            if (_ganadora)
            {
                using var ovGanadora = new SolidBrush(Color.FromArgb(120, 255, 215, 0));
                ImageManager.FillRoundRect(g, ovGanadora, rect.X, rect.Y, rect.Width, rect.Height, radio);
            }

            // ── Overlay zona de detección activa ─────────────────────────────
            if (_enZonaDeteccion)
            {
                using var ovDetec = new SolidBrush(COLOR_ZONA_OK);
                ImageManager.FillRoundRect(g, ovDetec, rect.X, rect.Y, rect.Width, rect.Height, radio);
                using var penDetec = new Pen(COLOR_ZONA_BORDE_OK, 3f);
                ImageManager.DrawRoundRect(g, penDetec, rect.X, rect.Y, rect.Width, rect.Height, radio);
            }

            // ── Overlay hover ────────────────────────────────────────────────
            if (_hover && !_marcada)
            {
                using var ovHover = new SolidBrush(COLOR_HOVER);
                ImageManager.FillRoundRect(g, ovHover, rect.X, rect.Y, rect.Width, rect.Height, radio);
            }

            // ── Flash de animación ───────────────────────────────────────────
            if (_flashColor.A > 0)
            {
                using var ovFlash = new SolidBrush(_flashColor);
                ImageManager.FillRoundRect(g, ovFlash, rect.X, rect.Y, rect.Width, rect.Height, radio);
            }

            // ── Borde exterior ───────────────────────────────────────────────
            Color bordeColor = _ganadora ? COLOR_BORDE_GANADORA
                             : _marcada ? COLOR_BORDE_MARCADA
                                         : COLOR_BORDE_NORMAL;
            float bordeAncho = _ganadora ? 3f : _marcada ? 2.5f : 1.5f;

            using var penBorde = new Pen(bordeColor, bordeAncho);
            ImageManager.DrawRoundRect(g, penBorde, rect.X, rect.Y, rect.Width, rect.Height, radio);
        }
    }
}