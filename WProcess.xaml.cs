using BackUper.Model;
using BackUper.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.IO.Compression;
using BackUper.View;

namespace BackUper
{
    /// <summary>
    /// Логика взаимодействия для WProcess.xaml
    /// </summary>
    public partial class WProcess : Window
    {
        #region Переменные/Свойства

        public Thread thread;
        public System.Timers.Timer tTimeOfWork = new System.Timers.Timer();
        public int timeOfWorkSec = 0;
        public int timeOfWorkMin = 0;
        public int timeOfWorkHour = 0;

        public string FolderForBackup { get { return tbFolderForBackup.Text; } set { tbFolderForBackup.Text = value; } }
        
        /// <summary>
        /// Свойство списка к обработке
        /// </summary>
        public List<FileItem> LFiles 
        {
            get
            {
                return lFiles; 
            }
            set
            {
                lFiles = value;
                BuildCheckBoxList(value);
            }
        }
        /// <summary>
        /// хранение списка к обработке
        /// </summary>
        private List<FileItem> lFiles;

        /// <summary>
        /// Состояние процесса 0 - Wait, 1 - Work, 2 - Stopped
        /// </summary>
        [DefaultValue(false)]
        public byte State 
        {
            set 
            {
                state = value;

                switch (value)
                {
                    case 0://Wait
                        bStart.IsEnabled = true;
                        bStop.IsEnabled = false;
                        pbState.ToolTip = "Wait";
                        lProcessState.Content = "Wait";
                        pbState.IsIndeterminate = false;

                        TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                        tTimeOfWork.Stop();
                        break;

                    case 1://Work
                        bStart.IsEnabled = false;
                        bStop.IsEnabled = true;
                        pbState.ToolTip = "Work";
                        lProcessState.Content = "Work";
                        pbState.IsIndeterminate = true;
                        TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;

                        timeOfWorkHour = timeOfWorkMin = timeOfWorkSec = 0;
                        tTimeOfWork.Start();
                        break;

                    case 2://Stopped
                        bStart.IsEnabled = true;
                        bStop.IsEnabled = false;
                        pbState.ToolTip = "Stopped";
                        lProcessState.Content = "Stopped";
                        pbState.IsIndeterminate = false;

                        TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                        timeOfWorkHour = timeOfWorkMin = timeOfWorkSec = 0;
                        tTimeOfWork.Stop();
                        break;
                }
            }
            get
            {
                return state;
            }
        }

        public byte state = 0; 

        #endregion Переменные/Свойства

        public WProcess()
        {
            InitializeComponent();
            tTimeOfWork.Interval = 1000;
            tTimeOfWork.Elapsed += ShowTime;
            State = 0;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (thread != null)
                thread.Abort();

            (Owner as WMain).SettingsCurrent.wsProcess.LoadValueFromWindow(this);
        }

        /// <summary>
        /// Отображение времени выполнения процесса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowTime(object sender, ElapsedEventArgs e)
        {
            timeOfWorkSec ++;
            //разбивка времени
            if (timeOfWorkSec >= 60)
            {
                timeOfWorkMin++;
                timeOfWorkSec = 0;
            }
            if (timeOfWorkMin >= 60)
            {
                timeOfWorkHour++;
                timeOfWorkMin = 0;
            }

            this.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(delegate()
            {
                tbTimeOfWork.Text = String.Format("{0}:{1}:{2}",
                    (timeOfWorkHour < 10 ? "0":"") + timeOfWorkHour.ToString(),
                    (timeOfWorkMin < 10 ? "0" : "") + timeOfWorkMin.ToString(),
                    (timeOfWorkSec < 10 ? "0" : "") + timeOfWorkSec.ToString()
                    );
            }));
        }

        #region CheckBoxesStates
        
        /// <summary>
        /// Генерация списка чек боксов
        /// </summary>
        /// <param name="lFiles"></param>
        public void BuildCheckBoxList(List<FileItem> lFiles)
        {
            foreach (var f in lFiles)
            {
                var cb = new CheckBox();
                cb.IsChecked = false;
                cb.IsThreeState = true;
                cb.Content = f.Name;
                cb.IsEnabled = false;
                spcbStates.Children.Add(cb);
            }

            if (SaveOpen.settings.Options.DoZip)
            {
                var cb = new CheckBox();
                cb.IsChecked = false;
                cb.IsThreeState = true;
                cb.Content = "Ziping";
                cb.IsEnabled = false;
                spcbStates.Children.Add(cb);
            }
            if (SaveOpen.settings.Options.DoZip && SaveOpen.settings.Options.DeleteAfterZip)
            {
                var cb = new CheckBox();
                cb.IsChecked = false;
                cb.IsThreeState = true;
                cb.Content = "Deleting copy";
                cb.IsEnabled = false;
                spcbStates.Children.Add(cb);
            }

        }

        /// <summary>
        /// очистка чек боксов состояния
        /// </summary>
        private void ClearCheckBoxListStates()
        {
            foreach (var cb in spcbStates.Children)
            {
                (cb as CheckBox).IsChecked = false;
            }
        }

        /// <summary>
        /// Установка значения checkBox-а из другого потока
        /// </summary>
        /// <param name="i"></param>
        /// <param name="state"></param>
        private void SetCheckBox(int i, bool? state)
        {
            this.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(delegate()
            {
                (spcbStates.Children[i] as CheckBox).IsChecked = state;
            }));
        }
        #endregion CheckBoxesStates

        /// <summary>
        /// Пуск процесса бэкапа
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="lFiles"></param>
        /// <returns></returns>
        public bool SaveBackup(string filePath, List<FileItem> lFiles)
        {
            try
            {
                //подготовка папки
                filePath = String.Format("{0}\\{1}_{2}", filePath, "Backup", DateTime.Now.ToString("yyyy-MM-dd H-mm-ss"));
                Directory.CreateDirectory(filePath);
                thread = new Thread(BackupProcess);

                TParams p = new TParams();
                p.lFiles = LFiles;
                p.mainFolder = filePath;
                thread.Start(p);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
        }


        /// <summary>
        /// Процесс бэкапа
        /// </summary>
        /// <param name="args"></param>
        private void BackupProcess(object args)
        {
            var lErrors = new List<string>();
            var tParams = (args as TParams);
            tParams.CheckConflicts();
            var i = 0;

            foreach (var f in tParams.lFiles)
            {
                //пометка о начале операции
                SetCheckBox(i, null);
                var aimPath = tParams.mainFolder;
                //Изменение целевой директории при дубле
                if (tParams.Conflicts[i])
                {
                    aimPath = tParams.mainFolder + "\\" + f.Name;
                    Directory.CreateDirectory(aimPath);
                }
                //операция
                if (SaveOpen.DoCopy(f.PathString, aimPath))
                {
                    //пометка о завершении операции с успехом
                    SetCheckBox(i, true);
                }
                i++;
            }
            //Архивирования
            if (SaveOpen.settings.Options.DoZip)
            {
                SetCheckBox(i, null);
                ZipFile.CreateFromDirectory(tParams.mainFolder, tParams.mainFolder + ".zip");
                SetCheckBox(i, true);
                i++;
                //Удаление не архивированной копии
                if (SaveOpen.settings.Options.DeleteAfterZip)
                {
                    SetCheckBox(i, null);
                    Directory.Delete(tParams.mainFolder, true);
                    SetCheckBox(i, true);
                    i++;
                }

            }
            //Завершение
            SaveOpen.settings.Options.LastBackup = DateTime.Now;//пометка о выполненнии Бэкапа
            this.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(delegate()
                {
                    State = 0;
                    //Закрытие программы
                    if (SaveOpen.settings.Options.CloseAfterBackup && lErrors.Count == 0)
                    {
                        this.Owner.Close();
                    }
                    else
                        MessageBox.Show("Process finished", "Finish", MessageBoxButton.OK, MessageBoxImage.Information);
                }));

            
        }


        #region buttons

        /// <summary>
        /// Start button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void bStart_Click(object sender, RoutedEventArgs e)
        {
            State = 1;
            //
            ClearCheckBoxListStates();
            //start
            SaveBackup(FolderForBackup, LFiles);
        }

        /// <summary>
        /// Stop button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bStop_Click(object sender, RoutedEventArgs e)
        {
            //stop
            thread.Abort();
            //
            State = 2;

            tTimeOfWork.Stop();
        }

        #endregion buttons



    }
}
