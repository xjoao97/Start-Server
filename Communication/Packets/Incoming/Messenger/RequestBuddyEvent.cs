#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Quests;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class RequestBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            if (Session.GetHabbo().GetMessenger().RequestBuddy(Packet.PopString()))
                OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_FRIEND);
        }
    }
}