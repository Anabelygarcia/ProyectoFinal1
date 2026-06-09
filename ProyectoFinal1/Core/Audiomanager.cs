using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Reflection;
using System.Threading.Tasks;

namespace ProyectoFinal1.Core
{
    public static class AudioManager
    {
        
        private static bool _silenciado = false;
        private static SoundPlayer? _playerActual = null;

        private static readonly Dictionary<string, byte[]> _cache = new();

        public static bool Silenciado
        {
            get => _silenciado;
            set
            {
                _silenciado = value;
                if (value) Detener();
            }
        }

        public static void ReproducirCarta(int cartaId)
        {
            if (_silenciado) return;
            ReproducirArchivo($"carta_{cartaId:D2}.wav");
        }

        public static void ReproducirLoteria()
        {
            if (_silenciado) return;
            ReproducirArchivo("loteria.wav", usarFallback: SystemSounds.Exclamation);
        }

        public static void ReproducirBarajar()
        {
            if (_silenciado) return;
            ReproducirArchivo("barajar.wav", usarFallback: SystemSounds.Asterisk);
        }

        public static void ReproducirFicha()
        {
            if (_silenciado) return;
            ReproducirArchivo("ficha.wav", usarFallback: SystemSounds.Beep);
        }

        public static void ReproducirError()
        {
            if (_silenciado) return;
            ReproducirArchivo("error.wav", usarFallback: SystemSounds.Hand);
        }

        public static void ReproducirInicio()
        {
            if (_silenciado) return;
            ReproducirArchivo("inicio.wav", usarFallback: SystemSounds.Asterisk);
        }
        
        public static void ReproducirArchivo(string nombreArchivo,
            System.Media.SystemSound? usarFallback = null)
        {
            if (_silenciado) return;

            try
            {
                byte[]? datos = ObtenerDatosSonido(nombreArchivo);

                if (datos != null)
                {
                    Detener();
                    var ms = new MemoryStream(datos);
                    _playerActual = new SoundPlayer(ms);
                    _playerActual.Play();
                }
                else
                {
                    (usarFallback ?? SystemSounds.Beep).Play();
                }
            }
            catch
            {
            }
        }

        public static void Detener()
        {
            try { _playerActual?.Stop(); } catch { }
            _playerActual = null;
        }

        public static void LimpiarCache() => _cache.Clear();

        private static byte[]? ObtenerDatosSonido(string nombreArchivo)
        {
            if (_cache.TryGetValue(nombreArchivo, out var cached))
                return cached;

            byte[]? datos = CargarDesdeRecursoEmbebido(nombreArchivo);

            if (datos == null)
                datos = CargarDesdeArchivoExterno(nombreArchivo);

            if (datos != null)
                _cache[nombreArchivo] = datos;

            return datos;
        }

        private static byte[]? CargarDesdeRecursoEmbebido(string nombreArchivo)
        {
            try
            {
                string nombreRecurso = $"ProyectoFinal1.Resources.Sounds.{nombreArchivo}";
                var asm = Assembly.GetExecutingAssembly();
                using var stream = asm.GetManifestResourceStream(nombreRecurso);
                if (stream == null) return null;

                var ms = new MemoryStream();
                stream.CopyTo(ms);
                return ms.ToArray();
            }
            catch { return null; }
        }

        private static byte[]? CargarDesdeArchivoExterno(string nombreArchivo)
        {
            try
            {
                // Buscar en ./Sounds/ relativo al ejecutable
                string carpeta = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Sounds");
                string ruta = Path.Combine(carpeta, nombreArchivo);

                if (!File.Exists(ruta)) return null;
                return File.ReadAllBytes(ruta);
            }
            catch { return null; }
        }
    }
}