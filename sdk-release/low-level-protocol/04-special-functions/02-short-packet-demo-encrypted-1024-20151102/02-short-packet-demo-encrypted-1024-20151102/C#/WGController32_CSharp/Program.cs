using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WGController32_CSharp
{
    static class Program
    {
        /// <summary>
        /// Application main entry point。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
