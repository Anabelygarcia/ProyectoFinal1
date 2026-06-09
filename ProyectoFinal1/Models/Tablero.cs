using System;
using System.Collections.Generic;

namespace ProyectoFinal1.Models
{
    public class Tablero
    {
        public const int FILAS = 4;
        public const int COLUMNAS = 4;
        public Carta?[,] Cartas { get; set; } = new Carta[FILAS, COLUMNAS];
        public bool[,] Marcado { get; set; } = new bool[FILAS, COLUMNAS];
        public string NombreJugador { get; set; } = "Jugador";
        public Tablero(string nombreJugador)
        {
            NombreJugador = nombreJugador;
        }

        public void LimpiarMarcas()
        {
            for (int f = 0; f < FILAS; f++)
                for (int c = 0; c < COLUMNAS; c++)
                    Marcado[f, c] = false;
        }

        public bool MarcarCarta(int cartaId)
        {
            for (int f = 0; f < FILAS; f++)
                for (int c = 0; c < COLUMNAS; c++)
                    if (Cartas[f, c]?.Id == cartaId)
                    {
                        Marcado[f, c] = true;
                        return true;
                    }
            return false;
        }

        public List<(int fila, int col)> CeldasMarcadas()
        {
            var lista = new List<(int, int)>();
            for (int f = 0; f < FILAS; f++)
                for (int c = 0; c < COLUMNAS; c++)
                    if (Marcado[f, c]) lista.Add((f, c));
            return lista;
        }

        public bool EstaCompleto()
        {
            for (int f = 0; f < FILAS; f++)
                for (int c = 0; c < COLUMNAS; c++)
                    if (Cartas[f, c] == null) return false;
            return true;
        }
        public void AsignarCartas(List<Carta> cartas)
        {
            int idx = 0;
            for (int f = 0; f < FILAS && idx < cartas.Count; f++)
                for (int c = 0; c < COLUMNAS && idx < cartas.Count; c++)
                    Cartas[f, c] = cartas[idx++];
        }
        public bool MarcarCelda(int fila, int columna)
        {
            if (fila < 0 || fila >= FILAS || columna < 0 || columna >= COLUMNAS) return false;
            Marcado[fila, columna] = true;
            return true;
        }
    }

}
