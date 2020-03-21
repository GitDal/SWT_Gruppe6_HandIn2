using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public class LogFile : ILogger
    {
        private string path;
        private string fileName;

        public StreamWriter sw { get; set; }

        public LogFile(string filename)
        {
            fileName = filename;
            path = Directory.GetCurrentDirectory() + @"\" + fileName;
        }
        

        public void CreateFile()
        {
            sw = File.CreateText(path);
            sw.WriteLine("Logging of door openings and closings");
            sw.Close();
        }

        public void AppendTextToFile(string text)
        {
            sw = File.AppendText(path);
            sw.WriteLine(text);
            sw.Close();
        }

        public void LogDoorLocked(int id)
        {
            if(!File.Exists(path))
                CreateFile();
            AppendTextToFile($"ID: {id} - Locked door at: {DateTime.Now.ToString()}");
        }

        public void LogDoorUnlocked(int id)
        {
            if (!File.Exists(path))
                CreateFile();
            AppendTextToFile($"ID: {id} - Unlocked door at: {DateTime.Now.ToString()}");
        }
    }
}
