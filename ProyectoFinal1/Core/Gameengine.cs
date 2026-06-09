using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoFinal1.Models;
using ProyectoFinal1.Core;
using ProyectoFinal1.Controls;

namespace ProyectoFinal1.Core
{

    public class GameEngine : IDisposable
    {
        public DeckManager Baraja { get; } = new DeckManager();
        public ChatServer Chat { get; } = new ChatServer();
        public EstadoPartida Estado { get; private set; } = EstadoPartida.Lobby;
        public ModoGanar ModoActual { get; private set; }
        public bool EsHost { get; set; }
        public Jugador? JugadorLocal { get; set; }

        private readonly List<Jugador> _jugadores = new();
        public IReadOnlyList<Jugador> Jugadores => _jugadores;

        public event Action<Carta>? CartaRevelada;

        public event Action<Jugador, Carta?>? GanadorDetectado;

        public event Action<ChatMensaje>? MensajeChat;

        public event Action<string, bool>? CambioJugadores;  

        public event Action<ModoGanar>? JuegoIniciado;

        public GameEngine()
        {
            ModoActual = WinChecker.ModoDelDia();
            SuscribirEventosRed();
        }

        private void SuscribirEventosRed()
        {
            Chat.MensajeRecibido += msg => MensajeChat?.Invoke(msg);
            Chat.JugadorConectado += nombre => CambioJugadores?.Invoke(nombre, true);
            Chat.JugadorDesconectado += nombre =>
            {
                QuitarJugador(nombre);
                CambioJugadores?.Invoke(nombre, false);
            };


            Chat.CartaRecibida += id =>
            {
                var carta = Baraja.BuscarPorId(id);
                if (carta != null) AplicarCarta(carta);
            };

            Chat.GanadorRecibido += nombre =>
            {
                var jugador = _jugadores.Find(j => j.Nombre == nombre);
                var siguiente = Baraja.VerSiguiente();
                if (jugador != null)
                    GanadorDetectado?.Invoke(jugador, siguiente);
            };

            Chat.JuegoIniciado += modoStr =>
            {
                if (Enum.TryParse(modoStr, out ModoGanar modo))
                    ModoActual = modo;
                Estado = EstadoPartida.EnJuego;
                JuegoIniciado?.Invoke(ModoActual);
            };
        }

        public void AgregarJugador(Jugador j)
        {
            if (!_jugadores.Contains(j))
                _jugadores.Add(j);
        }

        public void QuitarJugador(string nombre)
        {
            _jugadores.RemoveAll(j => j.Nombre == nombre);
        }

        public Jugador? BuscarJugador(string nombre) =>
            _jugadores.Find(j => j.Nombre == nombre);

        public void NuevaPartida(ModoGanar? modoForzado = null)
        {
            Baraja.Barajar();

            ModoActual = modoForzado ?? WinChecker.ModoDelDia();

            foreach (var j in _jugadores)
                j.ReiniciarRonda();

            Estado = EstadoPartida.EnJuego;
            AudioManager.ReproducirBarajar();

            if (EsHost)
                Chat.EnviarInicio(ModoActual.ToString());

            JuegoIniciado?.Invoke(ModoActual);
        }

        public Carta? SacarCarta()
        {
            if (Estado != EstadoPartida.EnJuego) return null;
            if (!Baraja.HayCartas)
            {
                Estado = EstadoPartida.FinRonda;
                return null;
            }

            var carta = Baraja.SiguienteCarta()!;

            AudioManager.ReproducirCarta(carta.Id);

            AplicarCarta(carta);

            if (EsHost)
                Chat.EnviarCarta(carta.Id);

            return carta;
        }

        private void AplicarCarta(Carta carta)
        {
            foreach (var j in _jugadores)
                j.Tablero.MarcarCarta(carta.Id);

            CartaRevelada?.Invoke(carta);
        }

        public bool VerificarGanador(Jugador jugador) =>
            WinChecker.Verificar(jugador.Tablero, ModoActual);

        public List<(int fila, int col)> CeldasGanadoras(Jugador jugador) =>
            WinChecker.CeldasGanadoras(jugador.Tablero, ModoActual);

        public void DeclararGanador(Jugador jugador)
        {
            if (Estado != EstadoPartida.EnJuego) return;

            if (!VerificarGanador(jugador))
                return;  

            Estado = EstadoPartida.FinRonda;
            jugador.GanadorRonda = true;
            jugador.PartidasGanadas++;

            var siguiente = Baraja.VerSiguiente();

            AudioManager.ReproducirLoteria();
            Chat.EnviarGanador(jugador.Nombre);
            GanadorDetectado?.Invoke(jugador, siguiente);
        }

        internal void DeclararGanadorRemoto(string nombre)
        {
            var jugador = BuscarJugador(nombre);
            var siguiente = Baraja.VerSiguiente();
            Estado = EstadoPartida.FinRonda;
            if (jugador != null)
            {
                jugador.GanadorRonda = true;
                jugador.PartidasGanadas++;
                GanadorDetectado?.Invoke(jugador, siguiente);
            }
            AudioManager.ReproducirLoteria();
        }

        public Task IniciarServidorAsync() =>
            Task.Run(() => Chat.IniciarServidorAsync());

        public Task<bool> ConectarClienteAsync(string host) =>
            JugadorLocal != null
                ? Chat.ConectarAsync(host, JugadorLocal.Nombre)
                : Task.FromResult(false);


        public string ResumenEstado() =>
            $"Partida: {Estado} | Modo: {WinChecker.Descripcion(ModoActual)} " +
            $"| Cartas jugadas: {Baraja.CartasJugadas}/54 " +
            $"| Jugadores: {_jugadores.Count}";

        public void Dispose()
        {
            Chat.Desconectar();
            Chat.DetenerServidor();
            Chat.Dispose();
            ImageManager.LimpiarCache();
            AudioManager.Detener();
        }

    }
}