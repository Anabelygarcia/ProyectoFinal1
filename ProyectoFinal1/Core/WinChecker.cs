using System;
using System.Collections.Generic;
using ProyectoFinal1.Models;
using ProyectoFinal1.Controls;

namespace ProyectoFinal1.Core
{
    public enum ModoGanar
    {
        Linea,      // Lunes
        Vertical,   // Martes
        Diagonal,   // Miércoles
        FormaL,     // Jueves
        Cruz,       // Viernes
        Marco,      // Sábado
        TablaLlena  // Domingo
    }

    public static class WinChecker
    {
        public static ModoGanar ModoDelDia()
        {
            return DateTime.Today.DayOfWeek switch
            {
                DayOfWeek.Monday => ModoGanar.Linea,
                DayOfWeek.Tuesday => ModoGanar.Vertical,
                DayOfWeek.Wednesday => ModoGanar.Diagonal,
                DayOfWeek.Thursday => ModoGanar.FormaL,
                DayOfWeek.Friday => ModoGanar.Cruz,
                DayOfWeek.Saturday => ModoGanar.Marco,
                _ => ModoGanar.TablaLlena
            };
        }

        public static string Descripcion(ModoGanar modo) => modo switch
        {
            ModoGanar.Linea => "Línea horizontal",
            ModoGanar.Vertical => "Línea vertical",
            ModoGanar.Diagonal => "Diagonal",
            ModoGanar.FormaL => "Forma de L",
            ModoGanar.Cruz => "Cruz central",
            ModoGanar.Marco => "Marco (bordes)",
            ModoGanar.TablaLlena => "Tabla llena",
            _ => ""
        };
        public static bool Verificar(Tablero tablero, ModoGanar modo)
        {
            bool[,] m = tablero.Marcado;
            int F = Tablero.FILAS, C = Tablero.COLUMNAS;

            return modo switch
            {
                ModoGanar.Linea => TieneLineaHorizontal(m, F, C),
                ModoGanar.Vertical => TieneLineaVertical(m, F, C),
                ModoGanar.Diagonal => TieneDiagonal(m, F),
                ModoGanar.FormaL => TieneFormaL(m, F, C),
                ModoGanar.Cruz => TieneCruz(m, F, C),
                ModoGanar.Marco => TieneMarco(m, F, C),
                ModoGanar.TablaLlena => TieneTablaLlena(m, F, C),
                _ => false
            };
        }

        static bool TieneLineaHorizontal(bool[,] m, int F, int C)
        {
            for (int f = 0; f < F; f++)
            {
                bool linea = true;
                for (int c = 0; c < C; c++) if (!m[f, c]) { linea = false; break; }
                if (linea) return true;
            }
            return false;
        }

        static bool TieneLineaVertical(bool[,] m, int F, int C)
        {
            for (int c = 0; c < C; c++)
            {
                bool linea = true;
                for (int f = 0; f < F; f++) if (!m[f, c]) { linea = false; break; }
                if (linea) return true;
            }
            return false;
        }

        static bool TieneDiagonal(bool[,] m, int F)
        {
            bool d1 = true, d2 = true;
            for (int i = 0; i < F; i++)
            {
                if (!m[i, i]) d1 = false;
                if (!m[i, F - 1 - i]) d2 = false;
            }
            return d1 || d2;
        }

        static bool TieneFormaL(bool[,] m, int F, int C)
        {
            if (m[0, 0] && m[1, 0] && m[2, 0] && m[3, 0] && m[3, 1]) return true;
            if (m[0, 0] && m[0, 1] && m[0, 2] && m[0, 3] && m[1, 3]) return true;
            
            if (m[0, 3] && m[1, 3] && m[2, 3] && m[3, 3] && m[3, 2]) return true;
            if (m[0, 0] && m[0, 1] && m[0, 2] && m[0, 3] && m[1, 0]) return true;
            
            if (m[3, 0] && m[2, 0] && m[1, 0] && m[0, 0] && m[0, 1]) return true;
            if (m[3, 0] && m[3, 1] && m[3, 2] && m[3, 3] && m[2, 3]) return true;
            
            if (m[3, 3] && m[2, 3] && m[1, 3] && m[0, 3] && m[0, 2]) return true;
            if (m[3, 3] && m[3, 2] && m[3, 1] && m[3, 0] && m[2, 0]) return true;
            return false;
        }

        static bool TieneCruz(bool[,] m, int F, int C)
        {
            for (int c = 0; c < C; c++) if (!m[1, c] || !m[2, c]) return false;
            for (int f = 0; f < F; f++) if (!m[f, 1] || !m[f, 2]) return false;
            return true;
        }

        static bool TieneMarco(bool[,] m, int F, int C)
        {
            for (int c = 0; c < C; c++) if (!m[0, c] || !m[F - 1, c]) return false;
            for (int f = 1; f < F - 1; f++) if (!m[f, 0] || !m[f, C - 1]) return false;
            return true;
        }

        static bool TieneTablaLlena(bool[,] m, int F, int C)
        {
            for (int f = 0; f < F; f++)
                for (int c = 0; c < C; c++)
                    if (!m[f, c]) return false;
            return true;
        }
    public static List<(int fila, int col)> CeldasGanadoras(Tablero tablero, ModoGanar modo)
        {
            bool[,] m = tablero.Marcado;
            return modo switch
            {
                ModoGanar.Linea => CeldasLineaHorizontal(m),
                ModoGanar.Vertical => CeldasLineaVertical(m),
                ModoGanar.Diagonal => CeldasDiagonal(m),
                ModoGanar.FormaL => CeldasFormaL(m),
                ModoGanar.Cruz => CeldasCruz(m),
                ModoGanar.Marco => CeldasMarco(m),
                ModoGanar.TablaLlena => CeldasTablaLlena(m),
                _ => new List<(int, int)>()
            };
        }

        private static List<(int, int)> CeldasLineaHorizontal(bool[,] m)
        {
            for (int f = 0; f < 4; f++)
            {
                bool ok = true;
                for (int c = 0; c < 4; c++) if (!m[f, c]) { ok = false; break; }
                if (ok)
                {
                    var l = new List<(int, int)>();
                    for (int c = 0; c < 4; c++) l.Add((f, c));
                    return l;
                }
            }
            return new List<(int, int)>();
        }

        private static List<(int, int)> CeldasLineaVertical(bool[,] m)
        {
            for (int c = 0; c < 4; c++)
            {
                bool ok = true;
                for (int f = 0; f < 4; f++) if (!m[f, c]) { ok = false; break; }
                if (ok)
                {
                    var l = new List<(int, int)>();
                    for (int f = 0; f < 4; f++) l.Add((f, c));
                    return l;
                }
            }
            return new List<(int, int)>();
        }

        private static List<(int, int)> CeldasDiagonal(bool[,] m)
        {
            bool d1 = true;
            for (int i = 0; i < 4; i++) if (!m[i, i]) { d1 = false; break; }
            if (d1)
            {
                var l = new List<(int, int)>();
                for (int i = 0; i < 4; i++) l.Add((i, i));
                return l;
            }
            bool d2 = true;
            for (int i = 0; i < 4; i++) if (!m[i, 3 - i]) { d2 = false; break; }
            if (d2)
            {
                var l = new List<(int, int)>();
                for (int i = 0; i < 4; i++) l.Add((i, 3 - i));
                return l;
            }
            return new List<(int, int)>();
        }

        private static List<(int, int)> CeldasFormaL(bool[,] m)
        {
            var formas = new (int f, int c)[][]
            {
        new[]{(0,0),(1,0),(2,0),(3,0),(3,1)},
        new[]{(3,0),(2,0),(1,0),(0,0),(0,1)},
        new[]{(0,3),(1,3),(2,3),(3,3),(3,2)},
        new[]{(3,3),(2,3),(1,3),(0,3),(0,2)},
        new[]{(0,0),(0,1),(0,2),(0,3),(1,3)},
        new[]{(0,3),(0,2),(0,1),(0,0),(1,0)},
        new[]{(3,0),(3,1),(3,2),(3,3),(2,3)},
        new[]{(3,3),(3,2),(3,1),(3,0),(2,0)},
            };
            foreach (var forma in formas)
            {
                bool ok = true;
                foreach (var (f, c) in forma) if (!m[f, c]) { ok = false; break; }
                if (ok)
                {
                    var l = new List<(int, int)>();
                    foreach (var (f, c) in forma) l.Add((f, c));
                    return l;
                }
            }
            return new List<(int, int)>();
        }

        private static List<(int, int)> CeldasCruz(bool[,] m)
        {
            for (int c = 0; c < 4; c++) if (!m[1, c] || !m[2, c]) return new List<(int, int)>();
            for (int f = 0; f < 4; f++) if (!m[f, 1] || !m[f, 2]) return new List<(int, int)>();
            var set = new System.Collections.Generic.HashSet<(int, int)>();
            for (int c = 0; c < 4; c++) { set.Add((1, c)); set.Add((2, c)); }
            for (int f = 0; f < 4; f++) { set.Add((f, 1)); set.Add((f, 2)); }
            return new List<(int, int)>(set);
        }

        private static List<(int, int)> CeldasMarco(bool[,] m)
        {
            for (int c = 0; c < 4; c++) if (!m[0, c] || !m[3, c]) return new List<(int, int)>();
            for (int f = 1; f < 3; f++) if (!m[f, 0] || !m[f, 3]) return new List<(int, int)>();
            var l = new List<(int, int)>();
            for (int c = 0; c < 4; c++) { l.Add((0, c)); l.Add((3, c)); }
            for (int f = 1; f < 3; f++) { l.Add((f, 0)); l.Add((f, 3)); }
            return l;
        }

        private static List<(int, int)> CeldasTablaLlena(bool[,] m)
        {
            var l = new List<(int, int)>();
            for (int f = 0; f < 4; f++)
                for (int c = 0; c < 4; c++)
                    if (!m[f, c]) return new List<(int, int)>();
                    else l.Add((f, c));
            return l;
        }
    }
}