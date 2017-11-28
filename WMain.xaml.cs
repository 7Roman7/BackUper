using BackUper.Model;
using BackUper.Utilities;
using BackUper.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BackUper
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class WMain : Window
    {
        /// <summary>
        /// Основной список объектов
        /// </summary>
        public List<FileItem> lFiles;

        CommandBinding commandPlay = new CommandBinding(ApplicationCommands.Delete);

        /// <summary>
        /// Хранилище настроек
        /// </summary>
        public Settings SettingsCurrent
        {
            get { return SaveOpen.settings; }
            set { SaveOpen.settings = value; }
        }



        /// <summary>
        /// Инициализация
        /// </summary>
        public WMain()
        {
            InitializeComponent();
            DataContext = this;
            
        }

        /// <summary>
        /// Загрузка окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //SetSettings(SaveOpen.settings);
            LoadSettings();
            // устанавливаем метод, который будет выполняться при вызове команды
            //commandBinding.Executed += ;
            // добавляем привязку к коллекции привязок элемента Button
            //helpButton.CommandBindings.Add(commandBinding);
        }

        /// <summary>
        /// Закрытие окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }
        #region Команды

        #region Меню

        /// <summary>
        /// Выход
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MIExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Настройки


        /// <summary>
        /// Загрузка настроек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadSettings_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();

            var wMessage = new WMessage("Settings was loaded", "Settings", 30);
            wMessage.ShowDialog();
        }
        /// <summary>
        /// Сохранение настроек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (SaveSettings())
            {
                (new WMessage("Settings was saved", "Settings", 30)).ShowDialog();
            }
            else
                (new WMessage("Settings was not saved", "Settings Error save", 30)).ShowDialog();
        }

        /// <summary>
        /// Загрузка настроек
        /// </summary>
        private void LoadSettings()
        {
            var settings = SaveOpen.LoadSettings();

            lFiles = settings.lFiles;
            tbBackupDirectory.Text = settings.directoryForBackup;
            miDoZip.IsChecked = settings.Options.DoZip;
            miCloseAfterBackup.IsChecked = settings.Options.CloseAfterBackup;

            SetSettings(settings);

            Refresh();
        }

        /// <summary>
        /// Применение настроек данной формы
        /// </summary>
        /// <param name="s"></param>
        private void SetSettings(Settings s)
        {
            SettingsCurrent.wsMain.SetValueToWindow(this);
            cbReimderIsCreated.IsChecked = Reminder.IsCreated;
        }

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        private bool SaveSettings()
        {
            FillSettings();
            return SaveOpen.SaveSettings();
        }

        public Settings FillSettings()
        {
            SaveOpen.settings.directoryForBackup = tbBackupDirectory.Text;
            SaveOpen.settings.lFiles = lFiles;
            SaveOpen.settings.Options.DoZip = miDoZip.IsChecked;
            SaveOpen.settings.Options.CloseAfterBackup = miCloseAfterBackup.IsChecked;
            SaveOpen.settings.wsMain = new Settings.WindowSettings(this);

            return SaveOpen.settings;
        }

        #endregion Настройки

        private void miAdditionalSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// О Программе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            var wAbout = new WAbout();
            wAbout.ShowDialog();
        }

        /// <summary>
        /// Инфо
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Для выделение нескольких объектов (файлов папок) можно использовать клавиши Ctrl, Shift.",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion Меню

        #region Действия с элементами
        private void bAdd_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new WSelectFileItem();
            dlg.MultiSelect = true;
            if (dlg.ShowDialog() == true)
            {
                foreach (var s in dlg.ResultPaths)
                {
                    var f = new FileItem(s);

                    if (!f.Exist)
                    {
                        MessageBox.Show(String.Format("Объект по указанному адресу ({0}) не обнаружен", f.PathString),
                            "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    lFiles.Add(f);
                }
                Refresh();
                dgFileItems.SelectedIndex = lFiles.Count - 1;
            }
        }

        /// <summary>
        /// Удаление объектов. Возможно группой.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bDelete_Click(object sender, RoutedEventArgs e)
        {

            foreach(FileItem fi in dgFileItems.SelectedItems)
            {
                lFiles.Remove(fi);
            }
            Refresh();
        }

        #region Перемещения

        private void bMoveDown_Click(object sender, RoutedEventArgs e)
        {
            int si = dgFileItems.SelectedIndex;
            if (si >= 0 && si < dgFileItems.Items.Count - 1)
            {
                lFiles.Reverse(si, 2);
                Refresh();
                dgFileItems.SelectedIndex = si + 1;
            }
        }

        private void bMoveUp_Click(object sender, RoutedEventArgs e)
        {
            int si = dgFileItems.SelectedIndex;
            if (si >= 0 + 1 && si < dgFileItems.Items.Count)
            {
                lFiles.Reverse(si - 1, 2);
                Refresh();
                dgFileItems.SelectedIndex = si - 1;
            }
        }


        #endregion Перемещения

        #endregion Действия с элементами

        /// <summary>
        /// Выбор директории для бэкапов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bSelectBackupDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new WSelectFileItem();
            dlg.ShowFiles = false;
            if (dlg.ShowDialog() == true)
            {
                var f = new FileItem(dlg.ResultPathString);

                if (!f.Exist)
                {
                    MessageBox.Show("Объект по указанному адресу не обнаружен", "Ошибка");
                    return;
                }

                tbBackupDirectory.Text = f.PathString;
            }
        }

        /// <summary>
        /// Создание Бэкапа кнопка
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bDo_Copy_Click(object sender, RoutedEventArgs e)
        {
            DoBackup(tbBackupDirectory.Text, lFiles);
        }

        /// <summary>
        /// Немедленный старт бэкапа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bStartBackup_Click(object sender, RoutedEventArgs e)
        {
            DoBackup(tbBackupDirectory.Text, lFiles, true);
        }
        #endregion Команды

        #region По таблице
        /// <summary>
        /// Обновление таблицы
        /// </summary>
        private void Refresh()
        {
            dgFileItems.ItemsSource = null;
            dgFileItems.ItemsSource = lFiles;
            /*
            lvFileItems.Items.Clear();
            
            foreach (var i in lFiles)
            {
                lvFileItems.Items.Add(i.name);

            }*/

        }

        /// <summary>
        /// Выбор элемента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvFileItems_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (dgFileItems.SelectedIndex >= 0 && dgFileItems.SelectedIndex < dgFileItems.Items.Count)
            {
                bDelete.IsEnabled = true;
            }
            else
            {
                bDelete.IsEnabled = false;
            }
        }

        #endregion По таблице

        #region Another methods
        /// <summary>
        /// Проверка данных для копирования
        /// </summary>
        /// <param name="lFiles"></param>
        /// <returns>Состояние об успехе/провале</returns>
        public bool CheckAndConvertListFiles(List<FileItem> lFiles)
        {
            for (int i = 0; i < lFiles.Count; i++)
                for (int j = i + 1; j < lFiles.Count; j++)
                {
                    if (lFiles[i].PathString == lFiles[j].PathString)
                    {
                        MessageBox.Show(String.Format("Обнаружен повторяющиеся адреса ({0}): {1}, {2}", lFiles[i].PathString, lFiles[i].Name, lFiles[j].Name),
                            "Конфликт адресов", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        dgFileItems.SelectedIndex = i;
                        dgFileItems.Focus();
                        return false;
                    }

                    if (lFiles[i].Name == lFiles[j].Name && lFiles[i].GetName() == lFiles[j].GetName())
                    {
                        MessageBox.Show(String.Format("Detected conflict ({0} result name {1}) and ({2} result name {3}).\r\n Both have Name - {4}. Change it.",
                            lFiles[i].PathString, lFiles[i].GetName(),
                            lFiles[j].PathString, lFiles[j].GetName(), 
                            lFiles[i].Name),
                            "Конфликт адресов", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        dgFileItems.SelectedIndex = i;
                        dgFileItems.Focus();
                        return false;
                    }

                }

            return true;
        }

        /// <summary>
        /// Создание Бэкапа
        /// </summary>
        /// <param name="directoryForBackup">Место для копирования</param>
        /// <param name="lFiles">Копирование</param>
        private void DoBackup(string directoryForBackup, List<FileItem> lFiles, bool startImmediately = false)
        {
            FillSettings();
            if (CheckAndConvertListFiles(lFiles))
            {
                var wProcess = new WProcess();
                wProcess.Owner = this;
                SettingsCurrent.wsProcess.SetValueToWindow(wProcess);
                wProcess.FolderForBackup = directoryForBackup;
                wProcess.LFiles = lFiles;
                if (startImmediately) wProcess.Start_Click(null, null);
                wProcess.ShowDialog();
            }
        }


        #endregion Another methods

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Reminder.ChangeState();

        }

        private void cbReimderIsCreated_Click(object sender, RoutedEventArgs e)
        {
            Reminder.ChangeState();
        }

        private void MIGetSize_Click(object sender, RoutedEventArgs e)
        {
            FileItem item = (dgFileItems.SelectedItem as FileItem);
            item.GetSize();
        }

        private void miGetFullSize_Click(object sender, RoutedEventArgs e)
        {
            long fullSize = 0;
            foreach (var f in lFiles)
            {
                fullSize += f.GetSize();
            }

            MessageBox.Show(SaveOpen.SizeToString(fullSize, false), "Total size", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
