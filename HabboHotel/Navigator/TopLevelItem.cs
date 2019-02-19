namespace Oblivion.HabboHotel.Navigator
{
    public class TopLevelItem
    {
        public TopLevelItem(int Id, string SearchCode, string Filter, string Localization)
        {
            this.Id = Id;
            this.SearchCode = SearchCode;
            this.Filter = Filter;
            this.Localization = Localization;
        }

        public int Id { get; set; }

        public string SearchCode { get; set; }

        public string Filter { get; set; }

        public string Localization { get; set; }
    }
}