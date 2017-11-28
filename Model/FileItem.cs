using BackUper.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BackUper.Model
{
    [Serializable]
    public class FileItem : BaseObject, IDataErrorInfo
    {
        #region Типы объекта
        public enum KindItem : byte
        {
            unknown,
            file,
            folder,
            hardDrive,
            CDRom
        }
        #endregion Типы объекта

        #region Переменные хранилище
        private string name = "";
        private string pathString = String.Empty;        //хранилище пути
        private bool exist = false;
        private KindItem kind = 0;
        private long size = -1;
        #endregion Переменные хранилище



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

        /// <summary>
        /// Существование файла
        /// </summary>
        public bool Exist {get {return exist;}}

        /// <summary>
        /// Является файлом. true - файл,
        /// </summary>
        public bool IsFile { get { return kind == KindItem.file ? true : false; } }
        /// <summary>
        /// Является папкой. true - папка,
        /// </summary>
        public bool IsFolder { get { return kind == KindItem.folder ? true : false; } }
        /// <summary>
        /// Является жестким диском. true - ЖДиск,
        /// </summary>
        public bool IsDrive { get { return kind == KindItem.hardDrive ? true : false; } }

        /// <summary>
        /// Является диском. true - Диск,
        /// </summary>
        public bool IsDisk { get { return kind == KindItem.CDRom ? true : false; } }

        /// <summary>
        /// Тип объъекта: 0-? 1-файл, 2-директория, 3-диск, 
        /// </summary>
        public KindItem Kind { get { return kind; } }

        /// <summary>
        /// Text name Kind
        /// </summary>
        public string KindName
        {
            get
            {
                switch (kind)
                {
                    case KindItem.file: return "File";
                    case KindItem.folder: return "Folder";
                    case KindItem.hardDrive: return "Drive";
                    case KindItem.CDRom: return "CDRom";

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
                case KindItem.file: return "/Images/Item/file_32.png";
                case KindItem.folder: return "/Images/Item/folder_32.png";
                case KindItem.hardDrive: return "/Images/Item/drive_32.png";
                case KindItem.CDRom: return "/Images/Item/disc_32.png";

                default: return "/Images/Item/unknown_32.png";
            }     
        } }

        [XmlIgnoreAttribute]
        public long Size
        {
            get { return size; }
            set
            {
                size = value;
                OnPropertyChanged("Size");
                OnPropertyChanged("SizeStr");
            }
        }

        public string SizeStr
        {
            get
            {
                if (size == -1)
                    return "?";
                else 
                return SaveOpen.SizeToString(size, false);
            }

        }

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

        /// <summary>
        /// Самоопределение размера
        /// </summary>
        /// <returns></returns>
        public long GetSize()
        {
            Size = SaveOpen.GetSize(PathString);
            return Size;
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Определение типа
        /// </summary>
        /// <returns></returns>
        public KindItem DetectType()
        {
            kind = 0;//not exist
            //File
            if (System.IO.File.Exists(PathString))
                kind = KindItem.file;
            else if (System.IO.Directory.Exists(PathString))
            {
                if (Path.GetPathRoot(PathString) != PathString)
                    kind = KindItem.folder;//folder
                else
                {
                    DriveInfo d = new DriveInfo(PathString);
                    if (d.DriveType == DriveType.Fixed)
                        kind = KindItem.hardDrive;//HardDrive
                }
            }
            else
            {//Drives
                DriveInfo d = new DriveInfo(PathString);
                if (d.DriveType == DriveType.CDRom)
                    kind = KindItem.CDRom;//HardDrive
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


    }
}
