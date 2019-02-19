namespace Oblivion.HabboHotel.Users.Navigator.SavedSearches
{
    public class SavedSearch
    {
        public SavedSearch(int Id, string Filter, string Search)
        {
            this.Id = Id;
            this.Filter = Filter;
            this.Search = Search;
        }

        public int Id { get; set; }

        public string Filter { get; set; }

        public string Search { get; set; }
    }
}