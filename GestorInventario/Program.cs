using System;
using System.Windows.Forms;
using GestorInventario.Forms;

namespace GestorInventario
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            Application.Run(new FrmLogin());
        }
    }
}
