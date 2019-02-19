#region

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Oblivion.HabboHotel.Achievements;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Inventory.Achievements
{
    internal class BadgeDefinitionsComposer : ServerPacket
    {
        public BadgeDefinitionsComposer(HybridDictionary Achievements)
            : base(ServerPacketHeader.BadgeDefinitionsMessageComposer)
        {
            WriteInteger(Achievements.Count);

            foreach (var Achievement in Achievements.Values.Cast<Achievement>())
            {
                WriteString(Achievement.GroupName.Replace("ACH_", ""));
                WriteInteger(Achievement.Levels.Count);
                foreach (var Level in Achievement.Levels.Values)
                {
                    WriteInteger(Level.Level);
                    WriteInteger(Level.Requirement);
                }
            }
        }
    }
}