using BackUper.Model;
using BackUper.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace BackUper.Utilities
{
    public static class SaveOpen
    {
        /// <summary>
        /// Адрес файла с настройками
        /// </summary>
        public static string SETTINGS_FILENAME = String.Format("{0}\\{1}", Environment.CurrentDirectory, "BackUper_settings.xml"); // "C:\\BackUper_settings.txt";

        public static Settings settings;

        public static string CurrentDirectory {get; set;}

        public static Settings FillSettings(string directoryForBackup, List<FileItem> lFiles, WMain wMain)
        {
            settings.directoryForBackup = directoryForBackup;
            settings.lFiles = lFiles;

            settings.wsMain.left = wMain.Left;
            settings.wsMain.top = wMain.Top;
            settings.wsMain.height = wMain.Height;
            settings.wsMain.width = wMain.Width;

            return settings;
        }

        static SaveOpen()
        {
            CurrentDirectory = "";
        }



        #region XML сериализации

        /// <summary>
        /// Запись настроек в файл
        /// </summary>
        /// <returns>Успешность записи</returns>
        public static bool SaveSettings()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Settings));
                using (TextWriter writer = new StreamWriter(SETTINGS_FILENAME))
                {
                    ser.Serialize(writer, settings);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Чтение настроек из файла
        /// </summary>
        public static Settings LoadSettings()
        {
            if (File.Exists(SETTINGS_FILENAME))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(Settings));
                    using (TextReader reader = new StreamReader(SETTINGS_FILENAME))
                    {
                        settings = ser.Deserialize(reader) as Settings;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    settings = new Settings();//дефолтные настройки
                }
            }
            else 
            {
                settings = new Settings();//дефолтные настройки
            }

            return settings;
        }

        #endregion XML сериализации


        /// <summary>
        /// Создание копии файла
        /// </summary>
        /// <param name="beginDir"></param>
        /// <param name="endDir"></param>
        /// <returns>true if success</returns>
        public static bool DoCopy(string beginDir, string endDir)
        {
            CurrentDirectory = beginDir;//для возможности отображения
            try
            {
                var f = new FileItem(beginDir);
                if (f.IsFile)//File
                    File.Copy(f.PathString, endDir + "\\" + f.Name);
                else
                {
                    if (f.IsFolder)// Directory
                    {
                        DirectoryInfo dirInf = new DirectoryInfo(beginDir);

                        //Создание Директорию
                        if (Directory.Exists(endDir + "\\" + dirInf.Name) != true)
                        {
                            Directory.CreateDirectory(endDir + "\\" + dirInf.Name);
                        }

                        //Создаем вложенные директории
                        foreach (var dir in dirInf.GetDirectories())
                            DoCopy(dir.FullName, endDir + "\\" + dirInf.Name);

                        //Копируем вложенные файлы
                        foreach (var fIn in dirInf.GetFiles())
                            DoCopy(fIn.FullName, endDir + "\\" + dirInf.Name);
                    }
                    else if (f.IsDrive)
                    {
                        DirectoryInfo dirInf = new DirectoryInfo(beginDir);
                        var driveDirName = "Drive_" + dirInf.Name[0];//взятие первого символа
                        Directory.CreateDirectory(endDir + "\\" + driveDirName);

                        //Создаем вложенные директории
                        foreach (var dir in dirInf.GetDirectories())
                            DoCopy(dir.FullName, endDir + "\\" + driveDirName);

                        //Копируем вложенные файлы
                        foreach (var fIn in dirInf.GetFiles())
                            DoCopy(fIn.FullName, endDir + "\\" + driveDirName);
                    }
                    else
                        if (f.Exist == false)
                            return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Ошибка при копировании: {0}", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Возвращает размер объекта
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long GetSize(string path)
        {
            try
            {
                var f = new FileItem(path);

                if (f.IsFile)
                    return new FileInfo(path).Length;
                else
                    return GatDirectorySize(new DirectoryInfo(path));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }

        }
        /// <summary>
        /// Возвращает размер директории
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static long GatDirectorySize(DirectoryInfo d)
        {
            long Size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                Size += GatDirectorySize(di);
            }
            return (Size);
        }

        /// <summary>
        /// Преобразование в читабельную строку
        /// </summary>
        /// <param name="v">значение в байтах</param>
        /// <param name="getFull">полный или сокращенный вариант</param>
        /// <returns></returns>
        public static string SizeToString (long v, bool getFull = true)
        {
            string result = String.Empty;
            var vLength = (byte)Math.Log10(v) + 1;
            string vString = v.ToString();

            if (getFull)
            {
                for (int i = 1; i <= vLength; i++)
                {
                    result = vString[vLength - i] + result;

                    if ((i) % 3 == 0 && i != vLength)
                    {
                        result = "." + result;
                    }
                }

                result += " byte";
            }
            else
            {
                if (vLength > 12)
                    result = String.Format("> {0} {1}",
                        vString.Substring(0, vLength % 3),
                        "TB");
                else if(vLength > 9)
                    result = String.Format("> {0} {1}",
                        vString.Substring(0, vLength % 3),
                        "GB");
                else if (vLength > 6)
                    result = String.Format("> {0} {1}",
                        vString.Substring(0, vLength % 3),
                        "MB");
                else if (vLength > 3)
                    result = String.Format("> {0} {1}",
                        vString.Substring(0, vLength % 3),
                        "KB");
                else if (vLength > 0)
                    result = String.Format("> {0} {1}",
                        vString.Substring(0, vLength % 3),
                        "B");
                else 
                    result = String.Format("~ {0} {1}",
                        vString.Substring(0, 1),
                        "B");
            }
            return result;
        }
    }








}
