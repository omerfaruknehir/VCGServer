using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Pastel;

namespace VCG_Library
{
    public static class Debug
    {
        public static bool IsFile { get; set; } = false;

        public static bool IsToOneFile { get; set; } = true;
        public static string LogPath { get; private set; } = "./Debug.log";
        public static string WarnPath { get; private set; } = "./Warn.log";
        public static string ErrPath { get; private set; } = "./Err.log";

        private static void WriteLine(object time, object line) => Console.WriteLine(("[" + time + "]\t").PastelBg(Color.FromArgb(100, 100, 100)) + line);
        private static void WriteLine(object time, object line, Color color) => Console.WriteLine((("[" + time + "]\t").PastelBg(Color.FromArgb(5, 25, 25)) + line).Pastel(color));

        public static void Log(object value)
        {
            Console.WriteLine(("[" + DateTime.Now + "]     ").Pastel(Color.FromArgb(250, 250, 250)) + value.ToString().Pastel(Color.FromArgb(175, 175, 175)));
        }

        public static void LogError(object value)
        {
            Console.WriteLine(("[" + DateTime.Now + "]     ").Pastel(Color.FromArgb(250, 200, 200)) + value.ToString().Pastel(Color.FromArgb(175, 150, 100)));
        }

        public static void LogWarning(object value)
        {
            Console.WriteLine(("[" + DateTime.Now + "]     ").Pastel(Color.FromArgb(250, 250, 175)) + value.ToString().Pastel(Color.FromArgb(175, 175, 100)));
        }
    }
}
