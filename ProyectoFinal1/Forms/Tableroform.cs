using LoteriaGame.Controls;
using ProyectoFinal1.Controls;
using ProyectoFinal1.Core;
using ProyectoFinal1.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ProyectoFinal1.Forms
{
    public class TableroForm : Form
    {
        private readonly GameEngine _engine;
        private readonly CartaControl[,] _celdas = new CartaControl[Tablero.FILAS, Tablero.COLUMNAS];

        private Label lblNombre;
        private Label lblModo;
        private Label lblEstado;
        private Button btnLoteria;
        private Panel pnlTablero;
        private Panel pnlFichas;
        private FlowLayoutPanel flpHistorial;
        private Label lblHistorial;
        private PictureBox picCartaActual;
        private Label lblCartaActual;

        public TableroForm(GameEngine engine)
        {
            _engine = engine;
            InitializeComponent();
            CargarTablero();
            SuscribirEventos();
        }

        private void InitializeComponent()
        {
            string nombre = _engine.JugadorLocal?.Nombre ?? "Jugador";
            this.Text = "Tablero de " + nombre;
            this.Size = new Size(900, 680);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Info superior
            lblNombre = new Label();
            lblNombre.Text = "Jugador: " + nombre;
            lblNombre.Font = new Font("Arial", 12, FontStyle.Bold);
            lblNombre.Location = new Point(10, 10);
            lblNombre.Size = new Size(250, 25);

            lblModo = new Label();
            lblModo.Text = "Modo: " + WinChecker.Descripcion(_engine.ModoActual);
            lblModo.Location = new Point(10, 38);
            lblModo.Size = new Size(400, 20);

            lblEstado = new Label();
            lblEstado.Text = "Esperando que salgan cartas...";
            lblEstado.Location = new Point(10, 60);
            lblEstado.Size = new Size(500, 20);
            lblEstado.ForeColor = Color.DarkBlue;

            // Boton LOTERIA
            btnLoteria = new Button();
            btnLoteria.Text = "LOTERIA!";
            btnLoteria.Font = new Font("Arial", 14, FontStyle.Bold);
            btnLoteria.Location = new Point(700, 10);
            btnLoteria.Size = new Size(150, 50);
            btnLoteria.BackColor = Color.Red;
            btnLoteria.ForeColor = Color.White;
            btnLoteria.Click += BtnLoteria_Click;

            // Panel del tablero 4x4
            pnlTablero = new Panel();
            pnlTablero.Location = new Point(10, 90);
            pnlTablero.Size = new Size(520, 530);
            pnlTablero.BorderStyle = BorderStyle.FixedSingle;

            // Panel de fichas (abajo)
            pnlFichas = new Panel();
            pnlFichas.Location = new Point(10, 625);
            pnlFichas.Size = new Size(520, 30);
            // Las fichas se crean en CargarTablero

            // Carta actual (derecha)
            var lblCartaTitulo = new Label();
            lblCartaTitulo.Text = "Carta actual:";
            lblCartaTitulo.Location = new Point(545, 90);
            lblCartaTitulo.Size = new Size(100, 20);

            picCartaActual = new PictureBox();
            picCartaActual.Location = new Point(545, 113);
            picCartaActual.Size = new Size(150, 200);
            picCartaActual.SizeMode = PictureBoxSizeMode.Zoom;
            picCartaActual.BorderStyle = BorderStyle.FixedSingle;

            lblCartaActual = new Label();
            lblCartaActual.Text = "---";
            lblCartaActual.Font = new Font("Arial", 11, FontStyle.Bold);
            lblCartaActual.Location = new Point(545, 318);
            lblCartaActual.Size = new Size(200, 25);

            // Historial
            lblHistorial = new Label();
            lblHistorial.Text = "Cartas anteriores:";
            lblHistorial.Location = new Point(545, 355);
            lblHistorial.Size = new Size(150, 20);

            flpHistorial = new FlowLayoutPanel();
            flpHistorial.Location = new Point(545, 378);
            flpHistorial.Size = new Size(320, 240);
            flpHistorial.AutoScroll = true;
            flpHistorial.BorderStyle = BorderStyle.FixedSingle;
            flpHistorial.FlowDirection = FlowDirection.LeftToRight;
            flpHistorial.WrapContents = true;

            this.Controls.AddRange(new Control[]
            {
                lblNombre, lblModo, lblEstado, btnLoteria,
                pnlTablero, pnlFichas,
                lblCartaTitulo, picCartaActual, lblCartaActual,
                lblHistorial, flpHistorial
            });

            // Fichas en la bandeja
            CrearFichas();
        }

        private void CargarTablero()
        {
            if (_engine.JugadorLocal == null) return;

            var tablero = _engine.JugadorLocal.Tablero;

            // Si no tiene cartas, asignar aleatorias
            if (!tablero.EstaCompleto())
                tablero.AsignarCartas(_engine.Baraja.CartasAleatoriasParaTablero(16));

            int tam = 126, gap = 6, ox = 8, oy = 8;

            for (int f = 0; f < Tablero.FILAS; f++)
            {
                for (int c = 0; c < Tablero.COLUMNAS; c++)
                {
                    var ctrl = new CartaControl();
                    ctrl.Carta = tablero.Cartas[f, c];
                    ctrl.Fila = f;
                    ctrl.Columna = c;
                    ctrl.Location = new Point(ox + c * (tam + gap), oy + f * (tam + gap));
                    ctrl.Size = new Size(tam, tam + 12);
                    ctrl.FichaMarcada += OnFichaMarcada;
                    _celdas[f, c] = ctrl;
                    pnlTablero.Controls.Add(ctrl);
                }
            }
        }

        private void CrearFichas()
        {
            int x = 0;
            for (int i = 0; i < 16; i++)
            {
                var ficha = new FichaControl();
                ficha.Location = new Point(x, 0);
                ficha.Size = new Size(30, 30);
                pnlFichas.Controls.Add(ficha);
                x += 32;
            }
        }

        private void SuscribirEventos()
        {
            _engine.CartaRevelada += carta =>
                this.Invoke(() => MostrarCarta(carta));

            _engine.GanadorDetectado += (jugador, siguiente) =>
                this.Invoke(() =>
                {
                    var f = new GanadorForm(jugador, siguiente, _engine.ModoActual);
                    f.ShowDialog(this);
                });
        }

        private void MostrarCarta(Carta carta)
        {
            // Actualizar imagen carta actual
            picCartaActual.Image = ImageManager.ObtenerImagen(carta, 150, 200);
            lblCartaActual.Text = carta.Nombre;
            lblEstado.Text = "Salio: " + carta.Nombre;

            // Agregar al historial
            var pic = new PictureBox();
            pic.Size = new Size(55, 70);
            pic.Image = ImageManager.ObtenerImagen(carta, 55, 70);
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            new ToolTip().SetToolTip(pic, carta.Nombre + "\n" + carta.Grito);
            flpHistorial.Controls.Add(pic);
            flpHistorial.ScrollControlIntoView(pic);

            // Marcar en el tablero visual si corresponde
            for (int f = 0; f < Tablero.FILAS; f++)
                for (int c = 0; c < Tablero.COLUMNAS; c++)
                    if (_celdas[f, c].Carta?.Id == carta.Id)
                        _celdas[f, c].Invalidate();
        }

        private void OnFichaMarcada(object sender, CartaEventArgs e)
        {
            if (_engine.JugadorLocal == null) return;

            _engine.JugadorLocal.Tablero.MarcarCelda(e.Fila, e.Columna);
            lblEstado.Text = "Marcaste: " + e.Carta.Nombre;

            // Revisar si ya gano
            if (_engine.VerificarGanador(_engine.JugadorLocal))
                lblEstado.Text = "Puedes cantar LOTERIA!";
        }

        private void BtnLoteria_Click(object sender, EventArgs e)
        {
            if (_engine.JugadorLocal == null) return;

            if (_engine.VerificarGanador(_engine.JugadorLocal))
            {
                _engine.DeclararGanador(_engine.JugadorLocal);
            }
            else
            {
                MessageBox.Show(
                    "Aun no cumples la condicion.\nNecesitas: " +
                    WinChecker.Descripcion(_engine.ModoActual),
                    "No valido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}