using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProyectoFinal1.Core;
using ProyectoFinal1.Models;

namespace ProyectoFinal1.Forms
{
    public class MainMenuForm : Form
    {
        private GameEngine _engine = new GameEngine();

        // Controles
        private Label lblTitulo;
        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblHost;
        private TextBox txtHost;
        private RadioButton rbHost;
        private RadioButton rbCliente;
        private Button btnConectar;
        private Button btnJugar;
        private Button btnCustomizar;
        private Label lblModo;
        private Label lblIp;
        private ListBox lstJugadores;
        private Label lblJugadores;
        private RichTextBox rtbChat;
        private TextBox txtMensaje;
        private Button btnEnviar;
        private Label lblChat;

        public MainMenuForm()
        {
            InitializeComponent();
            SuscribirEventos();
        }

        private void InitializeComponent()
        {
            this.Text = "Loteria Mexicana - Menu Principal";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Titulo
            lblTitulo = new Label();
            lblTitulo.Text = "LOTERIA MEXICANA";
            lblTitulo.Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold);
            lblTitulo.Location = new System.Drawing.Point(250, 10);
            lblTitulo.Size = new System.Drawing.Size(350, 40);

            // Modo del dia
            lblModo = new Label();
            lblModo.Text = "Modo de hoy: " + WinChecker.Descripcion(WinChecker.ModoDelDia());
            lblModo.Location = new System.Drawing.Point(10, 55);
            lblModo.Size = new System.Drawing.Size(400, 20);

            // Nombre
            lblNombre = new Label();
            lblNombre.Text = "Tu nombre:";
            lblNombre.Location = new System.Drawing.Point(10, 85);
            lblNombre.Size = new System.Drawing.Size(80, 20);

            txtNombre = new TextBox();
            txtNombre.Location = new System.Drawing.Point(95, 83);
            txtNombre.Size = new System.Drawing.Size(150, 20);
            txtNombre.Text = "Jugador1";

            // Radio buttons
            rbHost = new RadioButton();
            rbHost.Text = "Ser Host";
            rbHost.Location = new System.Drawing.Point(10, 115);
            rbHost.Checked = true;
            rbHost.CheckedChanged += (s, e) => txtHost.Enabled = !rbHost.Checked;

            rbCliente = new RadioButton();
            rbCliente.Text = "Unirse";
            rbCliente.Location = new System.Drawing.Point(90, 115);

            // IP del host
            lblHost = new Label();
            lblHost.Text = "IP del Host:";
            lblHost.Location = new System.Drawing.Point(10, 145);
            lblHost.Size = new System.Drawing.Size(80, 20);

            txtHost = new TextBox();
            txtHost.Location = new System.Drawing.Point(95, 143);
            txtHost.Size = new System.Drawing.Size(150, 20);
            txtHost.Text = "127.0.0.1";
            txtHost.Enabled = false;

            // IP local
            lblIp = new Label();
            lblIp.Text = "Tu IP: obteniendo...";
            lblIp.Location = new System.Drawing.Point(10, 170);
            lblIp.Size = new System.Drawing.Size(250, 20);
            ObtenerIpLocal();

            // Boton conectar
            btnConectar = new Button();
            btnConectar.Text = "Conectar";
            btnConectar.Location = new System.Drawing.Point(10, 195);
            btnConectar.Size = new System.Drawing.Size(100, 30);
            btnConectar.Click += BtnConectar_Click;

            // Lista jugadores
            lblJugadores = new Label();
            lblJugadores.Text = "Jugadores conectados:";
            lblJugadores.Location = new System.Drawing.Point(10, 235);
            lblJugadores.Size = new System.Drawing.Size(200, 20);

            lstJugadores = new ListBox();
            lstJugadores.Location = new System.Drawing.Point(10, 258);
            lstJugadores.Size = new System.Drawing.Size(250, 120);

            // Boton jugar
            btnJugar = new Button();
            btnJugar.Text = "INICIAR JUEGO";
            btnJugar.Location = new System.Drawing.Point(10, 390);
            btnJugar.Size = new System.Drawing.Size(120, 35);
            btnJugar.Click += BtnJugar_Click;

            // Boton personalizar
            btnCustomizar = new Button();
            btnCustomizar.Text = "Personalizar Tablero";
            btnCustomizar.Location = new System.Drawing.Point(140, 390);
            btnCustomizar.Size = new System.Drawing.Size(120, 35);
            btnCustomizar.Click += BtnCustomizar_Click;

            // Chat
            lblChat = new Label();
            lblChat.Text = "Chat:";
            lblChat.Location = new System.Drawing.Point(280, 55);
            lblChat.Size = new System.Drawing.Size(50, 20);

            rtbChat = new RichTextBox();
            rtbChat.Location = new System.Drawing.Point(280, 75);
            rtbChat.Size = new System.Drawing.Size(490, 400);
            rtbChat.ReadOnly = true;

            txtMensaje = new TextBox();
            txtMensaje.Location = new System.Drawing.Point(280, 485);
            txtMensaje.Size = new System.Drawing.Size(390, 25);
            txtMensaje.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    EnviarMensaje();
                }
            };

            btnEnviar = new Button();
            btnEnviar.Text = "Enviar";
            btnEnviar.Location = new System.Drawing.Point(680, 483);
            btnEnviar.Size = new System.Drawing.Size(80, 28);
            btnEnviar.Click += (s, e) => EnviarMensaje();

            // Agregar controles
            this.Controls.AddRange(new Control[]
            {
                lblTitulo, lblModo,
                lblNombre, txtNombre,
                rbHost, rbCliente,
                lblHost, txtHost,
                lblIp, btnConectar,
                lblJugadores, lstJugadores,
                btnJugar, btnCustomizar,
                lblChat, rtbChat, txtMensaje, btnEnviar
            });
        }

        private void SuscribirEventos()
        {
            _engine.Chat.MensajeRecibido += msg =>
                this.Invoke(() => AgregarMensaje(msg.Usuario, msg.Texto));

            _engine.Chat.JugadorConectado += nombre =>
                this.Invoke(() =>
                {
                    if (!lstJugadores.Items.Contains(nombre))
                        lstJugadores.Items.Add(nombre);
                    AgregarMensaje("Sistema", nombre + " se unio");
                });

            _engine.Chat.JugadorDesconectado += nombre =>
                this.Invoke(() =>
                {
                    lstJugadores.Items.Remove(nombre);
                    AgregarMensaje("Sistema", nombre + " se desconecto");
                });
        }

        private async void BtnConectar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Escribe tu nombre.");
                return;
            }

            btnConectar.Enabled = false;
            btnConectar.Text = "Conectando...";

            string nombre = txtNombre.Text.Trim();
            _engine.JugadorLocal = new Jugador(nombre, rbHost.Checked);
            _engine.AgregarJugador(_engine.JugadorLocal);
            _engine.EsHost = rbHost.Checked;

            if (rbHost.Checked)
            {
                _ = Task.Run(() => _engine.Chat.IniciarServidorAsync());
                await Task.Delay(300);
                await _engine.Chat.ConectarAsync("127.0.0.1", nombre);
                lstJugadores.Items.Add(nombre + " (tu)");
                AgregarMensaje("Sistema", "Servidor iniciado. Esperando jugadores...");
            }
            else
            {
                bool ok = await _engine.Chat.ConectarAsync(txtHost.Text.Trim(), nombre);
                if (ok)
                {
                    lstJugadores.Items.Add(nombre + " (tu)");
                    AgregarMensaje("Sistema", "Conectado al servidor.");
                }
                else
                {
                    MessageBox.Show("No se pudo conectar al servidor.");
                    btnConectar.Enabled = true;
                    btnConectar.Text = "Conectar";
                    return;
                }
            }

            btnConectar.Text = "Conectado";
        }

        private void BtnJugar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Escribe tu nombre.");
                return;
            }

            if (_engine.JugadorLocal == null)
            {
                string nombre = txtNombre.Text.Trim();
                _engine.JugadorLocal = new Jugador(nombre, true);
                _engine.AgregarJugador(_engine.JugadorLocal);
                _engine.EsHost = true;
            }

            var tableroForm = new TableroForm(_engine);
            tableroForm.Show();

            if (_engine.EsHost)
            {
                var barajaForm = new BarajaForm(_engine);
                barajaForm.Show();
            }

            this.Hide();
        }

        private void BtnCustomizar_Click(object sender, EventArgs e)
        {
            if (_engine.JugadorLocal == null)
            {
                string nombre = txtNombre.Text.Trim();
                if (string.IsNullOrEmpty(nombre)) nombre = "Jugador";
                _engine.JugadorLocal = new Jugador(nombre);
                _engine.AgregarJugador(_engine.JugadorLocal);
            }

            var f = new CustomizarForm(_engine);
            f.ShowDialog(this);
        }

        private void EnviarMensaje()
        {
            string texto = txtMensaje.Text.Trim();
            if (string.IsNullOrEmpty(texto)) return;

            _engine.Chat.EnviarChat(texto);
            AgregarMensaje(txtNombre.Text.Trim(), texto);
            txtMensaje.Clear();
        }

        private void AgregarMensaje(string usuario, string texto)
        {
            rtbChat.AppendText($"[{DateTime.Now:HH:mm}] {usuario}: {texto}\n");
            rtbChat.ScrollToCaret();
        }

        private void ObtenerIpLocal()
        {
            try
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    { lblIp.Text = "Tu IP: " + ip.ToString(); return; }
            }
            catch { lblIp.Text = "Tu IP: 127.0.0.1"; }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _engine.Dispose();
            base.OnFormClosing(e);
        }
    }
}