using BackUper.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для SelectFileItem.xaml
    /// </summary>
    public partial class WSelectFileItem : Window
    {
        /// <summary>
        /// результирующий адрес
        /// </summary>
        [DefaultValue("")]
        public string ResultPathString
        {
            get {
                    return (ResultPaths.Count > 0) ? ResultPaths[0] : String.Empty; 
                }
            set
            {
                var r = new List<string>();
                r.Add(value);
                ResultPaths = r;
            }
        }

        /// <summary>
        /// результирующий адрес
        /// </summary>
        [DefaultValue("")]
        public List<string> ResultPaths
        {
            get { return resultPaths; }
            set
            {
                resultPaths = value;
                tbResultPath.Clear();

                for (int i = 0; i < value.Count; i++ )
                {
                    tbResultPath.Text += value[i];
                    if (i + 1 < value.Count)
                        tbResultPath.Text += " + ";
                }
            }
        }
        private List<string> resultPaths = new List<string>();

        /// <summary>
        /// Текущий каталог
        /// </summary>
        [DefaultValue("")]
        public string CurrentPath 
        { 
            get { return currentPath; } 
            set { 
                currentPath = value;

                tbCurrentPath.Text = value != "" ? value : "Computer";
            }
        }
        private string currentPath = "";

        public string StartFolder { get; set; }


        [DefaultValue(true)]
        public bool ShowDirectories { get; set; }

        [DefaultValue(true)]
        public bool ShowFiles { get; set; }

        [DefaultValue(false)]
        public bool MultiSelect 
        {
            get { return lbFileItems.SelectionMode == SelectionMode.Extended; }
            set { lbFileItems.SelectionMode = (value ? SelectionMode.Extended : SelectionMode.Single); }
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public WSelectFileItem()
        {
            InitializeComponent();

            ShowDirectories = true;
            ShowFiles = true;
            MultiSelect = false;
            // При вызове показывать Заполнить Листбокс, списком локальных дисков
            GetAddDrivesToListBox();
        }

        /// <summary>
        /// Отмена
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bCansel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Выбрать
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bSelect_Click(object sender, RoutedEventArgs e)
        {
            if (ResultPaths.Count > 0)
            {
                DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Need to select File or Folder", "Object not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        
        /// <summary>
        /// На уровыень выше
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bLvlUp_Click(object sender, RoutedEventArgs e)
        {
            int curPathLength = CurrentPath.Length;
            // Определяем количество слэшей в пути
            string pattern = "\\";
            int slashCount = new Regex(Regex.Escape(pattern)).Matches(CurrentPath).Count;
            // MessageBox.Show(amount.ToString()); C:/Work/folder - 2

            // C:/ Если слэш один и путь состоит из названия диска - Показываем диски
            if ((slashCount <= 1) && (curPathLength <= 3))
            {
                GetAddDrivesToListBox();
            }
            // C:/Work Если слэш один, но это верхняя папка на диске
            else if ((slashCount <= 1) && (curPathLength > 3))
            {
                // находим слэш и удаляем знаки до конца строки C:/Work -> C:/
                int slashZnak = CurrentPath.IndexOf('\\') + 1;
                CurrentPath = CurrentPath.Remove(slashZnak);

                GetItemsToListBox();
            }
            //C:/Work/folder если Слэшей больше одного
            else
            {
                // показываем путь отняв последний слэш C:/Work/folder -> C:/Work/folder
                int position = currentPath.LastIndexOf('\\'); // где в последний раз находили слэш - +1 
                CurrentPath = CurrentPath.Remove(position);
                GetItemsToListBox();
            }
        }

        /// <summary>
        /// на самый верхний уровень
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bLvlUpDrives_Click(object sender, RoutedEventArgs e)
        {
            GetAddDrivesToListBox();
        }

        /// <summary>
        /// Переход по введенному адресу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bMoveTo_Click(object sender, RoutedEventArgs e)
        {
            var f = new FileItem(tbCurrentPath.Text);
            MoveToFolder(f);
        }


        // Два раза кликаем для того что бы войти в каталог
        private void mouseDoubleClickOnListBox(object sender, MouseButtonEventArgs e)
        {
            if (lbFileItems.SelectedItem != null)
            {
                var f = (lbFileItems.SelectedItem as FileItem);
                MoveToFolder(f);
            }
        }

        // Заносим текущее выделение в Путь
        private void currentSelect(object sender, SelectionChangedEventArgs e)
        {
            resultPaths.Clear();
            if (lbFileItems.SelectedItems.Count > 0)
            {
                foreach (var s in lbFileItems.SelectedItems)
                    resultPaths.Add((s as FileItem).PathString);

                ResultPaths = resultPaths;
            }
        }
        #region Методы
        
        /// <summary>
        /// Выводим папки/файлы
        /// </summary>
        private void GetItemsToListBox()
        {
            List<string> l = new List<string>();
            
            if (ShowDirectories)
                l.AddRange(Directory.GetDirectories(CurrentPath));
            if (ShowFiles)
                l.AddRange(Directory.GetFiles(CurrentPath));

            var lF = new List<FileItem>();
            foreach (var s in l)
            {
                if (cbAll.IsChecked == false)//не показывать все
                {
                    var d = new DirectoryInfo(s);
                    if (d.Attributes.HasFlag(FileAttributes.Hidden))
                        continue;
                }
                
                lF.Add(new FileItem(s));
            }

            ShowItemsToListBox(lF);
        }

        /// <summary>
        /// Вывод локальных дисков
        /// </summary>
        private void GetAddDrivesToListBox()
        {
            CurrentPath = String.Empty;

            string[] myDrives = Environment.GetLogicalDrives();
            var lF = new List<FileItem>();
            foreach(var s in myDrives)
            {
                lF.Add(new FileItem(s));
            }
            ShowItemsToListBox(lF);
        }

        /// <summary>
        /// Отображение списка
        /// </summary>
        /// <param name="lFiles"></param>
        private void ShowItemsToListBox(List<FileItem> lFiles)
        {
            lbFileItems.Items.Clear();
            foreach (var f in lFiles)
            {
                lbFileItems.Items.Add(f);
            }
        }

        /// <summary>
        /// Переход в папку
        /// </summary>
        /// <param name="f"></param>
        private void MoveToFolder(FileItem f)
        {
            if (f.Exist)
            {
                if (!f.IsFile)
                {
                    CurrentPath = f.PathString;
                    // Заполняем листбокс найденными каталогами
                    GetItemsToListBox();
                    // Очистка текущего выбора
                    ResultPaths = new List<string>();
                }
            }
        }

        #endregion


        #region Автоматизации
        private void cbAll_Click(object sender, RoutedEventArgs e)
        {
            var f = new FileItem(tbCurrentPath.Text);
            MoveToFolder(f);
        }

        private void tbCurrentPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*if (tbCurrentPath.Text != CurrentPath)
            {
                bMoveTo.IsEnabled = true;
            }
            else
            {
                bMoveTo.IsEnabled = false;
            }*/
        }

        #endregion Автоматизации

    }
}
