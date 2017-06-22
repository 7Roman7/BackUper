using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackUper.Model
{
    public class TParams
    {
        public string mainFolder;
        public List<FileItem> lFiles;
        public List<bool> Conflicts;


        public List<bool> CheckConflicts()
        {
            Conflicts = new List<bool>();
            for (int i = 0; i < lFiles.Count; i++)
                Conflicts.Add(false);
            for (int i = 0; i < lFiles.Count; i++)
            {
                for (int j = i + 1; j < lFiles.Count; j++)
                {
                    if (lFiles[i].GetName() == lFiles[j].GetName())
                    {
                        Conflicts[i] = true;
                        Conflicts[j] = true;
                    }
                }
            }
            return Conflicts;
        }


    }
}
