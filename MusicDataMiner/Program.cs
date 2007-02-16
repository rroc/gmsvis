using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MusicDataminer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main(String[] argv)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 mainWindow = new Form1();
            Application.Run( mainWindow );
            return 0;
        }
    }
}
