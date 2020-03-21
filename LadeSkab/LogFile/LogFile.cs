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
        public string path { get; }
        public string fileName { get; }
        public string LastMessage { get; private set; } //This is both for functionality and testing purposes

        public StreamWriter sw { get; set; }

        public LogFile(string filename)
        {
            fileName = filename;
            path = Directory.GetCurrentDirectory() + @"\" + fileName;
        }
        

        public void CreateFile()
        {
            sw = File.CreateText(path);
            LastMessage = "Logging of door openings and closings";
            sw.WriteLine(LastMessage);
            sw.Close();
        }

        public void DeleteFile()
        {
            if(FileExist())
                File.Delete(path);
        }

        public bool FileExist()
        {
            return File.Exists(path);
        }

        public void AppendTextToFile(string text)
        {
            sw = File.AppendText(path);
            sw.WriteLine(text);
            sw.Close();
        }

        public void LogDoorLocked(int id)
        {
            if(!FileExist())
                CreateFile();
            LastMessage = $"ID: {id} - Locked door at: {DateTime.Now.ToString()}";
            AppendTextToFile(LastMessage);
        }

        public void LogDoorUnlocked(int id)
        {
            if (!FileExist())
                CreateFile();
            LastMessage = $"ID: {id} - Unlocked door at: {DateTime.Now.ToString()}";
            AppendTextToFile($"ID: {id} - Unlocked door at: {DateTime.Now.ToString()}");
        }
    }
}
