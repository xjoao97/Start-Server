#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Inventory.Achievements;
using Oblivion.HabboHotel.Achievements;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.Achievements
{
    internal class GetAchievementsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
            => Session.SendMessage(new AchievementsComposer(Session,
                OblivionServer.GetGame().GetAchievementManager()._achievements.Values.Cast<Achievement>().ToList()));
    }
}