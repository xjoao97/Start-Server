using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Oblivion.Core
{
    public class BotConfig
    {
        private static List<string> _values;
        private static string _lastvalue;

        internal static void Init(string bot)
        {
            _values = ReadFile(Path.Combine(Application.StartupPath, "extra/bots/" + bot + ".ini"));
            Logging.WriteLine($"Loaded {bot} bot!");
        }

        internal static List<string> ReadFile(string path)
        {
            var strArray = File.ReadAllLines(path, Encoding.UTF8);
            Console.OutputEncoding = Encoding.UTF8;
            return
                strArray.Where(str => str.Length != 0 && str.Substring(0, 1) != "#" && str.Substring(0, 1) != "[")
                    .ToList();
        }

        internal static string GetRandomValue()
        {
            Random:
            var rnd = new Random();
            var randomEntry = _values[rnd.Next(0, _values.Count)];
            if (_lastvalue == randomEntry)
                goto Random;

            _lastvalue = randomEntry;
            return randomEntry;
        }
    }
}