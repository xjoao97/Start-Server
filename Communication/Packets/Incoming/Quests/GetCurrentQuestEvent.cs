#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Quests
{
    internal class GetCurrentQuestEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
            => OblivionServer.GetGame().GetQuestManager().GetCurrentQuest(Session, Packet);
    }
}