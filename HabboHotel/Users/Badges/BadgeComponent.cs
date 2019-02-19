#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Inventory.Badges;
using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Users.UserDataManagement;

#endregion

namespace Oblivion.HabboHotel.Users.Badges
{
    public class BadgeComponent
    {
        private readonly Dictionary<string, Badge> _badges;
        private readonly Habbo _player;

        public BadgeComponent(Habbo Player, UserData data)
        {
            _player = Player;
            _badges = new Dictionary<string, Badge>();

            foreach (var badge in data.badges.Where(badge => !_badges.ContainsKey(badge.Code)))
                _badges.Add(badge.Code, badge);
        }

        public int Count => _badges.Count;

        public int EquippedCount => _badges.Values.Count(Badge => Badge.Slot > 0);

        public ICollection<Badge> GetBadges() => _badges.Values;

        public Badge GetBadge(string Badge) => _badges.ContainsKey(Badge) ? _badges[Badge] : null;

        public bool TryGetBadge(string BadgeCode, out Badge Badge) => _badges.TryGetValue(BadgeCode, out Badge);

        public bool HasBadge(string Badge) => _badges.ContainsKey(Badge);

        public void GiveBadge(string Badge, bool InDatabase, GameClient Session)
        {
            if (string.IsNullOrEmpty(Badge) || Badge == "badge") //hardcoded, why not?
                return;
            if (HasBadge(Badge))
                return;

            if (InDatabase)
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("INSERT INTO user_badges (user_id,badge_id,badge_slot) VALUES (" + _player.Id +
                                      ",@badge," + 0 + ")");
                    dbClient.AddParameter("badge", Badge);
                    dbClient.RunQuery();
                }

            _badges.Add(Badge, new Badge(Badge, 0));

            Session?.SendMessage(new BadgesComposer(Session));
            Session?.SendMessage(new FurniListNotificationComposer(1, 4));
        }

        public void ResetSlots()
        {
            foreach (var Badge in _badges.Values)
                Badge.Slot = 0;
        }

        public void RemoveBadge(string Badge)
        {
            if (!HasBadge(Badge))
                return;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM user_badges WHERE badge_id = @badge AND user_id = " + _player.Id +
                                  " LIMIT 1");
                dbClient.AddParameter("badge", Badge);
                dbClient.RunQuery();
            }

            if (_badges.ContainsKey(Badge))
                _badges.Remove(Badge);
        }
    }
}