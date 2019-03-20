#region
using System.Linq;
using Oblivion.HabboHotel.Users;
using Oblivion.HabboHotel.Users.Badges;
#endregion

namespace Oblivion.Communication.Packets.Outgoing.Users
{
    class HabboUserBadgesComposer : ServerPacket
    {
        public HabboUserBadgesComposer(Habbo Habbo)
            : base(ServerPacketHeader.HabboUserBadgesMessageComposer)
        {
            WriteInteger(Habbo.Id);
            WriteInteger(Habbo.GetBadgeComponent().EquippedCount);

            foreach (var Badge in Habbo.GetBadgeComponent().GetBadges().ToList())
            {
                if (Badge.Slot <= 0)
                    continue;

                WriteInteger(Badge.Slot);
                WriteString(Badge.Code);
            }
        }
    }
}