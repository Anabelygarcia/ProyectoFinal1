using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProyectoFinal1.Models;

namespace ProyectoFinal1.Core
{
    public class ChatServer : IDisposable
    {
        public const int PUERTO          = 9876;
        public const int TIMEOUT_RECV_MS = 30_000;
        private const char SEP           = '|';

        private TcpListener?                        _listener;
        private readonly List<ClienteConectado>     _clientes    = new();
        private readonly object                     _lockClientes = new();
        private bool                                _servidorActivo;
        private CancellationTokenSource             _cts = new();

        private TcpClient?    _clienteConexion;
        private StreamWriter? _escritor;
        private string        _nombreLocal = string.Empty;
        private bool          _clienteConectado;
        public bool EsServidorActivo   => _servidorActivo;
        public bool EsClienteConectado => _clienteConectado;
        public int  NumJugadores       { get { lock (_lockClientes) return _clientes.Count; } }

        public event Action<ChatMensaje>? MensajeRecibido;
        public event Action<string>?      JugadorConectado;     
        public event Action<string>?      JugadorDesconectado;   
        public event Action<int>?         CartaRecibida;         
        public event Action<string>?      GanadorRecibido;       
        public event Action<string>?      JuegoIniciado;         
        public event Action<string>?      ErrorConexion;        

        public async Task IniciarServidorAsync()
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, PUERTO);
            try
            {
                _listener.Start();
                _servidorActivo = true;

                while (!_cts.IsCancellationRequested)
                {
                    TcpClient cliente = await _listener.AcceptTcpClientAsync(_cts.Token);
                    cliente.ReceiveTimeout = TIMEOUT_RECV_MS;
                    var cc = new ClienteConectado(cliente);
                    lock (_lockClientes) _clientes.Add(cc);
                    _ = Task.Run(() => AtenderClienteAsync(cc), _cts.Token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                ErrorConexion?.Invoke($"Error servidor: {ex.Message}");
            }
            finally
            {
                _servidorActivo = false;
            }
        }

        public void DetenerServidor()
        {
            _cts.Cancel();
            _listener?.Stop();
            lock (_lockClientes)
            {
                foreach (var cc in _clientes) cc.Cerrar();
                _clientes.Clear();
            }
            _servidorActivo = false;
        }

        private async Task AtenderClienteAsync(ClienteConectado cc)
        {
            string nombre = "Desconocido";
            try
            {
                using var reader = new StreamReader(cc.Stream, Encoding.UTF8);
                string? linea;
                while ((linea = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    ProcesarLineaServidor(linea, ref nombre, cc, GetMensajeRecibido());
                }
            }
            catch { /* cliente se desconectó */ }
            finally
            {
                lock (_lockClientes) _clientes.Remove(cc);
                cc.Cerrar();
                JugadorDesconectado?.Invoke(nombre);
                BroadcastServidor($"CHAT|Sistema|❌ {nombre} se desconectó", null);
            }
        }

        private Action<ChatMensaje>? GetMensajeRecibido()
        {
            return MensajeRecibido;
        }

        private void ProcesarLineaServidor(string linea, ref string nombre, ClienteConectado origen, Action<ChatMensaje>? mensajeRecibido)
        {
            var partes = linea.Split(SEP, 3);
            if (partes.Length < 2) return;
            string tipo = partes[0];
            string p1   = partes.Length > 1 ? partes[1] : string.Empty;
            string p2   = partes.Length > 2 ? partes[2] : string.Empty;

            switch (tipo)
            {
                case "JUGADOR":
                    nombre = p1;
                    JugadorConectado?.Invoke(nombre);
                    BroadcastServidor($"CHAT|Sistema|✅ {nombre} se unió al juego", origen);
                    break;

                case "CHAT":
                    BroadcastServidor(linea, null);
                    mensajeRecibido?.Invoke(new ChatMensaje(p1, p2, true, false));
                    break;

                case "CARTA":
                    BroadcastServidor(linea, origen);
                    if (int.TryParse(p1, out int idCarta))
                        CartaRecibida?.Invoke(idCarta);
                    break;

                case "GANADOR":
                    BroadcastServidor(linea, null);
                    GanadorRecibido?.Invoke(p1);
                    break;

                case "INICIO":
                    BroadcastServidor(linea, null);
                    JuegoIniciado?.Invoke(p1);
                    break;

                case "PING":
                    EnviarACliente(origen, "PONG|");
                    break;

                case "SALIR":
                    nombre = string.IsNullOrEmpty(p1) ? nombre : p1;
                    break;
            }
        }
        public void BroadcastServidor(string mensaje, ClienteConectado? excluir)
        {
            byte[] datos = Encoding.UTF8.GetBytes(mensaje + "\n");
            lock (_lockClientes)
            {
                var desconectados = new List<ClienteConectado>();
                foreach (var cc in _clientes)
                {
                    if (cc == excluir) continue;
                    try { cc.Stream.Write(datos, 0, datos.Length); }
                    catch { desconectados.Add(cc); }
                }
                foreach (var cc in desconectados) _clientes.Remove(cc);
            }
        }

        private void EnviarACliente(ClienteConectado cc, string mensaje)
        {
            try
            {
                byte[] datos = Encoding.UTF8.GetBytes(mensaje + "\n");
                cc.Stream.Write(datos, 0, datos.Length);
            }
            catch { }
        }
        public async Task<bool> ConectarAsync(string host, string nombreJugador)
        {
            try
            {
                _nombreLocal       = nombreJugador;
                _clienteConexion   = new TcpClient();
                _clienteConexion.ReceiveTimeout = TIMEOUT_RECV_MS;

                await _clienteConexion.ConnectAsync(host, PUERTO);
                var stream = _clienteConexion.GetStream();
                _escritor  = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                await _escritor.WriteLineAsync($"JUGADOR{SEP}{nombreJugador}");

                _clienteConectado = true;

                _ = Task.Run(EscucharServidorAsync);
                return true;
            }
            catch (Exception ex)
            {
                ErrorConexion?.Invoke($"No se pudo conectar: {ex.Message}");
                return false;
            }
        }

        private async Task EscucharServidorAsync()
        {
            try
            {
                using var reader = new StreamReader(
                    _clienteConexion!.GetStream(), Encoding.UTF8);
                string? linea;
                while ((linea = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    ProcesarLineaCliente(linea);
                }
            }
            catch { }
            finally
            {
                _clienteConectado = false;
                ErrorConexion?.Invoke("Se perdió la conexión con el servidor.");
            }
        }

        private void ProcesarLineaCliente(string linea)
        {
            var partes = linea.Split(SEP, 3);
            if (partes.Length < 1) return;
            string tipo = partes[0];
            string p1   = partes.Length > 1 ? partes[1] : string.Empty;
            string p2   = partes.Length > 2 ? partes[2] : string.Empty;

            switch (tipo)
            {
                case "CHAT":
                    bool esPropio = p1 == _nombreLocal;
                    MensajeRecibido?.Invoke(new ChatMensaje(p1, p2, false, esPropio));
                    break;

                case "CARTA":
                    if (int.TryParse(p1, out int id)) CartaRecibida?.Invoke(id);
                    break;

                case "GANADOR":
                    GanadorRecibido?.Invoke(p1);
                    break;

                case "INICIO":
                    JuegoIniciado?.Invoke(p1);
                    break;

                case "JUGADOR":
                    if (p1 != _nombreLocal)
                        JugadorConectado?.Invoke(p1);
                    break;

                case "PONG":
                    break;
            }
        }

        public void EnviarChat(string texto)
        {
            Enviar($"CHAT{SEP}{_nombreLocal}{SEP}{texto}");
        }

        public void EnviarCarta(int cartaId)
        {
            string msg = $"CARTA{SEP}{cartaId}";
            Enviar(msg);
            BroadcastServidor(msg, null);
        }
        public void EnviarGanador(string nombre)
        {
            string msg = $"GANADOR{SEP}{nombre}";
            Enviar(msg);
            BroadcastServidor(msg, null);
        }
        public void EnviarInicio(string modoGanar = "")
        {
            string msg = $"INICIO{SEP}{modoGanar}";
            Enviar(msg);
            BroadcastServidor(msg, null);
        }
        public void EnviarPing() => Enviar("PING|");

        private void Enviar(string mensaje)
        {
            try { _escritor?.WriteLine(mensaje); }
            catch { }
        }
        public void Desconectar()
        {
            try { _escritor?.WriteLine($"SALIR{SEP}{_nombreLocal}"); } catch { }
            _escritor?.Close();
            _clienteConexion?.Close();
            _clienteConectado = false;
        }
        public void Dispose()
        {
            Desconectar();
            DetenerServidor();
            _cts.Dispose();
        }
        public sealed class ClienteConectado
        {
            public TcpClient   TcpClient { get; }
            public NetworkStream Stream  { get; }
            public string       Nombre   { get; set; } = "?";

            public ClienteConectado(TcpClient tcp)
            {
                TcpClient = tcp;
                Stream    = tcp.GetStream();
            }

            public void Cerrar()
            {
                try { Stream.Close(); }    catch { }
                try { TcpClient.Close(); } catch { }
            }
        }
    }
}