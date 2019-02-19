using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Oblivion.Core
{
    public class Language
    {
        private static HybridDictionary _values;
        internal static void Init() => _values = ReadFile(Path.Combine(Application.StartupPath, "extra/lang.ini"));

        internal static void Reload()
        {
            _values.Clear();
            Init();
        }
        internal static HybridDictionary ReadFile(string path)
        {
            var strArray = File.ReadAllLines(path);
            var dictionary = new HybridDictionary();
            foreach (var strings in from str in strArray
                where str.Length != 0 && str.Contains("=") && str.Substring(0, 1) != "#" && str.Substring(0, 1) != "["
                select str.Split('='))
                dictionary.Add(strings[0], strings[1]);
            return dictionary;
        }

        internal static string GetValue(string value)
        {
            if (!_values.Contains(value))
            {
                Logging.WriteLine($"Lang var {value} not found.");
                return "Error";
            }
            return _values[value] as string;
        }

        internal static string GetValueWithVar(string value, string var)
        {
            if (!_values.Contains(value))
            {
                Logging.WriteLine($"Lang var {value} not found.");
                return "Error";
            }
            var result = (string) _values[value];
            result = result.Replace("{var}", var);
            return result;
        }

        internal static string GetValueWithVar(string value, List<string> vars)
        {
            if (!_values.Contains(value))
            {
                Logging.WriteLine($"Lang var {value} not found.");
                return "Error";
            }
            var i = 0;
            var result = (string) _values[value];
            foreach (var var in vars)
            {
                i++;
                result = result.Replace("{var" + i + "}", var);
            }
            return result;
        }
    }
}