#region

using System;
using System.Text.RegularExpressions;

#endregion

namespace Oblivion.Utilities
{
    internal static class StringCharFilter
    {
        /// <summary>
        ///     Escapes the characters used for injecting special chars from a user input.
        /// </summary>
        /// <param name="str">The string/text to escape.</param>
        /// <param name="allowBreaks">Allow line breaks to be used (\r\n).</param>
        /// <returns></returns>
        public static string Escape(string str, bool allowBreaks = false, bool html = true)
        {
            str = str.Trim();
            str = str.Replace(Convert.ToChar(1), ' ');
            str = str.Replace(Convert.ToChar(2), ' ');
            str = str.Replace(Convert.ToChar(3), ' ');
            str = str.Replace(Convert.ToChar(9), ' ');

            if (!allowBreaks)
            {
                str = str.Replace(Convert.ToChar(10), ' ');
                str = str.Replace(Convert.ToChar(13), ' ');
            }
            if (html)
                str = Regex.Replace(str, "<.*?>", string.Empty);

            return str;
        }


        public static string EscapeJSONString(string str) => str.Replace("\"", "\\\"");
    }
}