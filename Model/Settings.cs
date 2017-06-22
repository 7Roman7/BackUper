using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BackUper.Model
{
    [Serializable]
    public class Settings : INotifyPropertyChanged
    {
        #region Хранимые данные
        private AdditionalSettings options;

        public string directoryForBackup;
        public List<FileItem> lFiles;




        public AdditionalSettings Options
        {
            get { return options; }
            set
            {
                options = value;
                OnPropertyChanged("Options");
            }
        }

        public WindowSettings wsMain;
        public WindowSettings wsProcess;

        #endregion Хранимые данные

        /// <summary>
        /// Настройки по умолчанию
        /// </summary>
        public Settings()
        {
            directoryForBackup = Environment.GetLogicalDrives()[0];//первый выпавший диск
            lFiles = new List<FileItem>();
            wsMain = new WindowSettings();
            wsProcess = new WindowSettings(300, 200);
            options = new AdditionalSettings();
        }

        #region Члены INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion




        [Serializable]
        public class WindowSettings
        {//Класс настроек типового окна
            public double left = 0;
            public double top = 0;

            public double width = 0;
            public double height = 0;

            public WindowSettings() { }
            public WindowSettings(double h, double w)
                : this()
            {
                height = h;
                width = w;
            }
            public WindowSettings(Window w)
                : this()
            {
                left = w.Left;
                top = w.Top;
                height = w.Height;
                width = w.Width;
            }

            public void LoadValueFromWindow(Window w)
            {
                left = w.Left;
                top = w.Top;
                height = w.Height;
                width = w.Width;
            }
            public void SetValueToWindow(Window w)
            {
                w.Left = (left > 0 && left < 1000) ? left : 0;
                w.Top = (top > 0 && top < 1000) ? top : 0;
                w.Height = height;
                w.Width = width;
            }

        }

        [Serializable]
        public class AdditionalSettings :  INotifyPropertyChanged
        {
            private bool doZip = false;
            private bool deleteAfterZip = false;
            private bool closeAfterBackup = false;
            private DateTime lastBackup = DateTime.MinValue;




            public bool DoZip
            {
                get { return doZip; }
                set
                {
                    doZip = value;
                    OnPropertyChanged("DoZip");
                }
            }
            public bool DeleteAfterZip
            {
                get { return deleteAfterZip; }
                set
                {
                    deleteAfterZip = value;
                    OnPropertyChanged("DeleteAfterZip");
                }
            }
            public bool CloseAfterBackup
            {
                get { return closeAfterBackup; }
                set
                {
                    closeAfterBackup = value;
                    OnPropertyChanged("CloseAfterBackup");
                }
            }
            public DateTime LastBackup
            {
                get { return lastBackup; }
                set
                {
                    lastBackup = value;
                    OnPropertyChanged("LastBackup");
                }
            }


            #region Члены INotifyPropertyChanged

            public event PropertyChangedEventHandler PropertyChanged;
            public void OnPropertyChanged([CallerMemberName]string prop = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
            #endregion
        }

    }
}
