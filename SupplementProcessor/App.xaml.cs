using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace SupplementProcessor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static RegistryKey CurrentUser { get; set; }
        public static bool SkipConstrains { get; set; }


        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //MainWindow wnd = new MainWindow();
            if (e.Args.Length == 1)
            {
                MessageBox.Show("Now opening file: \n\n" + e.Args[0]);
                SkipConstrains = true;
            }

            //wnd.Show();
        }

        public static void SetAssociation(string Extension, string KeyName, string OpenWith, string FileDescription)
        {
            
        }
    }
}
