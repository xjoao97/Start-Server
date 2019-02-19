#region

using System;

#endregion

namespace Oblivion.HabboHotel.Rooms
{
    public class RoomPromotion
    {
        public RoomPromotion(string Name, string Desc, int CategoryId)
        {
            this.Name = Name;
            Description = Desc;
            TimestampStarted = OblivionServer.GetUnixTimestamp();
            TimestampExpires = OblivionServer.GetUnixTimestamp() + OblivionStaticGameSettings.RoomPromotionLifeTime * 60;
            this.CategoryId = CategoryId;
        }

        public RoomPromotion(string Name, string Desc, double Started, double Expires, int CategoryId)
        {
            this.Name = Name;
            Description = Desc;
            TimestampStarted = Started;
            TimestampExpires = Expires;
            this.CategoryId = CategoryId;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public double TimestampStarted { get; }

        public double TimestampExpires { get; set; }

        public bool HasExpired => TimestampExpires - OblivionServer.GetUnixTimestamp() < 0;

        public int MinutesLeft
            => Convert.ToInt32(Math.Ceiling((TimestampExpires - OblivionServer.GetUnixTimestamp()) / 60));

        public int CategoryId { get; set; }
    }
}