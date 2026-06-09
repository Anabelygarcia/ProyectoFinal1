using System;
using System.Drawing;
using System.Windows.Forms;
using ProyectoFinal1.Core;
using ProyectoFinal1.Models;

namespace ProyectoFinal1.Forms
{
    public class GanadorForm : Form
    {
        private readonly Jugador _ganador;
        private readonly Carta? _siguiente;
        private readonly ModoGanar _modo;

        public GanadorForm(Jugador ganador, Carta? siguiente, ModoGanar modo)
        {
            _ganador = ganador;
            _siguiente = siguiente;
            _modo = modo;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "LOTERIA!";
            this.Size = new Size(500, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(255, 250, 220);

            // Titulo
            var lblLoteria = new Label();
            lblLoteria.Text = "¡LOTERIA!";
            lblLoteria.Font = new Font("Arial", 36, FontStyle.Bold);
            lblLoteria.ForeColor = Color.DarkRed;
            lblLoteria.TextAlign = ContentAlignment.MiddleCenter;
            lblLoteria.Location = new Point(0, 15);
            lblLoteria.Size = new Size(494, 70);

            // Ganador
            var lblGanador = new Label();
            lblGanador.Text = "Gano: " + _ganador.Nombre;
            lblGanador.Font = new Font("Arial", 20, FontStyle.Bold);
            lblGanador.ForeColor = Color.DarkGreen;
            lblGanador.TextAlign = ContentAlignment.MiddleCenter;
            lblGanador.Location = new Point(0, 90);
            lblGanador.Size = new Size(494, 40);

            // Modo
            var lblModo = new Label();
            lblModo.Text = "Condicion: " + WinChecker.Descripcion(_modo);
            lblModo.Font = new Font("Arial", 11);
            lblModo.TextAlign = ContentAlignment.MiddleCenter;
            lblModo.Location = new Point(0, 135);
            lblModo.Size = new Size(494, 25);

            // Separador
            var sep = new Panel();
            sep.Location = new Point(50, 168);
            sep.Size = new Size(394, 2);
            sep.BackColor = Color.DarkRed;

            // Carta que seguia
            var lblSiguienteTitulo = new Label();
            lblSiguienteTitulo.Text = "La carta que seguia era:";
            lblSiguienteTitulo.Font = new Font("Arial", 11, FontStyle.Italic);
            lblSiguienteTitulo.ForeColor = Color.DarkGray;
            lblSiguienteTitulo.TextAlign = ContentAlignment.MiddleCenter;
            lblSiguienteTitulo.Location = new Point(0, 178);
            lblSiguienteTitulo.Size = new Size(494, 22);

            this.Controls.AddRange(new Control[]
            {
                lblLoteria, lblGanador, lblModo, sep, lblSiguienteTitulo
            });

            if (_siguiente != null)
            {
                var picSiguiente = new PictureBox();
                picSiguiente.Location = new Point(162, 205);
                picSiguiente.Size = new Size(170, 220);
                picSiguiente.SizeMode = PictureBoxSizeMode.Zoom;
                picSiguiente.Image = ImageManager.ObtenerImagen(_siguiente, 170, 220);
                picSiguiente.BorderStyle = BorderStyle.FixedSingle;

                var lblNombreSig = new Label();
                lblNombreSig.Text = _siguiente.Nombre;
                lblNombreSig.Font = new Font("Arial", 13, FontStyle.Bold);
                lblNombreSig.TextAlign = ContentAlignment.MiddleCenter;
                lblNombreSig.Location = new Point(0, 430);
                lblNombreSig.Size = new Size(494, 28);

                var lblGrito = new Label();
                lblGrito.Text = _siguiente.Grito;
                lblGrito.Font = new Font("Arial", 8, FontStyle.Italic);
                lblGrito.ForeColor = Color.DarkGray;
                lblGrito.TextAlign = ContentAlignment.MiddleCenter;
                lblGrito.Location = new Point(10, 460);
                lblGrito.Size = new Size(474, 18);

                this.Controls.Add(picSiguiente);
                this.Controls.Add(lblNombreSig);
                this.Controls.Add(lblGrito);
            }
            else
            {
                var lblNoHay = new Label();
                lblNoHay.Text = "Era la ultima carta de la baraja!";
                lblNoHay.Font = new Font("Arial", 12, FontStyle.Italic);
                lblNoHay.ForeColor = Color.DarkOrange;
                lblNoHay.TextAlign = ContentAlignment.MiddleCenter;
                lblNoHay.Location = new Point(0, 250);
                lblNoHay.Size = new Size(494, 30);
                this.Controls.Add(lblNoHay);
            }

            // Boton cerrar
            var btnCerrar = new Button();
            btnCerrar.Text = "Nueva Partida";
            btnCerrar.Font = new Font("Arial", 12, FontStyle.Bold);
            btnCerrar.Location = new Point(172, 482);
            btnCerrar.Size = new Size(150, 35);
            btnCerrar.BackColor = Color.DarkRed;
            btnCerrar.ForeColor = Color.White;
            btnCerrar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCerrar);
        }
    }
}