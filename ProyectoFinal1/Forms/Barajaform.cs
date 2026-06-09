using System;
using System.Windows.Forms;
using System.Drawing;
using ProyectoFinal1.Core;
using System.Threading;
using ProyectoFinal1.Models;

namespace ProyectoFinal1.Forms
{
    public class BarajaForm : Form
    {
        private readonly GameEngine _engine;

        private PictureBox picCarta;
        private Label lblNombreCarta;
        private Label lblGrito;
        private Label lblNumero;
        private Label lblRestantes;
        private Button btnSiguiente;
        private Button btnNuevaPartida;
        private FlowLayoutPanel flpAnteriores;
        private Label lblAnteriores;
        private PictureBox picSiguiente;
        private Label lblSiguienteTitulo;
        private CheckBox chkSonido;
        private System.Windows.Forms.Timer _timer;
        private bool _animando;
        private Carta? _cartaPendiente;

        public BarajaForm(GameEngine engine)
        {
            _engine = engine;
            InitializeComponent();
            _engine.NuevaPartida();
            ActualizarRestantes();
        }

        private void InitializeComponent()
        {
            this.Text = "Baraja - Cantor";
            this.Size = new Size(900, 680);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Carta grande
            var lblCartaTitulo = new Label();
            lblCartaTitulo.Text = "Carta actual:";
            lblCartaTitulo.Location = new Point(10, 10);
            lblCartaTitulo.Size = new Size(100, 20);

            picCarta = new PictureBox();
            picCarta.Location = new Point(10, 33);
            picCarta.Size = new Size(220, 290);
            picCarta.SizeMode = PictureBoxSizeMode.Zoom;
            picCarta.BorderStyle = BorderStyle.FixedSingle;
            picCarta.Image = ImageManager.ObtenerReverso(220, 290);

            lblNumero = new Label();
            lblNumero.Text = "#--";
            lblNumero.Font = new Font("Arial", 12, FontStyle.Bold);
            lblNumero.Location = new Point(10, 328);
            lblNumero.Size = new Size(60, 25);

            lblNombreCarta = new Label();
            lblNombreCarta.Text = "Presiona Siguiente...";
            lblNombreCarta.Font = new Font("Arial", 14, FontStyle.Bold);
            lblNombreCarta.Location = new Point(10, 355);
            lblNombreCarta.Size = new Size(230, 30);

            lblGrito = new Label();
            lblGrito.Text = "";
            lblGrito.Font = new Font("Arial", 8, FontStyle.Italic);
            lblGrito.Location = new Point(10, 388);
            lblGrito.Size = new Size(240, 40);

            // Botones
            btnSiguiente = new Button();
            btnSiguiente.Text = "SIGUIENTE CARTA";
            btnSiguiente.Font = new Font("Arial", 12, FontStyle.Bold);
            btnSiguiente.Location = new Point(10, 440);
            btnSiguiente.Size = new Size(240, 45);
            btnSiguiente.BackColor = Color.DarkRed;
            btnSiguiente.ForeColor = Color.White;
            btnSiguiente.Click += BtnSiguiente_Click;

            btnNuevaPartida = new Button();
            btnNuevaPartida.Text = "Nueva Partida";
            btnNuevaPartida.Location = new Point(10, 495);
            btnNuevaPartida.Size = new Size(240, 30);
            btnNuevaPartida.Click += BtnNuevaPartida_Click;

            lblRestantes = new Label();
            lblRestantes.Text = "Cartas restantes: 54";
            lblRestantes.Location = new Point(10, 535);
            lblRestantes.Size = new Size(200, 20);

            chkSonido = new CheckBox();
            chkSonido.Text = "Sonido activo";
            chkSonido.Checked = true;
            chkSonido.Location = new Point(10, 560);
            chkSonido.Size = new Size(130, 20);
            chkSonido.CheckedChanged += (s, e) =>
                AudioManager.Silenciado = !chkSonido.Checked;

            // Siguiente carta preview
            lblSiguienteTitulo = new Label();
            lblSiguienteTitulo.Text = "Siguiente:";
            lblSiguienteTitulo.Location = new Point(265, 10);
            lblSiguienteTitulo.Size = new Size(80, 20);

            picSiguiente = new PictureBox();
            picSiguiente.Location = new Point(265, 33);
            picSiguiente.Size = new Size(120, 160);
            picSiguiente.SizeMode = PictureBoxSizeMode.Zoom;
            picSiguiente.BorderStyle = BorderStyle.FixedSingle;
            picSiguiente.Image = ImageManager.ObtenerReverso(120, 160);

            // Historial
            lblAnteriores = new Label();
            lblAnteriores.Text = "Cartas anteriores:";
            lblAnteriores.Location = new Point(265, 210);
            lblAnteriores.Size = new Size(150, 20);

            flpAnteriores = new FlowLayoutPanel();
            flpAnteriores.Location = new Point(265, 233);
            flpAnteriores.Size = new Size(610, 400);
            flpAnteriores.AutoScroll = true;
            flpAnteriores.BorderStyle = BorderStyle.FixedSingle;
            flpAnteriores.FlowDirection = FlowDirection.LeftToRight;
            flpAnteriores.WrapContents = true;

            // Timer para animacion
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 80;
            _timer.Tick += Timer_Tick;

            this.Controls.AddRange(new Control[]
            {
                lblCartaTitulo, picCarta,
                lblNumero, lblNombreCarta, lblGrito,
                btnSiguiente, btnNuevaPartida,
                lblRestantes, chkSonido,
                lblSiguienteTitulo, picSiguiente,
                lblAnteriores, flpAnteriores
            });
        }

        private int _frame;

        private void BtnSiguiente_Click(object sender, EventArgs e)
        {
            if (_animando) return;
            if (!_engine.Baraja.HayCartas)
            {
                MessageBox.Show("Se acabaron las cartas!", "Fin",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _cartaPendiente = _engine.SacarCarta();
            if (_cartaPendiente != null)
            {
                _animando = true;
                _frame = 0;
                btnSiguiente.Enabled = false;
                picCarta.Image = ImageManager.ObtenerReverso(220, 290);
                _timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _frame++;
            if (_frame == 4 && _cartaPendiente != null)
            {
                MostrarCarta(_cartaPendiente);
            }
            if (_frame >= 6)
            {
                _timer.Stop();
                _animando = false;
                btnSiguiente.Enabled = true;
                ActualizarRestantes();
            }
        }

        private void MostrarCarta(Carta carta)
        {
            picCarta.Image = ImageManager.ObtenerImagen(carta, 220, 290);
            lblNombreCarta.Text = carta.Nombre;
            lblGrito.Text = carta.Grito;
            lblNumero.Text = "#" + carta.Id;

            // Agregar miniatura al historial
            var panel = new Panel();
            panel.Size = new Size(72, 95);

            var pic = new PictureBox();
            pic.Size = new Size(70, 80);
            pic.Location = new Point(1, 1);
            pic.Image = ImageManager.ObtenerImagen(carta, 70, 80);
            pic.SizeMode = PictureBoxSizeMode.Zoom;

            var lbl = new Label();
            lbl.Text = carta.Nombre;
            lbl.Font = new Font("Arial", 6);
            lbl.Location = new Point(0, 81);
            lbl.Size = new Size(72, 13);
            lbl.TextAlign = ContentAlignment.MiddleCenter;

            new ToolTip().SetToolTip(pic, carta.Nombre + "\n" + carta.Grito);

            panel.Controls.Add(pic);
            panel.Controls.Add(lbl);
            flpAnteriores.Controls.Add(panel);
            flpAnteriores.ScrollControlIntoView(panel);
        }

        private void BtnNuevaPartida_Click(object sender, EventArgs e)
        {
            _engine.NuevaPartida();
            picCarta.Image = ImageManager.ObtenerReverso(220, 290);
            picSiguiente.Image = ImageManager.ObtenerReverso(120, 160);
            lblNombreCarta.Text = "Presiona Siguiente...";
            lblGrito.Text = "";
            lblNumero.Text = "#--";
            flpAnteriores.Controls.Clear();
            ActualizarRestantes();
        }

        private void ActualizarRestantes()
        {
            lblRestantes.Text = "Cartas restantes: " + _engine.Baraja.CartasRestantes;
        }
    }
}