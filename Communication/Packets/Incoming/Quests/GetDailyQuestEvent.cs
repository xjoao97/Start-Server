#region

using Oblivion.Communication.Packets.Outgoing.LandingView;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Quests
{
    internal class GetDailyQuestEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var UsersOnline = OblivionServer.GetGame().GetClientManager().Count;

            Session.SendMessage(new ConcurrentUsersGoalProgressComposer(UsersOnline));
        }
    }
}