namespace Oblivion.HabboHotel.Rooms.Chat.Filter
{
    internal sealed class WordFilter
    {
        public WordFilter(string Word, string Replacement, bool Strict, bool Bannable)
        {
            this.Word = Word;
            this.Replacement = Replacement;
            IsStrict = Strict;
            IsBannable = Bannable;
        }

        public string Word { get; }

        public string Replacement { get; }

        public bool IsStrict { get; }

        public bool IsBannable { get; }
    }
}