using System;
using System.Collections.Generic;
using System.Linq;
using ProyectoFinal1.Models;

namespace ProyectoFinal1.Core
{
    public class DeckManager : Carta
    {
        private readonly List<Carta> _todasLasCartas = new();
        private List<Carta> _barajaActual = new();
        private readonly List<Carta> _historial = new();
        private readonly Random _rnd = new();
        public IReadOnlyList<Carta> TodasLasCartas => _todasLasCartas;
        public IReadOnlyList<Carta> Historial => _historial;
        public int CartasRestantes => _barajaActual.Count;
        public int CartasJugadas => _historial.Count;
        public bool HayCartas => _barajaActual.Count > 0;
        public Carta? CartaActual => _historial.Count > 0 ? _historial[^1] : null;
        public DeckManager()
        {
            InicializarCartas();
        }
        private void InicializarCartas()
        {
            var datos = new (int id, string nombre, string grito, string emoji)[]
            {
                ( 1, "El Gallo",        "¡El que le cantó a San Pedro no le volverá a cantar!",                           "🐓"),
                ( 2, "El Diablito",     "¡Pórtate bien cuñado o te lleva el coloradito!",                                 "😈"),
                ( 3, "La Dama",         "¡La que se viste de seda en el petate se queda!",                                "👩"),
                ( 4, "El Catrín",       "¡Don Ferruco en la Alameda, su bastón quería tirar!",                            "🎩"),
                ( 5, "El Paraguas",     "¡Paraguas de buena nota, el que no te cubre no te remoja!",                      "☂️"),
                ( 6, "La Sirena",       "¡Con los cantos de sirena no te vayas a marear!",                                "🧜"),
                ( 7, "La Escalera",     "¡Súbeme pasito a pasito, no me vayas a tirar!",                                  "🪜"),
                ( 8, "La Botella",      "¡El que con vino se acuesta, con agua se levanta!",                              "🍾"),
                ( 9, "El Barril",       "¡Al que le toca le toca, aunque se tape con la cola!",                           "🛢️"),
                (10, "El Árbol",        "¡El que a buen árbol se arrima, buena sombra le cobija!",                        "🌳"),
                (11, "El Melón",        "¡El melón y el zapote se parecen en el cogote!",                                 "🍈"),
                (12, "El Valiente",     "¡No, con el valiente no, con el valiente no!",                                   "💪"),
                (13, "El Gorrito",      "¡Una de cal y otra de arena, pa' que el adobe no se vaya!",                      "🎓"),
                (14, "La Muerte",       "¡Al pasar por el panteón me encontré un calaveron!",                             "💀"),
                (15, "La Pera",         "¡El que espera, desespera!",                                                     "🍐"),
                (16, "La Bandera",      "¡El verde, blanco y colorado, la bandera del soldado!",                          "🇲🇽"),
                (17, "El Bandolón",     "¡Tocando el bandolón en Acapulco se quedó Simón!",                               "🎸"),
                (18, "El Violoncello",  "¡Creciente del río, la llorona y el violoncello!",                               "🎻"),
                (19, "La Garza",        "¡La garza se fue a la laguna pa' que no la bobera ninguna!",                     "🦢"),
                (20, "El Pájaro",       "¡Pájaro que come tierra, algo de Dios alcanzará!",                               "🐦"),
                (21, "La Mano",         "¡La mano de un criminal!",                                                       "✋"),
                (22, "La Bota",         "¡Que buen peón, el que pega hasta el tacón!",                                    "👢"),
                (23, "La Luna",         "¡El farol de los enamorados!",                                                   "🌙"),
                (24, "El Cotorro",      "¡El que con cotorros anda, a cotorrear se enseña!",                              "🦜"),
                (25, "El Borracho",     "¡El que con vino se acuesta, con vino se levanta!",                              "🍺"),
                (26, "El Negrito",      "¡El que comió la hoja fue el gusanito!",                                         "👦"),
                (27, "El Corazón",      "¡No me extrañes corazón, que regreso en el camión!",                             "❤️"),
                (28, "La Sandía",       "¡Pártela, pártela que es sandía, papas, camotes y sandía!",                      "🍉"),
                (29, "El Tambor",       "¡El que fue a Sevilla perdió su silla!",                                         "🥁"),
                (30, "El Camarón",      "¡Camarón que se duerme, se lo lleva la corriente!",                              "🦐"),
                (31, "Las Jaras",       "¡Las jaras del indio azteca me las clavé en la chaqueta!",                       "🏹"),
                (32, "El Músico",       "¡El músico baila y toca, traga polvo y no se emboca!",                           "🎺"),
                (33, "La Araña",        "¡Tisarón, tisarón, el que se lleva no gana nada!",                               "🕷️"),
                (34, "El Soldado",      "¡Uno, dos y tres, el soldado de la guerra al revés!",                            "💂"),
                (35, "La Estrella",     "¡La guía de los marineros!",                                                     "⭐"),
                (36, "El Cazo",         "¡Cazuela, cazuela, le dijo el cazo!",                                            "🍳"),
                (37, "El Mundo",        "¡Todo el mundo al avión, porque se va la ilusión!",                              "🌍"),
                (38, "El Apache",       "¡Carita de niño, no seas malvado!",                                              "🪶"),
                (39, "El Nopal",        "¡Al nopal lo van a ver nomás cuando tiene tunas!",                               "🌵"),
                (40, "El Alacrán",      "¡Alacrán azul del sesenta, el que te pica revienta!",                            "🦂"),
                (41, "La Rosa",         "¡Rosita, rósame, ¡es que ya te huele el pie!",                                   "🌹"),
                (42, "La Calavera",     "¡Al pasar por el panteón me encontré un calaveron, calavera del montón!",        "💀"),
                (43, "El Cantarito",    "¡Tanto va el cántaro al agua que se rompe!",                                     "🏺"),
                (44, "El Venado",       "¡Saltó el venado, pasó por el manzano, le dio un tiro y lo peló con la mano!",   "🦌"),
                (45, "El Sol",          "¡La cobija de los pobres!",                                                      "☀️"),
                (46, "La Corona",       "¡Todo aquel que trabaja merece que se le ponga la corona!",                      "👑"),
                (47, "La Chalupa",      "¡Rema que rema con su rebozo de bolitas!",                                       "🚣"),
                (48, "El Pino",         "¡Frío, frío, frío como el pino en la sierra!",                                   "🌲"),
                (49, "El Pescado",      "¡Sabroso para el que lo guisa, sabroso para el que lo quiere!",                  "🐟"),
                (50, "La Palma",        "¡Palmero, súbete a la palma y tráeme de lo que está arriba!",                    "🌴"),
                (51, "La Maceta",       "¡El que nació para maceta, del cuarto no pasa!",                                 "🪴"),
                (52, "El Arpa",         "¡El arpa por ser tan grande no cabía en el cuarto!",                             "🎵"),
                (53, "La Rana",         "¡Al que le salga la rana, de su casa sale el agua!",                             "🐸"),
                (54, "El Maguey",       "¡De las plantas mexicanas el maguey es el más bello!",                           "🌿"),
            };

            _todasLasCartas.Clear();
            foreach (var (id, nombre, grito, emoji) in datos)
            _todasLasCartas.Add(new Carta(id, nombre, grito, emoji));
        }
        public void Barajar()
        {
            _barajaActual = _todasLasCartas.OrderBy(_ => _rnd.Next()).ToList();
            _historial.Clear();
        }
        public void BarajarSubconjunto(List<Carta> cartas)
        {
            _barajaActual = cartas.OrderBy(_ => _rnd.Next()).ToList();
            _historial.Clear();
        }
        public Carta? SiguienteCarta()
        {
            if (!HayCartas) return null;
            var carta = _barajaActual[0];
            _barajaActual.RemoveAt(0);
            _historial.Add(carta);
            return carta;
        }
        public Carta? VerSiguiente() =>
            _barajaActual.Count > 0 ? _barajaActual[0] : null;

        public List<Carta> VerProximas(int cantidad)
        {
            int tomar = Math.Min(cantidad, _barajaActual.Count);
            return _barajaActual.GetRange(0, tomar);
        }
        public Carta? ObtenerDelHistorial(int indice)
        {
            if (indice < 0 || indice >= _historial.Count) return null;
            return _historial[indice];
        }
        public bool FueJugada(int cartaId) =>
            _historial.Exists(c => c.Id == cartaId);
        public Carta? BuscarPorId(int id) =>
            _todasLasCartas.Find(c => c.Id == id);
        public List<Carta> CartasAleatoriasParaTablero(int cantidad = 16)
        {
            if (cantidad > _todasLasCartas.Count)
                cantidad = _todasLasCartas.Count;
            return _todasLasCartas
                .OrderBy(_ => _rnd.Next())
                .Take(cantidad)
                .ToList();
        }
        public List<List<Carta>> GenerarTablerosMultiples(int numJugadores, int cartasPorTablero = 16)
        {
            var resultado = new List<List<Carta>>();
            var pool = _todasLasCartas.OrderBy(_ => _rnd.Next()).ToList();

            for (int j = 0; j < numJugadores; j++)
            {
                if (pool.Count < cartasPorTablero)
                    pool.AddRange(_todasLasCartas.OrderBy(_ => _rnd.Next()));

                resultado.Add(pool.GetRange(0, cartasPorTablero));
                pool.RemoveRange(0, cartasPorTablero);
            }

            return resultado;
        }
    }
}