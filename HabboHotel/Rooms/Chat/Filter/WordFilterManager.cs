#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Filter
{
    public sealed class WordFilterManager
    {
        private readonly List<WordFilter> _filteredWords;


        public WordFilterManager()
        {
            _filteredWords = new List<WordFilter>();
        }

        public void Init()
        {
            if (_filteredWords.Count > 0)
                _filteredWords.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `wordfilter`");
                var Data = dbClient.getTable();

                if (Data == null) return;
                foreach (DataRow Row in Data.Rows)
                    _filteredWords.Add(new WordFilter(Convert.ToString(Row["word"]),
                        Convert.ToString(Row["replacement"]), OblivionServer.EnumToBool(Row["strict"].ToString()),
                        OblivionServer.EnumToBool(Row["bannable"].ToString())));
            }
        }

        public string CheckMessage(string Message, bool html = false)
        {
//            if (html)
//                Message = HttpUtility.HtmlDecode(Message);
            foreach (var Filter in _filteredWords.ToList())
                if (Message.ToLower().Contains(Filter.Word) && Filter.IsStrict || Message == Filter.Word)
                {
                    Message = Regex.Replace(Message, Filter.Word, Filter.Replacement, RegexOptions.IgnoreCase);
                }
                else if (Message.ToLower().Contains(Filter.Word) && !Filter.IsStrict || Message == Filter.Word)
                {
                    var Words = Message.Split(' ');

                    Message = "";
                    foreach (var Word in Words.ToList())
                        if (Word.ToLower() == Filter.Word)
                            Message += Filter.Replacement + " ";
                        else
                            Message += Word + " ";
                }

            return Message.TrimEnd(' ');
        }


        public bool CheckBannedWords(string Message)
        {
            Message = Message.Replace(" ", "").Replace(".", "z").Replace("_", "").ToLower();

            return
                _filteredWords.ToList().Where(Filter => Filter.IsBannable).Any(Filter => Message.Contains(Filter.Word));
        }


        public bool IsFiltered(string Message, bool html = false)
        {
            if (html)
                Message = HttpUtility.HtmlDecode(Message.ToLower());

            var data = Encoding.GetEncoding("utf-8").GetBytes(Message);

            Message = Encoding.UTF8.GetString(data);
            if (Message.Contains("s2.vc") || Message.Contains("abre.ai"))
                return true;

            Message = Regex.Replace(Message, "[àâäàáâãäåÀÁÂÃÄÅ@4ª∂]", "a");
            Message = Regex.Replace(Message, "[ß8]", "b");
            Message = Regex.Replace(Message, "[©çÇ¢]", "c");
            Message = Regex.Replace(Message, "[Ð]", "d");
            Message = Regex.Replace(Message, "[éèëêðÉÈËÊ£3∑]", "e");
            Message = Regex.Replace(Message, "[ìíîïÌÍÎÏ1]", "i");
            Message = Regex.Replace(Message, "[ñÑπ]", "n");
            Message = Regex.Replace(Message, "[òóôõöøÒÓÔÕÖØ0|ºΩ]", "o");
            Message = Regex.Replace(Message, "[®]", "r");
            Message = Regex.Replace(Message, "[šŠ$5∫§2]", "s");
            Message = Regex.Replace(Message, "[ùúûüµÙÚÛÜ]", "u");
            Message = Regex.Replace(Message, "[ÿŸ¥]", "y");
            Message = Regex.Replace(Message, "[žŽ]", "z");
            Message = Message.Replace("dot", ".");
            Message = Regex.Replace(Message, "[ ',-_¹²³.?´` ƒ()]", "");
            Message = Message.Replace("™", "TM");
            Message = Message.Replace("æ", "ae");
            Message = Message.Replace("∞", "oo");
//            Console.WriteLine(Message);

            return _filteredWords.ToList().Any(word => Message.ToLower().Contains(word.Word.ToLower()));
        }
    }
}