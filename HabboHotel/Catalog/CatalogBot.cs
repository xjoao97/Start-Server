namespace Oblivion.HabboHotel.Catalog
{
    public class CatalogBot
    {
        public string AIType;
        public string Figure;
        public string Gender;
        public int Id;
        public string Motto;
        public string Name;

        public CatalogBot(int Id, string Name, string Figure, string Motto, string Gender, string AIType)
        {
            this.Id = Id;
            this.Name = Name;
            this.Figure = Figure;
            this.Motto = Motto;
            this.Gender = Gender;
            this.AIType = AIType;
        }
    }
}