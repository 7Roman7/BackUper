using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BackUper.Model
{
    [Serializable]
    public class FileItem : IDataErrorInfo, INotifyPropertyChanged
    {
        #region Поля
        private string name = "";
        private string pathString = String.Empty;
        private bool exist = false;
        private byte kind = 0;
        #endregion Поля





        /// <summary>
        /// Наименование
        /// </summary>
        public string Name 
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        
        /// <summary>
        /// Путь
        /// </summary>
        public string PathString 
        {
            get { return pathString; }
            set { pathString = value; Analysis(); OnPropertyChanged("Name"); }
        }
        //хранилище пути

        /// <summary>
        /// Существование файла
        /// </summary>
        public bool Exist{get {return exist;}}

        /// <summary>
        /// Является файлом. true - файл,
        /// </summary>
        public bool IsFile { get { return kind == 1 ? true : false; } }
        /// <summary>
        /// Является папкой. true - файл,
        /// </summary>
        public bool IsFolder { get { return kind == 2 ? true : false; } }
        /// <summary>
        /// Является папкой. true - файл,
        /// </summary>
        public bool IsDrive { get { return kind == 3 ? true : false; } }

        /// <summary>
        /// Тип объъекта: 0-? 1-файл, 2-директория, 3-диск, 
        /// </summary>
        public byte Kind { get { return kind; } }


        /// <summary>
        /// Text name Kind
        /// </summary>
        public string KindName
        {
            get
            {
                switch (kind)
                {
                    case 1: return "File";
                    case 2: return "Folder";
                    case 3: return "Drive";

                    default: return "Unknown";
                }
            }
        }

        /// <summary>
        /// Отображаемое изображение
        /// </summary>
        public string Image { get {
            switch (kind)
            {
                case 1: return "Images/Item/file.png";
                case 2: return "Images/Item/folder.png";
                case 3: return "Images/Item/drive.png";

                default: return "Images/Item/unknown.png";
            }     
        } }

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public FileItem()
        {
        }
        /// <summary>
        /// Конструктор основной
        /// </summary>
        /// <param name="path"></param>
        public FileItem(string path) : this()
        {
            pathString = path;
            //Проверка на существование и определение типа

            Analysis();
        }

        /// <summary>
        /// Метод для формирования полей при создании или изменении пути
        /// </summary>
        private void Analysis()
        {
            if (DetectType() == 0)
                exist = false;
            else
            {
                exist = true;
                if (Name == String.Empty)
                    Name = GetName();
            }
        }


        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Определение типа
        /// </summary>
        /// <returns></returns>
        public byte DetectType()
        {
            kind = 0;//not exist

            //File
            if (System.IO.File.Exists(PathString))
                kind = 1;
            else
                if (System.IO.Directory.Exists(PathString))
                {
                
                    if (Path.GetPathRoot(PathString) != PathString)
                        kind = 2;//folder
                    else
                        kind = 3;//Drive
                } 

            return kind;
        }

        public string GetName()
        {
            return IsFile ? Path.GetFileName(PathString) : (new DirectoryInfo(PathString)).Name;
        }

        #region Члены IDataErrorInfo

        public string Error
        {
            get
            {
                string error = String.Empty;
                switch (Name)
                {
                    case "Name":
                        if ((Name.Length == 0))
                        {
                            error = "Название не должно быть пустым.";
                        }
                        break;
                }
                return error;
            }
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

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
