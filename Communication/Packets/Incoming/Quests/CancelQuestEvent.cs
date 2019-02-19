#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Quests
{
    internal class CancelQuestEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
            => OblivionServer.GetGame().GetQuestManager().CancelQuest(Session, Packet);
    }
}