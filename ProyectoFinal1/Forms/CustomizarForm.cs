using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ProyectoFinal1.Core;
using ProyectoFinal1.Models;

namespace ProyectoFinal1.Forms
{
    public class CustomizarForm : Form
    {
        private readonly GameEngine _engine;
        private Carta?[,] _seleccion = new Carta[Tablero.FILAS, Tablero.COLUMNAS];

        private Panel[,] _celdas = new Panel[Tablero.FILAS, Tablero.COLUMNAS];
        private FlowLayoutPanel flpCartas;
        private Label lblInfo;
        private int _filaSeleccionada = -1;
        private int _colSeleccionada = -1;

        public CustomizarForm(GameEngine engine)
        {
            _engine = engine;

            // Cargar seleccion previa si existe
            if (_engine.JugadorLocal != null)
            {
                var t = _engine.JugadorLocal.Tablero;
                for (int f = 0; f < Tablero.FILAS; f++)
                    for (int c = 0; c < Tablero.COLUMNAS; c++)
                        _seleccion[f, c] = t.Cartas[f, c];
            }

            InitializeComponent();
            CargarTodasLasCartas();
        }

        private void InitializeComponent()
        {
            this.Text = "Personalizar Tablero";
            this.Size = new Size(1050, 680);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Info
            lblInfo = new Label();
            lblInfo.Text = "1. Haz clic en una celda del tablero   2. Haz clic en la carta que quieres";
            lblInfo.Location = new Point(10, 8);
            lblInfo.Size = new Size(700, 20);

            // Botones
            var btnAleatorio = new Button();
            btnAleatorio.Text = "Generar Aleatorio";
            btnAleatorio.Location = new Point(720, 5);
            btnAleatorio.Size = new Size(130, 28);
           // btnAleatorio.Click += (s, e) => GenerarAleatorio();

            var btnLimpiar = new Button();
            btnLimpiar.Text = "Limpiar";
            btnLimpiar.Location = new Point(860, 5);
            btnLimpiar.Size = new Size(80, 28);
            btnLimpiar.Click += (s, e) => Limpiar();

            var btnGuardar = new Button();
            btnGuardar.Text = "Guardar";
            btnGuardar.Location = new Point(950, 5);
            btnGuardar.Size = new Size(80, 28);
            btnGuardar.BackColor = Color.Green;
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Click += BtnGuardar_Click;

            // Panel del tablero (izquierda)
            var pnlTablero = new Panel();
            pnlTablero.Location = new Point(10, 35);
            pnlTablero.Size = new Size(500, 610);
            pnlTablero.BorderStyle = BorderStyle.FixedSingle;

            var lblTablero = new Label();
            lblTablero.Text = "TU TABLERO (4x4)";
            lblTablero.Font = new Font("Arial", 11, FontStyle.Bold);
            lblTablero.Location = new Point(150, 5);
            lblTablero.Size = new Size(200, 22);
            pnlTablero.Controls.Add(lblTablero);

            // Celdas del tablero
            int tam = 115, gap = 5, ox = 8, oy = 32;
            for (int f = 0; f < Tablero.FILAS; f++)
            {
                for (int c = 0; c < Tablero.COLUMNAS; c++)
                {
                    int ff = f, cc = c;
                    var celda = new Panel();
                    celda.Location = new Point(ox + c * (tam + gap), oy + f * (tam + gap));
                    celda.Size = new Size(tam, tam + 10);
                    celda.BorderStyle = BorderStyle.FixedSingle;
                    celda.BackColor = Color.LightGray;
                    celda.Cursor = Cursors.Hand;
                    celda.Click += (s, e) => SeleccionarCelda(ff, cc);

                    _celdas[f, c] = celda;
                    pnlTablero.Controls.Add(celda);
                    DibujarCelda(f, c);
                }
            }

            // Panel de cartas (derecha)
            var pnlCartas = new Panel();
            pnlCartas.Location = new Point(520, 35);
            pnlCartas.Size = new Size(510, 610);
            pnlCartas.BorderStyle = BorderStyle.FixedSingle;

            var lblCartas = new Label();
            lblCartas.Text = "TODAS LAS CARTAS";
            lblCartas.Font = new Font("Arial", 11, FontStyle.Bold);
            lblCartas.Location = new Point(150, 5);
            lblCartas.Size = new Size(200, 22);
            pnlCartas.Controls.Add(lblCartas);

            var txtBuscar = new TextBox();
            txtBuscar.Location = new Point(5, 30);
            txtBuscar.Size = new Size(498, 22);
            txtBuscar.PlaceholderText = "Buscar carta...";
            txtBuscar.TextChanged += (s, e) => FiltrarCartas(txtBuscar.Text);
            pnlCartas.Controls.Add(txtBuscar);

            flpCartas = new FlowLayoutPanel();
            flpCartas.Location = new Point(3, 58);
            flpCartas.Size = new Size(504, 545);
            flpCartas.AutoScroll = true;
            flpCartas.FlowDirection = FlowDirection.LeftToRight;
            flpCartas.WrapContents = true;
            pnlCartas.Controls.Add(flpCartas);

            this.Controls.AddRange(new Control[]
            {
                lblInfo, btnAleatorio, btnLimpiar, btnGuardar,
                pnlTablero, pnlCartas
            });
        }

        private void CargarTodasLasCartas(string filtro = "")
        {
            flpCartas.Controls.Clear();
            foreach (var carta in _engine.Baraja.TodasLasCartas)
            {
                if (!string.IsNullOrEmpty(filtro) &&
                    !carta.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    continue;

                bool usada = EstaUsada(carta);

                var panel = new Panel();
                panel.Size = new Size(76, 100);
                panel.BackColor = usada ? Color.LightGreen : Color.WhiteSmoke;
                panel.BorderStyle = BorderStyle.FixedSingle;
                panel.Cursor = Cursors.Hand;
                panel.Tag = carta;

                var pic = new PictureBox();
                pic.Size = new Size(70, 78);
                pic.Location = new Point(3, 2);
                pic.Image = ImageManager.ObtenerImagen(carta, 70, 78);
                pic.SizeMode = PictureBoxSizeMode.Zoom;
                pic.Tag = carta;

                var lbl = new Label();
                lbl.Text = carta.Nombre;
                lbl.Font = new Font("Arial", 6.5f);
                lbl.Location = new Point(0, 80);
                lbl.Size = new Size(76, 18);
                lbl.TextAlign = ContentAlignment.MiddleCenter;

                panel.Controls.Add(pic);
                panel.Controls.Add(lbl);
                panel.Click += (s, e) => AsignarCarta(carta);
                pic.Click += (s, e) => AsignarCarta(carta);

                flpCartas.Controls.Add(panel);
            }
        }

        private void FiltrarCartas(string texto)
        {
            CargarTodasLasCartas(texto);
        }

        private void SeleccionarCelda(int f, int c)
        {
            // Deseleccionar anterior
            if (_filaSeleccionada >= 0)
                _celdas[_filaSeleccionada, _colSeleccionada].BackColor = Color.LightGray;

            _filaSeleccionada = f;
            _colSeleccionada = c;
            _celdas[f, c].BackColor = Color.Yellow;
            lblInfo.Text = "Celda (" + (f + 1) + "," + (c + 1) + ") seleccionada. Ahora elige una carta.";
        }

        private void AsignarCarta(Carta carta)
        {
            if (_filaSeleccionada < 0)
            {
                lblInfo.Text = "Primero haz clic en una celda del tablero.";
                return;
            }

            // Quitar la carta si ya estaba en otra celda
            for (int f = 0; f < Tablero.FILAS; f++)
                for (int c = 0; c < Tablero.COLUMNAS; c++)
                    if (_seleccion[f, c]?.Id == carta.Id)
                        _seleccion[f, c] = null;

            _seleccion[_filaSeleccionada, _colSeleccionada] = carta;
            _celdas[_filaSeleccionada, _colSeleccionada].BackColor = Color.LightGray;

            DibujarCelda(_filaSeleccionada, _colSeleccionada);

            lblInfo.Text = "'" + carta.Nombre + "' asignada a (" +
                           (_filaSeleccionada + 1) + "," + (_colSeleccionada + 1) + ")";
            _filaSeleccionada = -1;
            _colSeleccionada = -1;

            CargarTodasLasCartas();
        }

        private void DibujarCelda(int f, int c)
        {
            var celda = _celdas[f, c];
            celda.Controls.Clear();

            var carta = _seleccion[f, c];
            if (carta == null)
            {
                var lblVacio = new Label();
                lblVacio.Text = "(" + (f + 1) + "," + (c + 1) + ")";
                lblVacio.ForeColor = Color.Gray;
                lblVacio.TextAlign = ContentAlignment.MiddleCenter;
                lblVacio.Dock = DockStyle.Fill;
                lblVacio.Click += (s, e) => SeleccionarCelda(f, c);
                celda.Controls.Add(lblVacio);
            }
            else
            {
                int ff = f, cc = c;
                var pic = new PictureBox();
                pic.Size = new Size(celda.Width - 4, celda.Height - 20);
                pic.Location = new Point(2, 2);
                pic.Image = ImageManager.ObtenerImagen(carta, pic.Width, pic.Height);
                pic.SizeMode = PictureBoxSizeMode.Zoom;
                pic.Cursor = Cursors.Hand;
                pic.Click += (s, e) => SeleccionarCelda(ff, cc);

                var lbl = new Label();
                lbl.Text = carta.Nombre;
                lbl.Font = new Font("Arial", 7);
                lbl.Location = new Point(0, celda.Height - 18);
                lbl.Size = new Size(celda.Width, 16);
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Click += (s, e) => SeleccionarCelda(ff, cc);

                celda.Controls.Add(pic);
                celda.Controls.Add(lbl);
            }
        }

        private void GenerarAleatorio()
        {
            var cartas = _engine.Baraja.CartasAleatoriasParaTablero(16);
            int i = 0;
            for (int f = 0; f < Tablero.FILAS; f++)
                for (int c = 0; c < Tablero.COLUMNAS; c++)
                    _seleccion[f, c] = cartas[i++];

            for (int f = 0; f < Tablero.FILAS; f++)
                for (int c = 0; c < Tablero.COLUMNAS; c++)
                    DibujarCelda(f, c);

            lblInfo.Text = "Tablero generado aleatoriamente.";
            CargarTodasLasCartas();
        }

        private void Limpiar()
        {
            for (int f = 0; f < Tablero.FILAS; f++)
                for (int c = 0; c < Tablero.COLUMNAS; c++)
                {
                    _seleccion[f, c] = null;
                    DibujarCelda(f, c);
                }
            CargarTodasLasCartas();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // Contar vacias
            int vacias = 0;
            for (int f = 0; f < Tablero.FILAS; f++)
                for (int c = 0; c < Tablero.COLUMNAS; c++)
                    if (_seleccion[f, c] == null) vacias++;

            if (vacias > 0)
            {
                var r = MessageBox.Show(
                    "Hay " + vacias + " celdas vacias. ¿Rellenar aleatoriamente?",
                    "Celdas vacias",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (r == DialogResult.Cancel) return;
                if (r == DialogResult.Yes)
                {
                    var disponibles = _engine.Baraja.TodasLasCartas
                        .Where(c => !EstaUsada(c))
                        .OrderBy(_ => Guid.NewGuid())
                        .ToList();
                    int idx = 0;
                    for (int f = 0; f < Tablero.FILAS; f++)
                        for (int c = 0; c < Tablero.COLUMNAS; c++)
                            if (_seleccion[f, c] == null && idx < disponibles.Count)
                                _seleccion[f, c] = disponibles[idx++];
                }
            }

            // Guardar en el tablero del jugador
            if (_engine.JugadorLocal != null)
            {
                var t = _engine.JugadorLocal.Tablero;
                for (int f = 0; f < Tablero.FILAS; f++)
                    for (int c = 0; c < Tablero.COLUMNAS; c++)
                        t.Cartas[f, c] = _seleccion[f, c];
            }

            MessageBox.Show("Tablero guardado.", "Listo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private bool EstaUsada(Carta carta)
        {
            for (int f = 0; f < Tablero.FILAS; f++)
                for (int c = 0; c < Tablero.COLUMNAS; c++)
                    if (_seleccion[f, c]?.Id == carta.Id) return true;
            return false;
        }
    }
}