namespace Oblivion.HabboHotel.Catalog.Pets
{
    public class PetRace
    {
        public bool _hasPrimaryColour;
        public bool _hasSecondaryColour;

        public PetRace(int RaceId, int PrimaryColour, int SecondaryColour, bool HasPrimaryColour,
            bool HasSecondaryColour)
        {
            this.RaceId = RaceId;
            this.PrimaryColour = PrimaryColour;
            this.SecondaryColour = SecondaryColour;
            _hasPrimaryColour = HasPrimaryColour;
            _hasSecondaryColour = HasSecondaryColour;
        }

        public int RaceId { get; set; }

        public int PrimaryColour { get; set; }

        public int SecondaryColour { get; set; }

        public bool HasPrimaryColour
        {
            get { return _hasPrimaryColour; }
            set { _hasPrimaryColour = value; }
        }

        public bool HasSecondaryColour
        {
            get { return _hasSecondaryColour; }
            set { _hasSecondaryColour = value; }
        }
    }
}