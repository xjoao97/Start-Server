#region

using System;

#endregion

namespace Oblivion.HabboHotel.Cache
{
    public class UserCache
    {
        public UserCache(int Id, string Username, string Motto, string Look)
        {
            this.Id = Id;
            this.Username = Username;
            this.Motto = Motto;
            this.Look = Look;
            AddedTime = DateTime.Now;
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Motto { get; set; }
        public string Look { get; set; }
        public DateTime AddedTime { get; set; }

        public bool isExpired()
        {
            var CacheTime = DateTime.Now - AddedTime;
            return CacheTime.TotalMinutes >= 30;
        }
    }
}