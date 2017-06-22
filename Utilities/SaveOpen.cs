using BackUper.Model;
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
        public static string SETTINGS_FILENAME = "C:\\BackUper_settings.txt";

        public static Settings settings;
        

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




        #region XML сериализации

        /// <summary>
        /// Запись настроек в файл
        /// </summary>
        public static void SaveSettings()
        {
            try
                {
                XmlSerializer ser = new XmlSerializer(typeof(Settings));
                using (TextWriter writer = new StreamWriter(SETTINGS_FILENAME))
                {
                    ser.Serialize(writer, settings);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }








}
