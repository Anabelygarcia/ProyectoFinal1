using System;
using System.Net.Sockets;
using ProyectoFinal1.Core;

namespace ProyectoFinal1.Models
{
    public enum EstadoPartida
    {
        Lobby,
        EnJuego,
        Pausada,
        FinRonda,
        Terminada
    }

    public class Jugador
    {
        public string Nombre { get; set; } = "Jugador";
        public bool EsHost { get; set; }
        public bool EsLocal { get; set; } = true;
        public Tablero Tablero { get; set; }
        public bool GanadorRonda { get; set; }
        public int PartidasGanadas { get; set; }
        public bool ListoParaJugar { get; set; }
        public TcpClient? Conexion { get; set; }
        public string DireccionIp { get; set; } = "127.0.0.1";

        public Jugador()
        {
            Tablero = new Tablero(Nombre);
        }

        public Jugador(string nombre, bool esHost = false, bool esLocal = true)
        {
            Nombre = nombre;
            EsHost = esHost;
            EsLocal = esLocal;
            Tablero = new Tablero(nombre);
        }

        public void ReiniciarRonda()
        {
            GanadorRonda = false;
            Tablero.LimpiarMarcas();
        }

        public bool EstaConectado() =>
            Conexion != null && Conexion.Connected;

        public override string ToString() =>
            $"{(EsHost ? "Host" : "Jugador")} {Nombre}";
    }

    public class ChatMensaje
    {
        public string Usuario { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public DateTime Hora { get; set; } = DateTime.Now;
        public bool EsSistema { get; set; }
        public bool EsPropio { get; set; }

        public string HoraFormateada => Hora.ToString("HH:mm");

        public ChatMensaje() { }

        public ChatMensaje(string usuario, string texto,
            bool esSistema = false, bool esPropio = false)
        {
            Usuario = usuario;
            Texto = texto;
            EsSistema = esSistema;
            EsPropio = esPropio;
        }
    }
}