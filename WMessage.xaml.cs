using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BackUper
{
    /// <summary>
    /// Логика взаимодействия для WMessage.xaml
    /// </summary>
    public partial class WMessage : Window
    {
        Timer tClose = new Timer();

        public WMessage()
        {
            InitializeComponent();
            tClose.Elapsed += CloseWindow;
        }

        public WMessage(string text) : this()
        {
            tbMessage.Text = text;
        }

        public WMessage(string text, string title)
            : this(text)
        {
            Title = title;
        }

        public WMessage(string text, string title, int timeToClose)
            : this(text, title)
        {
            tClose.Interval = timeToClose;
            tClose.Start();
        }


        private void CloseWindow(object sender, ElapsedEventArgs e)
        {
            tClose.Stop();
            this.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(delegate()
            {
                this.Close();
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }










    }
}
