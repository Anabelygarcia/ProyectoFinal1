//using ProyectoFinal.Forms;
using ProyectoFinal1.Forms;
using System;
using System.Windows.Forms;

namespace ProyectoFinal
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainMenuForm());
        }
    }
}