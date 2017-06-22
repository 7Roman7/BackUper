using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.TaskScheduler;
using System.Windows;

namespace BackUper.Utilities
{
    public static class Reminder
    {
        private static string REMINDER_NAME = "BackUper_AutoStart";
        private static string exePath = Environment.CurrentDirectory + "\\" + "BackUper.exe";
        public static bool IsCreated { get { return TaskService.Instance.AllTasks.Count(x => x.Name == REMINDER_NAME) != 0; } }

        public static bool Create()
        {
            using (TaskService ts = new TaskService())
            {
                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Start program Backuper for remind about Backup.";
                // Create a trigger that will fire the task at this time every other day
                td.Triggers.Add(new DailyTrigger { DaysInterval = 1, StartBoundary= DateTime.Now.Add(new TimeSpan(1)) });
                // Create an action that will launch Notepad whenever the trigger fires
                td.Actions.Add(new ExecAction(exePath, SaveOpen.SETTINGS_FILENAME, null));

                // Register the task in the root folder
                ts.RootFolder.RegisterTaskDefinition(REMINDER_NAME, td);
            }
            return true;
        }
        public static bool Remove()
        {
            using (TaskService ts = new TaskService())
            {
                // Remove the task
                ts.RootFolder.DeleteTask(REMINDER_NAME);
            }
            return true;
        }
        public static void ChangeState()
        {
            if (IsCreated)
            {
                if (Remove()) 
                    MessageBox.Show(String.Format("Task is Removed"), "Reminder operation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (Create())
            {
                MessageBox.Show(String.Format("Task is Created"), "Reminder operation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static string GetNextExecuteText()
        {
            if (IsCreated)
                return "Delete";
            else
                return "Create";
        }



    }
}
