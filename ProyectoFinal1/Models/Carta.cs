using System.Drawing;

namespace ProyectoFinal1.Models
{
    public class Carta
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Grito { get; set; } = "";        
        public string ImagenPath { get; set; } = ""; 
        public string SonidoPath { get; set; } = "";   
        public Image? Imagen { get; set; }
        public string Emoji { get; }

        public Carta() { }

        public Carta(int id, string nombre, string grito, string imagenPath, string sonidoPath)
        {
            Id = id;
            Nombre = nombre;
            Grito = grito;
            ImagenPath = imagenPath;
            SonidoPath = sonidoPath;
        }

        public Carta(int id, string nombre, string grito, string emoji)
        {
            Id = id;
            Nombre = nombre;
            Grito = grito;
            Emoji = emoji;
        }

        public override string ToString() => $"#{Id:D2} {Nombre}";
    }
}
