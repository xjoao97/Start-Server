#region

using System.Linq;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Users
{
    internal class HabboUserBadgesComposer : ServerPacket
    {
        public HabboUserBadgesComposer(Habbo Habbo)
            : base(ServerPacketHeader.HabboUserBadgesMessageComposer)
        {
            WriteInteger(Habbo.Id);
            WriteInteger(Habbo.GetBadgeComponent().EquippedCount);

            foreach (var Badge in Habbo.GetBadgeComponent().GetBadges().ToList().Where(Badge => Badge.Slot > 0))
            {
                WriteInteger(Badge.Slot);
                WriteString(Badge.Code);
            }
        }
    }
}