using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TibberSubscription
{
    internal class Logger
    {
        readonly static string LogFile = Directory.GetCurrentDirectory() + @"\Log\Log.txt";
        readonly static string LogPath = Directory.GetCurrentDirectory() + @"\Log";
        readonly static int MaxFiles = 3;
        readonly static int MaxSize = 1024 * 1024 * 10;

        public Logger()
        {
            if(!Directory.Exists(Directory.GetCurrentDirectory() + @"\Log"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Log");
            }
        }

        public void LogEntry(string message, string category)
        {
            lock(LogFile)
            {
                CheckFileSize(LogFile);
                File.AppendAllText(
                    LogFile, 
                    "[" + DateTime.Now + "]" + "[" + category + "]" + message + Environment.NewLine, 
                    Encoding.UTF8);
            }
        }

        private void CheckFileSize(string logFile)
        {
            try
            {
                var size = new FileInfo(LogFile).Length;
                var lognametmp = Path.Combine(LogPath, Path.GetFileNameWithoutExtension(LogFile));
                if (size > MaxSize)
                {
                    string[] FileList = Directory.GetFiles(LogPath, "Log*.txt", SearchOption.TopDirectoryOnly);
                    if(FileList.Length > 0)
                    {
                        // only take files like logfilename.log and logfilename.0.log, so there also can be a maximum of 10 additional rolled files (0..9)
                        var rolledLogFileList = FileList.Where(fileName => fileName.Length == (logFile.Length + 2)).ToArray();
                        Array.Sort(rolledLogFileList, 0, rolledLogFileList.Length);
                        if (rolledLogFileList.Length >= MaxFiles)
                        {
                            File.Delete(rolledLogFileList[MaxFiles - 1]);
                            var list = rolledLogFileList.ToList();
                            list.RemoveAt(MaxFiles - 1);
                            rolledLogFileList = list.ToArray();
                        }
                        // move remaining rolled files
                        for (int i = rolledLogFileList.Length; i > 0; --i)
                            File.Move(rolledLogFileList[i - 1], lognametmp + "." + i + Path.GetExtension(logFile));
                        var targetPath = lognametmp + ".0" + Path.GetExtension(logFile);
                        // move original file
                        File.Move(logFile, targetPath);
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
    }
}
