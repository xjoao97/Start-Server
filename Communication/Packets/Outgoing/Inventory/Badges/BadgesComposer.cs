﻿#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Users.Badges;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Inventory.Badges
{
    internal class BadgesComposer : ServerPacket
    {
        public BadgesComposer(GameClient Session)
            : base(ServerPacketHeader.BadgesMessageComposer)
        {
            var EquippedBadges = new List<Badge>();

            WriteInteger(Session.GetHabbo().GetBadgeComponent().Count);
            foreach (var Badge in Session.GetHabbo().GetBadgeComponent().GetBadges().ToList())
            {
                WriteInteger(1);
                WriteString(Badge.Code);

                if (Badge.Slot > 0)
                    EquippedBadges.Add(Badge);
            }

            WriteInteger(EquippedBadges.Count);
            foreach (var Badge in EquippedBadges)
            {
                WriteInteger(Badge.Slot);
                WriteString(Badge.Code);
            }
        }
    }
}