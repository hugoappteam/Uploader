using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Parse;


namespace Uploader
{
    class Program
    {

            static void Main(string[] args)
            {

                ParseClient.Initialize("pLIB1tDh2NBfOCWCFRhXIXehOaGFjLJmkMbYWN6D", "r4TzmlsgX3LM8AfWAFzdEJwH9Fw1hXN3vwNZsXin");

                if (AlreadyRunning())
                {
                    return;
                }

                //Ordner für Log-Dateien anlegen
                if (!Directory.Exists("../UploadLog"))
                {
                    Directory.CreateDirectory("../UploadLog");
                }
                //Log
                DateTime current = DateTime.Now;
                using (StreamWriter Writer = new StreamWriter(@"../UploadLog/log_"+ current.ToShortDateString()+".txt", true))
                {
                    Writer.WriteLine("Änderungen erkannt und Uploader.exe ausgeführt."+ " " + current.TimeOfDay);

                }
                HTMLRead.Structure();
                Console.ReadLine();
            
            }

            
            
            //prüfen ob Uploader bereits läuft um zu vermeiden, dass zwei Instazen aufgerufen werden
            private static bool AlreadyRunning()
            {
                Process current = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(current.ProcessName);
                foreach (Process process in processes)
                {
                    if (process.Id != current.Id)
                    {
                        if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == current.MainModule.FileName)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
    }
}
