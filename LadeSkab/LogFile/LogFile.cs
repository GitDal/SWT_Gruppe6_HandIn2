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

        public LogFile(string filename)
        {
            fileName = filename;
            path = Directory.GetCurrentDirectory() + @"\" + fileName;
        }

        public void LogDoorLocked(int id)
        {

            if (!File.Exists(path))
            {
                using (StreamWriter logWriterInit = File.CreateText(path))
                {
                    logWriterInit.WriteLine($"Logging of door openings and closings");
                    logWriterInit.WriteLine($"ID: {id} - Locked door at: {DateTime.Now.ToString()}");
                }
            }
            else
            {
                using (StreamWriter logWriter = File.AppendText(path))
                {
                    logWriter.WriteLine($"ID: {id} - Locked door at: {DateTime.Now.ToString()}");
                }
            }

        }

        public void LogDoorUnlocked(int id)
        {
            if (!File.Exists(path))
            {
                using (StreamWriter logWriterInit = File.CreateText(path))
                {
                    logWriterInit.WriteLine($"Logging of door openings and closings");
                    logWriterInit.WriteLine($"ID: {id} - Unlocked door at: {DateTime.Now.ToString()}");
                }
            }
            else
            {
                using (StreamWriter logWriter = File.AppendText(path))
                {
                    logWriter.WriteLine($"ID: {id} - Unlocked door at: {DateTime.Now.ToString()}");
                }
            }
        }
    }
}
