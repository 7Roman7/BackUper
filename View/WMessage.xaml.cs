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
using BackUper;


namespace BackUper.View
{
    /// <summary>
    /// Логика взаимодействия для WMessage.xaml
    /// </summary>
    public partial class WMessage : Window
    {
        /// <summary>
        /// Таймер для прогресса и закрытия объекта
        /// </summary>
        Timer tClosing = new Timer(100);

        public WMessage()
        {
            InitializeComponent();
            tClosing.Elapsed += ClosingProgress;
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

        /// <summary>
        /// Конструктор с самозакрыванием
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="timeToClose">Время для закрытия в декасекундах(1/10)</param>
        public WMessage(string text, string title, int timeToClose)
            : this(text, title)
        {
            pbClosing.Maximum = timeToClose;
            tClosing.Start();
        }

        private void ClosingProgress(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(delegate ()
            {
                if (pbClosing.Value == pbClosing.Maximum)
                {
                    tClosing.Stop();
                    Close();
                }
                else
                {
                    pbClosing.Value += 1;
                }
            }));
        }

        private void BOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }










    }
}
