using System;
using System.IO;

namespace NetFlowLibrary
{
    /// <summary>
    /// Класс логирования 
    /// </summary>
    /// <example>
    /// ...
    ///  Logs.Write("Все очень плохо!");
    /// ...
    /// </example>
    public class Logs
    {
        public static void Write(string Message)
        {
            /* var path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location);
             File.AppendAllText(path + @"\error.log", "->" + DateTime.Now.ToString() + " " + Message+Environment.NewLine);*/
            Write("error", "->" + DateTime.Now.ToString() + " " + Message + Environment.NewLine);
        }

        public static void Write(Exception Ex)
        {
            Write(Ex.GetType().ToString() + " " + Ex.Message + Environment.NewLine + "\t" + Ex.StackTrace);
        }

        public static void Write(string FileName, string Message)
        {
            var path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location);
            File.AppendAllText(path + @"\" + FileName + "_" + DateTime.Now.ToString("yyyy_MM_dd") + ".log", Message);
        }

    }
}
