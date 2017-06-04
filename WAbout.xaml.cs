using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BackUper
{
    /// <summary>
    /// Логика взаимодействия для WAbout.xaml
    /// </summary>
    public partial class WAbout : Window
    {
        public WAbout()
        {
            InitializeComponent();
            var fv = FileVersionInfo.GetVersionInfo(Environment.CurrentDirectory + "\\" +  Process.GetCurrentProcess().ProcessName+ ".exe");
            lVersion.Content = fv.FileVersion.ToString();
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
