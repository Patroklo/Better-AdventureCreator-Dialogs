using System;
using System.Collections.Generic;
using System.Linq;

namespace Dialogs
{


    public static class Extensions
    {
        public static List<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }

    public class FileManager
    {

        public static string dialogDirectory = "Assets/AdventureCreatorDialogs/Dialogs/";
        protected static List<string> AllFiles = new List<string>();

        public static List<string> LoadFiles()
        {
            if (AllFiles.Count == 0)
            {
                string[] fileList = System.IO.Directory.GetFiles(dialogDirectory, "*.bin", System.IO.SearchOption.AllDirectories);

                fileList = fileList.Select(s => s.Replace(dialogDirectory, "")).ToArray();
                fileList = fileList.Select(s => s.Replace(".bin", "")).ToArray();

                AllFiles = fileList.ToList();

            }

            return AllFiles;
        }

        public static String LoadFile(string FileName)
        {
            return dialogDirectory + FileName + ".bin";
        }

        public static void ResetFiles()
        {
            AllFiles = new List<string>();
        }

    }
}
