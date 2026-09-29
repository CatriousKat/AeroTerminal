using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AeroTerminal
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string initialDir = (args.Length > 0 && Directory.Exists(args[0])) ? args[0] : null;
            Application.Run(new MainForm(initialDir));
        }
    }
}