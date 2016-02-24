using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OSS.Win
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!IsExistProcess())
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                MessageBox.Show(@"OSS服务引擎程序已经在运行中！", @"警告");
                Application.Exit();
            }
        }

        private static bool IsExistProcess()
        {
            bool result = false;
            Process[] p = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Application.ExecutablePath));
            if (p != null && p.Length > 1)
            {
                result = true;
            }
            return result;
        }
    }
}
