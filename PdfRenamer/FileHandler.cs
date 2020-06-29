using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace PdfRenamer
{
    internal class FileHandler
    {
        internal Log Log;

        internal List<FileInfo> GetFileNames(string path)
        {
            FileInfo[] files = null;
            List<FileInfo> outfiles = null;
            try
            {
                if (Directory.Exists(path))
                {
                    files = new DirectoryInfo(path).GetFiles("*.pdf");
                    outfiles = files.Where(x => x.Exists && x?.Name.Contains(".pdf") == true).ToList();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message, ex.StackTrace);
            }
            return outfiles;
        }

        internal bool Move(FileInfo currentFileInfo, string outputFile)
        {
            bool moved = false;
            try
            {
                File.Move(currentFileInfo.FullName, outputFile);
                moved = true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.StackTrace);
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "Ошибка");
            }
            return moved;
        }

        internal string CreateTempFile(FileInfo currentFileInfo)
        {
            string tempFileFullName = Path.GetTempPath() + currentFileInfo.Name;

            try
            {
                File.Copy(currentFileInfo.FullName, tempFileFullName);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
            return tempFileFullName;
        }

        internal void Delete(string tempFileFullName)
        {
            try
            {
                FileUtil fileUtil = new FileUtil();
                foreach (var proc in fileUtil.WhoIsLocking(tempFileFullName))
                {
                    proc.Kill();
                }
                File.Delete(tempFileFullName);
            }
            catch (NullReferenceException nullex)
            {
                Log.WriteLine(nullex.Message, nullex.StackTrace);
                MessageBox.Show(nullex.Message, nullex.StackTrace);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message, ex.StackTrace);
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}